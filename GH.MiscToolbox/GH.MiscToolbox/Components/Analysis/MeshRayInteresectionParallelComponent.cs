using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace GH.MiscToolbox.Components.Analysis
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
            pManager.AddPointParameter("Point", "P", "Point to start ray from", GH_ParamAccess.list);
            pManager.AddVectorParameter("Normal", "N", "Normal to use for orienting the Vectors", GH_ParamAccess.list); // This can be a plane as well or a vector assuming a plane with normal plane
            pManager.AddVectorParameter("Vectors", "V", "Vectors with Y Axis being forward, those will be oriented to match the normal of the samples", GH_ParamAccess.list);
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
            if (!DA.GetDataList(1, Mt))
                return;
            List<Point3d> points = new List<Point3d>();
            if (!DA.GetDataList(2, points))
                return;
            List<Vector3d> normals = new List<Vector3d>();
            if (!DA.GetDataList(3, normals))
                return;
            List<Vector3d> vectors = new List<Vector3d>();
            if (!DA.GetDataList(4, vectors))
                return;
            bool run = false;
            if (!DA.GetData(5, ref run))
                return;


            if (!run)
                return;

            List<int> hits = new List<int>();
            var jobs = new List<Tuple<int, int, Ray3d, Mesh, int[]>>();

            var context = new Mesh();
            M.ForEach(x => context.Append(x));

            if (points.Count != normals.Count)
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

            var origin = Plane.WorldXY;// new Plane(new Point3d(), Vector3d.YAxis);
            for (int i = 0; i < points.Count; i++)
            {
                // Assume normal Plane
                var xaxis = Vector3d.CrossProduct(Vector3d.ZAxis, normals[i]);
                var plane = new Plane(points[i], xaxis, normals[i]);
                // Orient Vectors Around Normal
                var transformation = Transform.PlaneToPlane(origin, plane);
                //var transformation = Transform.Rotation(0.1, Vector3d.ZAxis, new Point3d());
                var copy = vectors.Select(x => (Point3d)x).ToList();
                //copy.ForEach(x => x.Transform(transformation));

                for (int j = 0; j < copy.Count; j++)
                {
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

            //for (int i = 0; i < points.Count; i++)
            //{
            //    for (int j = 0; j < length; j++)
            //    {

            //    }
            //}


            DA.SetDataTree(0, targetIndexData.ToTree());

            if (raysDebug)
            {
                var lines = pointData.Select((x, i) => x.Select((y, j) => hitData[i][j] ? new Line(points[i], y) : Line.Unset).ToArray()).ToArray();
                DA.SetDataTree(1, lines.ToTree());

            }


            if (distDebug)
            {
                DA.SetDataTree(2, distData.ToTree());
            }

            if (hitsDebug)
            {
                DA.SetDataTree(3, hitData.ToTree());
            }

            DA.SetDataTree(4, vectorsTransformed.ToTree());

        }

        double[][] distData;
        Point3d[][] pointData;
        bool[][] hitData;
        int[][] targetIndexData;

        private bool raysDebug = true;
        private bool distDebug = true;
        private bool hitsDebug = true;

        public void RunJob(Tuple<int, int, Ray3d, Mesh, int[]> task)
        {
            int[] indeces;
            double d = Rhino.Geometry.Intersect.Intersection.MeshRay(task.Item4, task.Item3, out indeces);

            var targetHit = false;
            targetIndexData[task.Item1][task.Item2] = -2; // Missed
            if (indeces != null && indeces.Length > 0)
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
            get { return new Guid("7fcc07a9-39e1-4d86-b51b-40c8f24f5f05"); }
        }
    }
}