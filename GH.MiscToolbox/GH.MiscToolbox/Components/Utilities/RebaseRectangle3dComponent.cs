using System;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace MiscToolbox.Components.Utilities
{
    public class RebaseRectangle3dComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the RebaseRectangle3dComponent class.
        /// </summary>
        public RebaseRectangle3dComponent()
          : base("Rebase Rectangle3d", "RebaseRect",
              "Rebase a rectangle3d by recreating it on their first corner. This makes their width, height domains to always start from zero",
              "MiscToolbox", "Utilities")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddRectangleParameter("Rectangle", "R", "Rebase this Rectangle", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddRectangleParameter("Rectangle", "R", "Rebased Rectangle", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Rectangle3d rectangle3D = Rectangle3d.Unset;
            if (!DA.GetData(0, ref rectangle3D))
                return;

            Plane plane = rectangle3D.Plane;
            plane.Origin = rectangle3D.Corner(0);
            
            DA.SetData(0, new Rectangle3d(plane, rectangle3D.Width, rectangle3D.Height));
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
            get { return new Guid("3be117e8-f351-42c2-a3ca-00a3e1ce4a3c"); }
        }
    }
}