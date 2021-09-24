using System;
using Grasshopper.Kernel;

namespace MiscToolbox.Components.Numerical
{
    public class FormatPercentageComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the FormatPercentageComponent class.
        /// </summary>
        public FormatPercentageComponent()
          : base("Format Percentage", "Format_%",
              "format values as a percentage",
              "MiscToolbox", "Data")
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
            pManager.AddTextParameter("Value", "V", "Values rounded", GH_ParamAccess.item);
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

            double v = Math.Round(value * 100, s);
            var st = v.ToString();
            var split = st.Split(new char[] { '.' });
            if (split.Length > 1)
            {
                string v1 = split[1].PadRight(s, '0');
                st = split[0] + "." + v1;
            }

            DA.SetData(0, st + "%");
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
            get { return new Guid("543f0270-24ea-490a-bd19-4105615a6b3d"); }
        }
    }
}