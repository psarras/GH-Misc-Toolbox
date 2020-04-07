using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace GH.MiscToolbox.Components
{
    public class ConstrainComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the ConstrainComponent class.
        /// </summary>
        public ConstrainComponent()
          : base("Constrain", "Con",
              "Constrain values between two numbers",
              "MiscToolbox", "Numerical")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Value", "V", "Values to constrain", GH_ParamAccess.list);
            pManager.AddNumberParameter("Min", "m", "Values to constrain", GH_ParamAccess.item);
            pManager.AddNumberParameter("Max", "M", "Values to constrain", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("Value", "V", "Constrained values", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var values = new List<double>();
            if (!DA.GetDataList(0, values))
                return;

            var min = 0.0;
            if (!DA.GetData(1, ref min))
                return;

            var max = 0.0;
            if (!DA.GetData(2, ref max))
                return;

            values = values.Select(x => x < min ? min : x > max ? max : x).ToList();

            DA.SetDataList(0, values);

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
            get { return new Guid("8de7f80b-53df-452a-ab48-08b88eb707cf"); }
        }
    }
}