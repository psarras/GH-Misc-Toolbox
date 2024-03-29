﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using GH_IO.Serialization;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using MiscToolbox.Extensions;
using Rhino.Geometry;

namespace MiscToolbox.Components.Analysis
{
    public class MeshRayInteresectionParallelComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MeshRayInteresectionParallelComponent class.
        /// </summary>
        public MeshRayInteresectionParallelComponent()
          : base("MeshRay Interesection Parallel", "MeshRay",
              "Run an analysis agains a mesh from Rays",
              "MiscToolbox", "Analysis")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "Context mesh that can obstruct target", GH_ParamAccess.list);
            pManager.AddMeshParameter("Target Mesh", "Mt", "Mesh for a target to check against", GH_ParamAccess.list);
            pManager[1].Optional = true;
            pManager.AddPointParameter("Point", "P", "Point to start ray from", GH_ParamAccess.list);
            pManager.AddGenericParameter("Normal", "N", "This can be either a Vector or a Plane. Used for orienting the Vectors", GH_ParamAccess.list);
            pManager.AddVectorParameter("Vectors", "V", "Vectors with Y Axis being forward, those will be oriented to match the normal of the samples", GH_ParamAccess.list);
            pManager.AddNumberParameter("Weights", "W", "Optional weight per Vector, the sum of the succesfull hits per Target would be outputed", GH_ParamAccess.list);
            pManager[5].Optional = true;
            pManager.AddNumberParameter("Distance", "D", "Distance threshold. More than that and the Hit is acounted. Leave negative for infinite distance", GH_ParamAccess.item, -1);
            pManager.AddBooleanParameter("Run", "R", "Run analysis", GH_ParamAccess.item, true);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddIntegerParameter("Results", "R", "Total Hit on each Target, The first two  slots are reserved for miss, context and then goes each Mesh in Mt", GH_ParamAccess.tree);
            pManager.AddNumberParameter("Values", "V", "Total Values based on weights per Target", GH_ParamAccess.tree);
            pManager.AddNumberParameter("Intersections", "I", "percentage of target points each panel hits", GH_ParamAccess.tree);
            pManager.AddLineParameter("Ray", "R", "Ray hits, used for debugging", GH_ParamAccess.tree);
            pManager.AddNumberParameter("Distance", "D", "Distance to each hit, used for debugging", GH_ParamAccess.tree);
            pManager.AddBooleanParameter("Hit", "H", "If you hit the target, used for debugging", GH_ParamAccess.tree);
            pManager.AddVectorParameter("Vectors", "V", "Transformed Vectors from Point/Normal Pair", GH_ParamAccess.tree);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Mesh> M = new List<Mesh>();
            if (!DA.GetDataList(0, M))
                return;
            List<Mesh> Mt = new List<Mesh>();
            DA.GetDataList(1, Mt);

            List<Point3d> points = new List<Point3d>();
            if (!DA.GetDataList(2, points))
                return;
            var normalInput = new List<GH_ObjectWrapper>();
            if (!DA.GetDataList(3, normalInput))
                return;

            var sample = normalInput.First().Value;
            List<Plane> planeNormals = new List<Plane>();
            List<Vector3d> vectorNormals = new List<Vector3d>();
            bool isPlane = false;
            if (sample is GH_Plane)
            {
                isPlane = true;
                foreach (var p in normalInput)
                {
                    if (p.Value is GH_Plane plane)
                        planeNormals.Add(plane.Value);
                }
            }
            else if(sample is GH_Vector)
            {
                foreach (var v in normalInput)
                {
                    if (v.Value is GH_Vector vector3d)
                        vectorNormals.Add(vector3d.Value);
                }
            }
            else
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "You must provide either planes of vectors for normals");
                return;
            }

            List<Vector3d> vectors = new List<Vector3d>();
            if (!DA.GetDataList(4, vectors))
                return;
            List<double> weights = new List<double>();
            DA.GetDataList(5, weights);
            threshold = -1;
            if (!DA.GetData(6, ref threshold))
                return;
            IncludeThreshold = threshold > 0;

            bool run = false;
            if (!DA.GetData(7, ref run))
                return;


            if (!run)
                return;

            List<int> hits = new List<int>();
            var jobs = new List<Tuple<int, int, Ray3d, Mesh, int[]>>();

            var context = new Mesh();
            M.ForEach(x => context.Append(x));

            if (points.Count != (isPlane ? planeNormals.Count : vectorNormals.Count))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Provide equal amount of points and vectors");
                return;
            }

            int[] faceBarriers = new int[Mt.Count + 1];

            faceBarriers[0] = context.Faces.Count;
            for (int i = 0; i < Mt.Count; i++)
            {
                faceBarriers[i + 1] = faceBarriers[i] + Mt[i].Faces.Count;
            }

            faceBarriers.Where(x => x == 10).Select((x, i) => i);
            Mt.ForEach(x => context.Append(x));


            jobs = new List<Tuple<int, int, Ray3d, Mesh, int[]>>();
            distData = new double[points.Count][];
            pointData = new Point3d[points.Count][];
            hitData = new bool[points.Count][];
            targetIndexData = new int[points.Count][];
            var vectorsTransformed = new Vector3d[points.Count][];

            for (int i = 0; i < points.Count; i++)
            {
                distData[i] = new double[vectors.Count];
                pointData[i] = new Point3d[vectors.Count];
                hitData[i] = new bool[vectors.Count];
                targetIndexData[i] = new int[vectors.Count];
                vectorsTransformed[i] = new Vector3d[vectors.Count];
            }

            var origin = Plane.WorldXY;
            for (int i = 0; i < points.Count; i++)
            {
                Plane plane = Plane.Unset;
                if(!isPlane)
                {
                    // Assume normal Plane
                    var xaxis = Vector3d.CrossProduct(Vector3d.ZAxis, vectorNormals[i]);
                    plane = new Plane(points[i], xaxis, vectorNormals[i]);
                }
                else
                {
                    plane = planeNormals[i];
                }

                // Orient Vectors Around Normal
                var transformation = Transform.PlaneToPlane(origin, plane);
                var copy = vectors.Select(x => (Point3d)x).ToList();

                for (int j = 0; j < copy.Count; j++)
                {
                    // Couldn't make it work with transforming vectors...
                    var line = new Line(new Point3d(), new Vector3d(copy[j]), 2);
                    line.Transform(transformation);
                    copy[j] = line.To;
                }

                vectorsTransformed[i] = copy.Select(x => x - points[i]).ToArray();

                for (int j = 0; j < copy.Count; j++)
                {
                    //  Cast Ray
                    Ray3d ray = new Ray3d(points[i], (Vector3d)vectorsTransformed[i][j]);
                    jobs.Add(new Tuple<int, int, Ray3d, Mesh, int[]>(i, j, ray, context, faceBarriers));
                }
            }

            System.Threading.Tasks.Parallel.ForEach(jobs, x => RunJob(x));
            int[][] cumulativeResults = new int[points.Count][];
            for (int i = 0; i < points.Count; i++)
            {
                int[] cumulativeAnswers = new int[Mt.Count + 2];
                for (int j = 0; j < vectors.Count; j++)
                {
                    cumulativeAnswers[targetIndexData[i][j] + 2]++;
                }
                cumulativeResults[i] = cumulativeAnswers;
            }

            if (weights.Count > 0)
            {
                double[][] values = new double[points.Count][];
                for (int i = 0; i < points.Count; i++)
                {
                    double[] cumulativeValues = new double[Mt.Count + 2];

                    for (int j = 0; j < vectors.Count; j++)
                    {
                        cumulativeValues[targetIndexData[i][j] + 2] += weights[j];
                    }
                    values[i] = cumulativeValues;
                }
                DA.SetDataTree(1, values.ToTree());
            }

            DA.SetDataTree(0, cumulativeResults.ToTree());

            if (IndexDebug)
                DA.SetDataTree(2, targetIndexData.ToTree());

            if (raysDebug)
            {
                var lines = pointData.Select((x, i) => x.Select((y, j) => hitData[i][j] ? new Line(points[i], y) : Line.Unset).ToArray()).ToArray();
                DA.SetDataTree(3, lines.ToTree());
            }


            if (distDebug)
                DA.SetDataTree(4, distData.ToTree());

            if (hitsDebug)
                DA.SetDataTree(5, hitData.ToTree());

            if (vectorsDebug)
                DA.SetDataTree(6, vectorsTransformed.ToTree());



        }

        double[][] distData;
        Point3d[][] pointData;
        bool[][] hitData;
        int[][] targetIndexData;

        private double threshold = 0;
        private bool IncludeThreshold = false;

        private bool raysDebug = false;
        private bool distDebug = false;
        private bool hitsDebug = false;

        private bool IndexDebug = false;
        private bool vectorsDebug = false;

        public void RunJob(Tuple<int, int, Ray3d, Mesh, int[]> task)
        {
            int[] indeces;
            double d = Rhino.Geometry.Intersect.Intersection.MeshRay(task.Item4, task.Item3, out indeces);

            var targetHit = false;
            targetIndexData[task.Item1][task.Item2] = -2; // Missed
            if (indeces != null && indeces.Length > 0 && (!IncludeThreshold || d < threshold))
            {
                var index = indeces[0];

                for (int i = 0; i < task.Item5.Length; i++)
                {
                    var faceID = task.Item5[i];
                    if (index <= faceID)
                    {
                        targetIndexData[task.Item1][task.Item2] = i - 1;
                        targetHit = true;
                        break;
                    }
                }
            }

            if (distDebug)
                distData[task.Item1][task.Item2] = d;

            if (raysDebug)
            {
                if (d >= 0)
                    pointData[task.Item1][task.Item2] = task.Item3.PointAt(d);
                else
                    pointData[task.Item1][task.Item2] = Point3d.Unset;
            }

            hitData[task.Item1][task.Item2] = d >= 0 && targetHit;
        }

        public override void AppendAdditionalMenuItems(ToolStripDropDown menu)
        {
            base.AppendAdditionalMenuItems(menu);
            ToolStripMenuItem item4 = Menu_AppendItem(menu, "Indeces", Menu_Indices, true, IndexDebug);
            ToolStripMenuItem item1 = Menu_AppendItem(menu, "Rays Debug", Menu_Rays, true, raysDebug);
            ToolStripMenuItem item2 = Menu_AppendItem(menu, "Distances", Menu_Dist, true, distDebug);
            ToolStripMenuItem item3 = Menu_AppendItem(menu, "Hits", Menu_Hits, true, hitsDebug);
            ToolStripMenuItem item5 = Menu_AppendItem(menu, "Vectors", Menu_Vectors, true, vectorsDebug);

        }

        private void Menu_Vectors(object sender, EventArgs e)
        {
            vectorsDebug = !vectorsDebug;
            ExpireSolution(true);
        }

        private void Menu_Indices(object sender, EventArgs e)
        {
            IndexDebug = !IndexDebug;
            ExpireSolution(true);
        }

        private void Menu_Hits(object sender, EventArgs e)
        {
            hitsDebug = !hitsDebug;
            ExpireSolution(true);
        }

        private void Menu_Dist(object sender, EventArgs e)
        {
            distDebug = !distDebug;
            ExpireSolution(true);
        }

        private void Menu_Rays(object sender, EventArgs e)
        {
            raysDebug = !raysDebug;
            ExpireSolution(true);
        }

        public override bool Write(GH_IWriter writer)
        {
            writer.SetBoolean("vectorsDebug", vectorsDebug);
            writer.SetBoolean("IndexDebug", IndexDebug);
            writer.SetBoolean("hitsDebug", hitsDebug);
            writer.SetBoolean("distDebug", distDebug);
            writer.SetBoolean("raysDebug", raysDebug);

            return base.Write(writer);
        }

        public override bool Read(GH_IReader reader)
        {
            vectorsDebug = reader.GetBoolean("vectorsDebug");
            IndexDebug = reader.GetBoolean("IndexDebug");
            hitsDebug = reader.GetBoolean("hitsDebug");
            distDebug = reader.GetBoolean("distDebug");
            raysDebug = reader.GetBoolean("raysDebug");
            return base.Read(reader);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return null;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("7fcc07a9-39e1-4d86-b51b-40c8f24f5f05"); }
        }
    }
}