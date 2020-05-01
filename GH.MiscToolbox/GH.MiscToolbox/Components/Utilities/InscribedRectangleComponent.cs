using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace GH.MiscToolbox.Components.Utilities
{
    public class InscribedRectangleComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the InscribedRectangleComponent class.
        /// </summary>
        public InscribedRectangleComponent()
          : base("Inscribed Rectangle", "InsRect",
              "Description",
              "MiscToolbox", "Utilities")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("Boundary", "B", "Boundary", GH_ParamAccess.item);
            pManager.AddCurveParameter("Curve", "C", "Curve", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddCurveParameter("Curve", "C", "Win Curve", GH_ParamAccess.item);
            pManager.AddNumberParameter("Area", "A", "Area", GH_ParamAccess.item);
            pManager.AddCurveParameter("Polyline", "P", "Polyline", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Curve boundary = null;
            if (!DA.GetData(0, ref boundary))
                return;
            List<Curve> curves = new List<Curve>();
            if (!DA.GetDataList(1, curves))
                return;

            pairs = new Curve[curves.Count];
            areas = new double[curves.Count];
            rects = new Polyline[curves.Count];
            System.Threading.Tasks.Parallel.ForEach(curves.Select((x, i) => new Tuple<Curve, int>(x, i)), y =>
            {
                RunJob(y, boundary);
            });

            //var curvePair = GetLargestRectangle(curves, boundary, out double area, out Polyline polyline);
            DA.SetDataList(0, pairs);
            DA.SetDataList(1, areas);
            DA.SetDataList(2, rects);
        }

        Curve[] pairs;
        Polyline[] rects;
        double[] areas;

        private void RunJob(Tuple<Curve, int> y, Curve boundary)
        {
            pairs[y.Item2] = GetLargestRectangle(y.Item1, boundary, out double area, out Polyline polyline);
            rects[y.Item2] = polyline;
            areas[y.Item2] = area;
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
            get { return new Guid("451060c7-2dce-4df7-ba3b-5677f52af582"); }
        }

        /// <summary>
        /// https://web.archive.org/web/20120410040052/http://blog.csharphelper.com/2010/01/04/calculate-a-polygons-area-in-c.aspx
        /// https://stackoverflow.com/questions/2034540/calculating-area-of-irregular-polygon-in-c-sharp
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
        public double Area(List<Point3d> points)
        {
            var area = Math.Abs(points.Take(points.Count() - 1)
              .Select((p, i) => (points[i + 1].X - p.X) * (points[i + 1].Y + p.Y))
              .Sum() / 2);
            return area;
        }

        public Curve RotateCurve(Curve StartCurve, double angle)
        {
            var nurb = StartCurve.ToNurbsCurve();
            var transform = Transform.Rotation(angle, Vector3d.ZAxis, StartCurve.PointAt(0.5));
            nurb.Transform(transform);
            return nurb;
        }

        public Curve GetLargestRectangle(Curve StartCurve, Curve Boundary, out double area, out Polyline polyline)
        {
            double step = 1 / 180.0 * Math.PI;
            List<Curve> curves = new List<Curve>();
            for (int i = -90; i < 90; i++)
            {
                var c = RotateCurve(StartCurve, step * i);
                Rhino.Geometry.Intersect.CurveIntersections result =
                  Rhino.Geometry.Intersect.Intersection.CurveCurve(Boundary, c, 0.1, 0.1);
                if (!result.Any())
                {
                    curves.Add(c);
                }
            }

            double max = 0;
            Curve best = null;
            polyline = null;
            foreach (Curve curve in curves)
            {
                var points = new List<Point3d>
        { curve.PointAtStart, StartCurve.PointAtStart,
          curve.PointAtEnd, StartCurve.PointAtEnd,
          curve.PointAtStart};
                var temp = Area(points);
                if (temp > max)
                {
                    max = temp;
                    best = curve;
                    polyline = new Polyline(points);
                }
            }
            area = max;
            return best;
        }
    }
}