using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace GH.MiscToolbox.Components
{
    public class MassAndOrComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MassAndComponent class.
        /// </summary>
        public MassAndOrComponent()
          : base("MassAndOr", "MassAndOr",
              "Check if all values are True, Or at least one is",
              "MiscToolbox", "Numerical")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("Values", "V", "Values to check", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddBooleanParameter("And", "A", "All are true", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Or", "O", "At least one is true", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<bool> data = new List<bool>();
            if (!DA.GetDataList(0, data))
                return;

            DA.SetData(0, data.All(x => x));
            DA.SetData(1, data.Exists(x => x == true));
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
            get { return new Guid("b65853a2-7b33-46ac-a2cf-9b456a11613d"); }
        }
    }
}