using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;

namespace MiscToolbox.Components.Numerical
{
    public class ValueStatsComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the ValueStatsComponent class.
        /// </summary>
        public ValueStatsComponent()
          : base("Value Stats", "ValStats",
              "Get min, max, average value",
              "MiscToolbox", "Numerical")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Value", "V", "Values to analyse", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("Min", "m", "Min value", GH_ParamAccess.item);
            pManager.AddNumberParameter("Max", "M", "Max value", GH_ParamAccess.item);
            pManager.AddNumberParameter("Average", "A", "Average value", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var list = new List<double>();
            if (!DA.GetDataList(0, list))
                return;

            DA.SetData(0, list.Min());
            DA.SetData(1, list.Max());
            DA.SetData(2, list.Average());
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
            get { return new Guid("f9f846ce-12c7-4cd2-8869-51b28736d6d5"); }
        }
    }
}