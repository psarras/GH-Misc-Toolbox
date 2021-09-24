using System;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace MiscToolbox.Components.Numerical
{
    public class LengthDomainComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the LengthDomainComponent class.
        /// </summary>
        public LengthDomainComponent()
          : base("Length Domain", "LenDom",
              "Description",
              "MiscToolbox", "Numerical")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddIntervalParameter("Domain", "D", "Domain to get length from", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("Length", "L", "Length of the Domain", GH_ParamAccess.item);
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

            DA.SetData(0, interval.Length);
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
            get { return new Guid("d4a4e844-fc7f-402e-b10a-3951d7b84349"); }
        }
    }
}