using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MiscToolbox.Extensions
{
    public static class Point3dExtention
    {
        public static Point3d Copy(this Point3d point3D)
        {
            return new Point3d(point3D);
        }

        public static Point3d Average(this IEnumerable<Point3d> point3Ds)
        {
            var v = new Point3d();
            foreach (var p in point3Ds)
            {
                v += p;
            }

            v /= point3Ds.Count();

            return v;
        }

        public static Point3d Normalize(this Point3d point3D)
        {
            return point3D / point3D.DistanceTo(new Point3d());
        }

        public static double AngleBetween(Vector3d a, Vector3d b)
        {
            return a.Angle() - b.Angle();
        }

        public static double Angle(this Vector3d vector3D)
        {
            var a = Vector3d.VectorAngle(Vector3d.YAxis, vector3D);
            if (vector3D.X > 0)
                a = 2 * Math.PI - a;
            return a;
        }



        /// <summary>
        /// https://web.archive.org/web/20120410040052/http://blog.csharphelper.com/2010/01/04/calculate-a-polygons-area-in-c.aspx
        /// https://stackoverflow.com/questions/2034540/calculating-area-of-irregular-polygon-in-c-sharp
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
        public static double Area(this List<Point3d> points)
        {
            var area = Math.Abs(points.Take(points.Count() - 1)
               .Select((p, i) => (points[i + 1].X - p.X) * (points[i + 1].Y + p.Y))
               .Sum() / 2);
            return area;
        }

        public static IEnumerable<int> OldIndicesIfSorted<T>(this IEnumerable<T> source) where T : IComparable<T>
        {
            return source
                .Select((item, index) => new { item, index })
                .OrderBy(a => a.item)
                .Select(a => a.index);
        }


    }
}
