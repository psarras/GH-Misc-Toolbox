using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using GH_IO.Serialization;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using MiscToolbox.Extensions;
using Rhino.Geometry;

namespace MiscToolbox.Components.Analysis
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
            pManager.AddMeshParameter("Target Mesh", "Mt", "Mesh for a target to check against", GH_ParamAccess.list);
            pManager.AddPointParameter("Point", "P", "Point to start ray from", GH_ParamAccess.list);
            pManager.AddVectorParameter("Normal", "N", "Normal to use for the sample", GH_ParamAccess.list);
            pManager[3].Optional = true;
            pManager.AddPointParameter("Target", "T", "Points representing the target", GH_ParamAccess.tree);
            pManager.AddNumberParameter("Angle", "A", "Angle threshold in radians", GH_ParamAccess.item, -1);
            pManager[5].Optional = true;
            pManager.AddBooleanParameter("Run", "R", "Run analysis", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("Intersections", "I", "percentage of target points each panel hits",
                GH_ParamAccess.tree);
            pManager.AddLineParameter("Ray", "R", "Ray hits, used for debugging", GH_ParamAccess.tree);
            pManager.AddNumberParameter("Distance", "D", "Distance to each hit, used for debugging",
                GH_ParamAccess.tree);
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
            List<Vector3d> normals = new List<Vector3d>();
            DA.GetDataList(3, normals);
            GH_Structure<GH_Point> targets = new GH_Structure<GH_Point>();
            if (!DA.GetDataTree(4, out targets))
                return;
            angle = -1;
            if (!DA.GetData(5, ref angle))
                return;
            bool run = false;
            if (!DA.GetData(6, ref run))
                return;
            if (!run)
                return;


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

            bool checkNormals = normals != null && normals.Count == points.Count && angle > 0;

            for (int k = 0; k < Mt.Count; k++)
            {
                List<Tuple<int, int, Ray3d, Mesh, int[], int>> jobs = new List<Tuple<int, int, Ray3d, Mesh, int[], int>>();
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
                        Vector3d direction = targets[k][j].Value - points[i];
                        bool cast = true;
                        if (checkNormals)
                        {
                            double a = Vector3d.VectorAngle(direction, normals[i]);
                            cast = a < angle;
                        }

                        Ray3d ray = new Ray3d(points[i], direction);
                        if (cast)
                            jobs.Add(new Tuple<int, int, Ray3d, Mesh, int[], int>(i, j, ray, context, faceBarriers,
                                k + 1));
                    }
                }

                System.Threading.Tasks.Parallel.ForEach(jobs, x => RunJob(x));
                var pointIntersections = hitData.Select(x => Average(x));
                if (distDebug)
                    distDataResults[k] = distData;

                hitDataResults[k] = hitData;

                if (raysDebug)
                    pointDataResults[k] = pointData;

                results.Merge(pointIntersections.ToList());
            }

            DA.SetDataTree(0, results.ToTree());


            if (raysDebug)
            {
                var lines = pointDataResults.Select((x, i) =>
                    x.Select((y, j) => y.Select((z, k) => new Line(points[j], z)).ToArray()).ToArray()).ToArray();
                DA.SetDataTree(1, lines.ToTree<Line>());
            }


            if (distDebug)
                DA.SetDataTree(2, distDataResults.ToTree<double>());

            if (hitsDebug)
                DA.SetDataTree(3, hitDataResults.ToTree<bool>());
        }

        double[][] distData;
        Point3d[][] pointData;
        bool[][] hitData;
        private bool raysDebug = false;
        private bool distDebug = false;
        private bool hitsDebug = false;

        private double angle;

        public static double Average(bool[] data)
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
                var faceIDLow = task.Item5[task.Item6 - 1];
                if (index < faceID && index >= faceIDLow)
                {
                    targetHit = true;
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
            ToolStripMenuItem item1 = Menu_AppendItem(menu, "Rays Debug", Menu_Rays, true, raysDebug);
            ToolStripMenuItem item2 = Menu_AppendItem(menu, "Distances", Menu_Dist, true, distDebug);
            ToolStripMenuItem item3 = Menu_AppendItem(menu, "Hits", Menu_Hits, true, hitsDebug);
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
            writer.SetBoolean("hitsDebug", hitsDebug);
            writer.SetBoolean("distDebug", distDebug);
            writer.SetBoolean("raysDebug", raysDebug);

            return base.Write(writer);
        }

        public override bool Read(GH_IReader reader)
        {
            hitsDebug = reader.GetBoolean("hitsDebug");
            distDebug = reader.GetBoolean("distDebug");
            raysDebug = reader.GetBoolean("raysDebug");
            return base.Read(reader);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => null;

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("19216157-e943-49c7-a4b8-901fe1799404"); }
        }
    }
}