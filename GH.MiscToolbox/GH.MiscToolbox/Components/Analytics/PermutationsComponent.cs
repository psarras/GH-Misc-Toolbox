using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
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

            switch (permType)
            {
                case PermutationTypes.Permutations:
                    break;
                case PermutationTypes.Permutations_Rep:
                    break;
                case PermutationTypes.kComp_Rep:
                    break;
                case PermutationTypes.kComp:
                    double combinations = GetNumberOfCombinations(options, selection);

                    if (combinations < 999999)
                    {
                        var permutations = GetKCombs(Enumerable.Range(0, options), selection);
                        List<string> perm = permutations.Select(x => string.Join("", x)).ToList();
                        DA.SetDataList(1, perm);
                    }
                    DA.SetData(0, combinations);
                    break;
                default:
                    break;
            }


        }

        public override void AppendAdditionalMenuItems(ToolStripDropDown menu)
        {
            base.AppendAdditionalMenuItems(menu);
            ToolStripMenuItem item1 = Menu_AppendItem(menu, PermutationTypes.kComp.ToString(), Menu_kComp, true, permType == PermutationTypes.kComp);
            ToolStripMenuItem item2 = Menu_AppendItem(menu, PermutationTypes.kComp_Rep.ToString(), Menu_kComp_RepRep, true, permType == PermutationTypes.kComp_Rep);
            ToolStripMenuItem item3 = Menu_AppendItem(menu, PermutationTypes.Permutations.ToString(), Menu_Permutations, true, permType == PermutationTypes.Permutations);
            ToolStripMenuItem item4 = Menu_AppendItem(menu, PermutationTypes.Permutations_Rep.ToString(), Menu_Permutations_Rep, true, permType == PermutationTypes.Permutations_Rep);
        }

        enum PermutationTypes
        {
            kComp,
            kComp_Rep,
            Permutations,
            Permutations_Rep,
        }

        PermutationTypes permType;
        private void Menu_Permutations_Rep(object sender, EventArgs e)
        {
            permType = PermutationTypes.Permutations_Rep;
            this.ExpireSolution(true);
            //this.ClearData();
        }

        private void Menu_Permutations(object sender, EventArgs e)
        {
            permType = PermutationTypes.Permutations;
            this.ExpireSolution(true);

        }

        private void Menu_kComp(object sender, EventArgs e)
        {
            permType = PermutationTypes.kComp;
            //this.ClearData();
            this.ExpireSolution(true);
        }

        private void Menu_kComp_RepRep(object sender, EventArgs e)
        {
            permType = PermutationTypes.kComp_Rep;
            //this.ClearData();
            this.ExpireSolution(true);
        }

        //Make sure you tell the component how to Write / Read the Extra info (Absolute)
        public override bool Write(GH_IO.Serialization.GH_IWriter writer)
        {
            // First add our own field.
            writer.SetInt32("Permutation", (int)permType);
            // Then call the base class implementation.
            return base.Write(writer);
        }

        public override bool Read(GH_IO.Serialization.GH_IReader reader)
        {
            // First read our own field.
            int val = 0;
            if(reader.TryGetInt32("Permutation", ref val))
            {
                permType = (PermutationTypes)reader.GetInt32("Permutation");
            }
            // Then call the base class implementation.
            return base.Read(reader);
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