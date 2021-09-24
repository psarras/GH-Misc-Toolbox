using System;
using Grasshopper.Kernel;

namespace MiscToolbox.Components.Numerical
{
    public class RoundValuesComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the RoundValuesComponent class.
        /// </summary>
        public RoundValuesComponent()
          : base("Round Values", "RoundVal",
              "Round Values to significant digits",
              "MiscToolbox", "Numerical")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Value", "V", "Values to round", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Significant Digits", "S", "Number of significant digits", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("Value", "V", "Values rounded", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var value = 0.0;
            if (!DA.GetData(0, ref value))
                return;

            var s = 0;
            if (!DA.GetData(1, ref s))
                return;

            DA.SetData(0, Math.Round(value, s));
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
            get { return new Guid("71dba965-c638-427e-89d8-57b5b556e8b8"); }
        }
    }
}