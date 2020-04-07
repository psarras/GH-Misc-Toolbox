using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace GH.MiscToolbox.Components
{
    public class CreateEnumerationComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the CreateEnumerationComponent class.
        /// </summary>
        public CreateEnumerationComponent()
          : base("Create Enumeration", "CreateEnum",
              "Create a series of enumeration to match the given list",
              "MiscToolbox", "Data")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Value", "V", "Values to enumerate", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Enumeration", "E", "resulting enumeration", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var values = new List<object>();
            if (!DA.GetDataList(0, values))
                return;

            var count = values.Count.ToString().Length;

            DA.SetDataList(0, Enumerable.Range(0, values.Count).Select(x => x.ToString().PadLeft(count, '0')));

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
            get { return new Guid("0d7f9452-1b1f-492d-b970-049c7b0d1ab3"); }
        }
    }
}