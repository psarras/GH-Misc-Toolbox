using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace GH.MiscToolbox.Components.Analytics
{
    public class PermutationsComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the PermutationsComponent class.
        /// </summary>
        public PermutationsComponent()
          : base("Permutations", "Perm",
              "Calculate the different permutations",
              "MiscToolbox", "Analytics")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddIntegerParameter("Options", "O", "differnt choices to pick from", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Selection", "S", "number of differnt picks to get", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("Size", "S", "Number of permutations", GH_ParamAccess.item);
            pManager.AddTextParameter("Permutations", "P", "All the different permutations", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            int options = 0;
            if (!DA.GetData(0, ref options))
                return;
            int selection = 0;
            if (!DA.GetData(1, ref selection))
                return;

            
            double combinations = GetNumberOfCombinations(options, selection);

            if(combinations < 999999)
            {
                var permutations = GetKCombs(Enumerable.Range(0, options), selection);
                List<string> perm = permutations.Select(x => string.Join("", x)).ToList();
                DA.SetDataList(1, perm);
            }

            DA.SetData(0, combinations);
        }

        /// <summary>
        /// Permutations with repetition
        /// {1,1} {1,2} {1,3} {1,4} {2,1} {2,2} {2,3} {2,4} {3,1} {3,2} {3,3} {3,4} {4,1} {4,2} {4,3} {4,4}
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        static IEnumerable<IEnumerable<T>> GetPermutationsWithRept<T>(IEnumerable<T> list, int length)
        {
            if (length == 1) return list.Select(t => new T[] { t });
            return GetPermutationsWithRept(list, length - 1)
                .SelectMany(t => list,
                    (t1, t2) => t1.Concat(new T[] { t2 }));
        }

        /// <summary>
        /// K-combinations with repetition
        /// {1,1} {1,2} {1,3} {1,4} {2,2} {2,3} {2,4} {3,3} {3,4} {4,4}
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        static IEnumerable<IEnumerable<T>> GetKCombsWithRept<T>(IEnumerable<T> list, int length) where T : IComparable
        {
            if (length == 1) return list.Select(t => new T[] { t });
            return GetKCombsWithRept(list, length - 1)
                .SelectMany(t => list.Where(o => o.CompareTo(t.Last()) >= 0),
                    (t1, t2) => t1.Concat(new T[] { t2 }));
        }

        /// <summary>
        /// K-combinations
        /// {1,2} {1,3} {1,4} {2,3} {2,4} {3,4}
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        static IEnumerable<IEnumerable<T>> GetKCombs<T>(IEnumerable<T> list, int length) where T : IComparable
        {
            if (length == 1) return list.Select(t => new T[] { t });
            return GetKCombs(list, length - 1)
                .SelectMany(t => list.Where(o => o.CompareTo(t.Last()) > 0),
                    (t1, t2) => t1.Concat(new T[] { t2 }));
        }

        /// <summary>
        /// Permutations
        /// {1,2} {1,3} {1,4} {2,1} {2,3} {2,4} {3,1} {3,2} {3,4} {4,1} {4,2} {4,3}
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        static IEnumerable<IEnumerable<T>> GetPermutations<T>(IEnumerable<T> list, int length)
        {
            if (length == 1) return list.Select(t => new T[] { t });
            return GetPermutations(list, length - 1)
                .SelectMany(t => list.Where(o => !t.Contains(o)),
                    (t1, t2) => t1.Concat(new T[] { t2 }));
        }

        double GetNumberOfCombinations(double listLength, double conbinationSize)
        {
            return Factorial(listLength) /
              (Factorial(conbinationSize) * Factorial(listLength - conbinationSize));
        }

        double Factorial(double num)
        {
            if (num == 0)
                return 1;
            else
                return num * Factorial(num - 1);
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
            get { return new Guid("6e36e205-1f24-4da0-bd96-fbc17d6ae09c"); }
        }
    }
}