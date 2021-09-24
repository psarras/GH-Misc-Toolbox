using System.Collections.Generic;
using System.Linq;
using Rhino.Geometry;

namespace MiscToolbox.Extensions
{
    public static class GeometryExtentions
    {
        public static Vector3d Average(this IEnumerable<Vector3d> vecs)
        {
            var average = new Vector3d();
            foreach (var v in vecs)
            {
                if (v.IsValid)
                    average += v;
            }
            average /= vecs.Count();
            return average;
        }

        public static List<Vector3d> Remap(this List<Vector3d> vecs, double min, double max, double newMin, double newMax)
        {
            var remapVecs = new List<Vector3d>();
            for (int i = 0; i < vecs.Count; i++)
            {
                var vec = vecs[i];
                remapVecs.Add(vec * vec.Length.Remap(min, max, newMin, newMax));
            }
            return remapVecs;
        }

        public static double Remap(this double value, double min, double max, double newMin, double newMax)
        {
            // (val - min) / (oldDelta)
            // ([0, 1]) * (delta) + min
            return ((value - min) / (max - min)) * (newMax - newMin) + newMin;
        }

    }
}