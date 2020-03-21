using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace GH.MiscToolbox.Components
{
    public class EvaluateDomainComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the EvaluateIntervalComponent class.
        /// </summary>
        public EvaluateDomainComponent()
          : base("Evaluate Domain", "EvalDom",
              "Description",
              "MiscToolbox", "Data")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddIntervalParameter("Domain", "D", "Domain to evaluate from", GH_ParamAccess.item);
            pManager.AddNumberParameter("t Parameter", "t", "Parameter to evaluate domain with. This is between [0, 1]", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("Value", "V", "Value from evaluation", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Interval interval = new Interval();
            if (!DA.GetData(0, ref interval))
                return;
            double t = 0;
            if (!DA.GetData(1, ref t))
                return;

            DA.SetData(0, interval.ParameterAt(t));
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
            get { return new Guid("2762b400-4f17-4c75-9e11-54a54ea086bc"); }
        }
    }
}