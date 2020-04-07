using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace GH.MiscToolbox.Components
{
    public class IntegerDivisorsComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the IntegerDivisorsComponent class.
        /// </summary>
        public IntegerDivisorsComponent()
          : base("Integer Divisors", "IntDiv",
              "Get all the integer divisors",
              "MiscToolbox", "Numerical")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddIntegerParameter("Value", "V", "Value to analyse for integer divisions", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddIntegerParameter("Divisors", "D", "Integer divisors", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            int value = 0;
            if (!DA.GetData(0, ref value))
                return;

            var divisors = new List<int>();
            for (int i = 1; i <= value; i++)
            {
                if(value % i == 0)
                    divisors.Add(i);
            }

            DA.SetDataList(0, divisors);
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
            get { return new Guid("4722fcd7-8f11-4985-b61d-c68c12d17c1f"); }
        }
    }
}