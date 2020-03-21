using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace GH.MiscToolbox.Components
{
    public class SymmetricalDomainComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the SymmetricalDomainComponent class.
        /// </summary>
        public SymmetricalDomainComponent()
          : base("Symmetrical Domain", "SymDom",
              "Create a symmetrical domain based on the given value",
              "MiscToolbox", "Data")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Value", "V", "Value to use to create a domain", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddIntervalParameter("Domain", "D", "Symmetrical domain", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            double val = 0;
            if (!DA.GetData(0, ref val))
                return;

            DA.SetData(0, new Interval(-val, val));
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
            get { return new Guid("0a994264-9adb-4cb7-ac20-168f3a805ce0"); }
        }
    }
}