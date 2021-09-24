using System;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;

namespace MiscToolbox.Components.Data
{
    public class GetBranchComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the GetBranchComponent class.
        /// </summary>
        public GetBranchComponent()
          : base("GetBranch", "GetBranch",
              "Get the Branch of a component and also safely wrap the bounds so you will never run out of index.",
              "MiscToolbox", "Data")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Tree", "T", "Tree to get Branch from", GH_ParamAccess.tree);
            pManager.AddIntegerParameter("index", "i", "Branch Index to get", GH_ParamAccess.item, 0);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Branch", "B", "Resulting branch", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            if (!DA.GetDataTree(0, out GH_Structure<IGH_Goo> tree))
                return;
            int index = 0;
            if (!DA.GetData(1, ref index))
                return;

            DA.SetDataList(0, tree.Branches[(index + tree.PathCount) % tree.PathCount]);

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
            get { return new Guid("526cfbd8-0463-4481-8239-d3259d76ea80"); }
        }
    }
}