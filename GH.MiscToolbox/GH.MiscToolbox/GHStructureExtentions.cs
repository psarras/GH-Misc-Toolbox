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
        public static DataTree<T> ToTree<T>(this T[][] data)
        {
            var tree = new DataTree<T>();
            
            for (int i = 0; i < data.Length; i++)
            {
                tree.AddRange(data[i], new GH_Path(i));
            }
            return tree;
        }
    }
}
