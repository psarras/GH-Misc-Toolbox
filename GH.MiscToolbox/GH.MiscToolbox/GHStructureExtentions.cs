using Grasshopper;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GH.MiscToolbox
{
    public static class GHStructureExtentions
    {
        public static DataTree<T> ToTree<T>(this IEnumerable<IEnumerable<T>> data)
        {
            var tree = new DataTree<T>();
            int i = 0;
            foreach (var d in data)
            {
                tree.AddRange(d, new GH_Path(i));
                i++;
            }
            return tree;
        }

        public static DataTree<T> ToTree<T>(this IEnumerable<IEnumerable<IEnumerable<T>>> data)
        {
            var tree = new DataTree<T>();
            int j = 0, i = 0;

            foreach (var dd in data)
            {
                j = 0;
                foreach (var d in dd)
                {
                    tree.AddRange(d, new GH_Path(i, j));
                    j++;
                }
                i++;
            }
            return tree;
        }

        public static void Merge<T>(this List<List<T>> data, List<T> extra)
        {
            if (data.Count == 0)
            {
                for (int i = 0; i < extra.Count; i++)
                {
                    data.Add(new List<T> { extra[i] });
                }
                return;
            }

            if (data.Count != extra.Count)
                throw new Exception("Not Same Length");
            for (int i = 0; i < extra.Count; i++)
            {
                data[i].Add(extra[i]);
            }
        }
    }
}
