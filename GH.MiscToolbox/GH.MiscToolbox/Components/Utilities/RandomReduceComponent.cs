using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;

namespace MiscToolbox.Components.Utilities
{
    public class RandomReduceComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the RandomReduceComponent class.
        /// </summary>
        public RandomReduceComponent()
          : base("Random Reduce", "RndRed",
              "Randomly Reduce a list of items, provided a percentage of the total length",
              "MiscToolbox", "Utilities")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Data", "D", "Data to reduce", GH_ParamAccess.list);
            pManager.AddNumberParameter("Percentage", "P", "Percentage of the length of the list to remove", GH_ParamAccess.item, 0.5);
            pManager.AddIntegerParameter("Seed", "S", "Seed to use for randomness", GH_ParamAccess.item, 0);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Data", "D", "Reduced data", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<object> data = new List<object>();
            if (!DA.GetDataList(0, data))
                return;
            double percentage = 0;
            if (!DA.GetData(1, ref percentage))
                return;
            int seed = 0;
            if (!DA.GetData(2, ref seed))
                return;
            int numberOfItems = (int)(percentage * data.Count);

            Random r = new Random(seed);
            var ordered = data.OrderBy(x => r.NextDouble());
            
            DA.SetDataList(0, ordered.Take(numberOfItems));
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
            get { return new Guid("ab74ced0-c7ea-47ec-9e51-422430614f4f"); }
        }
    }
}