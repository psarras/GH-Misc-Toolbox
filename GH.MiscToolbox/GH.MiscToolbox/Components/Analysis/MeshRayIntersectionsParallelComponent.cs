using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace GH.MiscToolbox.Components
{
    public class MeshRayIntersectionsParallelComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MeshRayIntersectionsParallelComponent class.
        /// </summary>
        public MeshRayIntersectionsParallelComponent()
          : base("Mesh Ray Intersections Parallel", "MeshRay",
              "Description",
              "MiscToolbox", "Analysis")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "Context mesh that can obstruct target", GH_ParamAccess.item);
            pManager.AddMeshParameter("Target Mesh", "Mt", "Mesh for a target to check against", GH_ParamAccess.item);
            pManager.AddPointParameter("Point", "P", "Point to start ray from", GH_ParamAccess.list);
            pManager.AddPointParameter("Target", "T", "Points representing the target", GH_ParamAccess.list);
            pManager.AddBooleanParameter("Run", "R", "Run analysis", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("Intersections", "I", "percentage of target points each panel hits", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Mesh M = null;
            if (!DA.GetData(0, ref M))
                return;
            Mesh Mt = null;
            if (!DA.GetData(1, ref Mt))
                return;
            List<Point3d> points = new List<Point3d>();
            if (!DA.GetDataList(2, points))
                return;
            List<Point3d> targets = new List<Point3d>();
            if (!DA.GetDataList(3, targets))
                return;
            bool run = false;
            if (!DA.GetData(4, ref run))
                return;


            if (!run)
                return;

            List<int> hits = new List<int>();
            var jobs = new List<Tuple<int, int, Ray3d, Mesh, int>>();

            var faceIndex = M.Faces.Count;

            M.Append(Mt);

            for (int i = 0; i < points.Count; i++)
            {
                for (int j = 0; j < targets.Count; j++)
                {
                    //  Cast Ray
                    Ray3d ray = new Ray3d((Point3d)points[i], targets[j] - points[i]);
                    jobs.Add(new Tuple<int, int, Ray3d, Mesh, int>(i, j, ray, M, faceIndex));
                }
            }

            distData = new double[points.Count][];
            pointData = new Point3d[points.Count][];
            intersectData = new bool[points.Count][];
            for (int i = 0; i < points.Count; i++)
            {
                distData[i] = new double[targets.Count];
                pointData[i] = new Point3d[targets.Count];
                intersectData[i] = new bool[targets.Count];
            }

            System.Threading.Tasks.Parallel.ForEach(jobs, x => RunJob(x));
            var pointIntersections = intersectData.Select(x => Average(x));

            DA.SetDataList(0, pointIntersections);
        }

        double[][] distData;
        Point3d[][] pointData;
        bool[][] intersectData;

        public double Average(bool[] data)
        {

            int count = 0;
            foreach (var d in data)
            {
                count += d ? 1 : 0;
            }
            return (1.0 * count) / data.Length;
        }

        //public void RunJob(Tuple<int, int, Ray3d, Mesh> task)
        //{
        //    double d = Rhino.Geometry.Intersect.Intersection.MeshRay(task.Item4, task.Item3);

        //    distData[task.Item1][task.Item2] = d;
        //    if (d != -1)
        //        pointData[task.Item1][task.Item2] = task.Item3.PointAt(d);
        //    else
        //        pointData[task.Item1][task.Item2] = Point3d.Unset;
        //    intersectData[task.Item1][task.Item2] = d != -1;
        //}

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

            intersectData[task.Item1][task.Item2] = d >= 0 && targetHit;
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