﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace GH.MiscToolbox.Components
{
    public class MeshPointIntersectionParallelComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MeshRayIntersectionsParallelComponent class.
        /// </summary>
        public MeshPointIntersectionParallelComponent()
          : base("Mesh Point Intersections Parallel", "MeshPoint",
              "Run an analysis for meshes for specific target points",
              "MiscToolbox", "Analysis")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "Context mesh that can obstruct target", GH_ParamAccess.list);
            pManager.AddMeshParameter("Target Mesh", "Mt", "Mesh for a target to check against", GH_ParamAccess.list); //List
            pManager.AddPointParameter("Point", "P", "Point to start ray from", GH_ParamAccess.list);
            pManager.AddPointParameter("Target", "T", "Points representing the target", GH_ParamAccess.tree); // Tree
            pManager.AddBooleanParameter("Run", "R", "Run analysis", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("Intersections", "I", "percentage of target points each panel hits", GH_ParamAccess.tree);
            pManager.AddLineParameter("Ray", "R", "Ray hits, used for debugging", GH_ParamAccess.tree);
            pManager.AddNumberParameter("Distance", "D", "Distance to each hit, used for debugging", GH_ParamAccess.tree);
            pManager.AddBooleanParameter("Hit", "H", "If you hit the target, used for debugging", GH_ParamAccess.tree);
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
            if (!DA.GetDataList(1, Mt))
                return;
            List<Point3d> points = new List<Point3d>();
            if (!DA.GetDataList(2, points))
                return;
            GH_Structure<GH_Point> targets = new GH_Structure<GH_Point>();
            if (!DA.GetDataTree(3, out targets))
                return;
            bool run = false;
            if (!DA.GetData(4, ref run))
                return;


            if (!run)
                return;

            List<int> hits = new List<int>();
            var jobs = new List<Tuple<int, int, Ray3d, Mesh, int[], int>>();

            var context = new Mesh();
            M.ForEach(x => context.Append(x));

            var faceIndex = context.Faces.Count;

            if (Mt.Count != targets.PathCount)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Provide equal amount of meshes and targets");
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

            List<List<double>> results = new List<List<double>>();
            double[][][] distDataResults = new double[Mt.Count][][];
            bool[][][] hitDataResults = new bool[Mt.Count][][];
            Point3d[][][] pointDataResults = new Point3d[Mt.Count][][];

            for (int k = 0; k < Mt.Count; k++)
            {
                jobs = new List<Tuple<int, int, Ray3d, Mesh, int[], int>>();
                distData = new double[points.Count][];
                pointData = new Point3d[points.Count][];
                hitData = new bool[points.Count][];

                for (int i = 0; i < points.Count; i++)
                {
                    distData[i] = new double[targets[k].Count];
                    pointData[i] = new Point3d[targets[k].Count];
                    hitData[i] = new bool[targets[k].Count];
                }

                for (int i = 0; i < points.Count; i++)
                {
                    for (int j = 0; j < targets[k].Count; j++)
                    {
                        //  Cast Ray
                        Ray3d ray = new Ray3d(points[i], targets[k][j].Value - points[i]);
                        jobs.Add(new Tuple<int, int, Ray3d, Mesh, int[], int>(i, j, ray, context, faceBarriers, k + 1));
                    }
                }

                System.Threading.Tasks.Parallel.ForEach(jobs, x => RunJob(x));
                var pointIntersections = hitData.Select(x => Average(x));
                distDataResults[k] = distData;
                hitDataResults[k] = hitData;
                pointDataResults[k] = pointData;
                results.Merge(pointIntersections.ToList());
            }

            DA.SetDataTree(0, results.ToTree());


            if (raysDebug)
            {
                var lines = pointDataResults.Select((x, i) => x.Select((y, j) => y.Select((z, k) => new Line(points[j], z)).ToArray()).ToArray()).ToArray();
                DA.SetDataTree(1, lines.ToTree<Line>());
            }


            if (distDebug)
            {
                DA.SetDataTree(2, distDataResults.ToTree<double>());
            }

            if (hitsDebug)
            {
                DA.SetDataTree(3, hitDataResults.ToTree<bool>());
            }


        }

        double[][] distData;
        Point3d[][] pointData;
        bool[][] hitData;
        private bool raysDebug = true;
        private bool distDebug = true;
        private bool hitsDebug = true;

        public double Average(bool[] data)
        {

            int count = 0;
            foreach (var d in data)
            {
                count += d ? 1 : 0;
            }
            return (1.0 * count) / data.Length;
        }

        public void RunJob(Tuple<int, int, Ray3d, Mesh, int> task)
        {
            int[] indeces;
            double d = Rhino.Geometry.Intersect.Intersection.MeshRay(task.Item4, task.Item3, out indeces);

            var targetHit = false;
            if (indeces != null && indeces.Length > 0)
            {
                foreach (var i in indeces)
                {
                    if (i >= task.Item5)
                    {
                        targetHit = true;
                        break;
                    }
                }
            }

            distData[task.Item1][task.Item2] = d;
            if (d >= 0)
                pointData[task.Item1][task.Item2] = task.Item3.PointAt(d);
            else
                pointData[task.Item1][task.Item2] = Point3d.Unset;

            hitData[task.Item1][task.Item2] = d >= 0 && targetHit;
        }

        public void RunJob(Tuple<int, int, Ray3d, Mesh, int[], int> task)
        {
            int[] indeces;
            double d = Rhino.Geometry.Intersect.Intersection.MeshRay(task.Item4, task.Item3, out indeces);

            var targetHit = false;
            if (indeces != null && indeces.Length > 0)
            {
                var index = indeces[0];
                var faceID = task.Item5[task.Item6];
                if (index <= faceID)
                {
                    targetHit = true;
                }
            }

            distData[task.Item1][task.Item2] = d;
            if (d >= 0)
                pointData[task.Item1][task.Item2] = task.Item3.PointAt(d);
            else
                pointData[task.Item1][task.Item2] = Point3d.Unset;

            hitData[task.Item1][task.Item2] = d >= 0 && targetHit;
        }

        public override void AppendAdditionalMenuItems(ToolStripDropDown menu)
        {
            base.AppendAdditionalMenuItems(menu);
            ToolStripMenuItem item1 = Menu_AppendItem(menu, "Rays Debug", Menu_Rays, true, raysDebug);
            ToolStripMenuItem item2 = Menu_AppendItem(menu, "Distances", Menu_dist, true, distDebug);
            ToolStripMenuItem item3 = Menu_AppendItem(menu, "Hits", Menu_hits, true, hitsDebug);

        }

        private void Menu_hits(object sender, EventArgs e)
        {
            hitsDebug = !hitsDebug;
            ExpireSolution(true);
        }

        private void Menu_dist(object sender, EventArgs e)
        {
            distDebug = !distDebug;
            ExpireSolution(true);
        }

        private void Menu_Rays(object sender, EventArgs e)
        {
            raysDebug = !raysDebug;
            ExpireSolution(true);
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
            get { return new Guid("19216157-e943-49c7-a4b8-901fe1799404"); }
        }
    }
}