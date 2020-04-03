using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GH.MiscToolbox
{
    public static class BoundingBoxExtentions
    {
        public static double Width(this BoundingBox boundingBox)
        {
            return boundingBox.Max.X - boundingBox.Min.X;
        }

        public static double Depth(this BoundingBox boundingBox)
        {
            return boundingBox.Max.Y - boundingBox.Min.Y;
        }

        public static double Height(this BoundingBox boundingBox)
        {
            return boundingBox.Max.Z - boundingBox.Min.Z;
        }


        public static Box CenterBox(this Box box)
        {
            var x = box.X.Length / 2;
            var y = box.Y.Length / 2;
            var z = box.Z.Length / 2;

            Box b = new Box(new Plane(box.Center, box.Plane.XAxis, box.Plane.YAxis), new Interval(-x, x), new Interval(-y, y), new Interval(-z, z));
            return b;
        }

    }
}
