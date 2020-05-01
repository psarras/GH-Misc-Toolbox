using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace GH.MiscToolbox.Components.Utilities
{
    public class CenterLineComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the CenterLineComponent class.
        /// </summary>
        public CenterLineComponent()
          : base("Center Direction Line", "CDL",
              "Create a line from a center point and a length",
              "MiscToolbox", "Utilities")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Center", "C", "Center of the Line", GH_ParamAccess.item);
            pManager.AddVectorParameter("Direction", "D", "Direction of the line", GH_ParamAccess.item);
            pManager.AddNumberParameter("Length", "L", "Length of the line", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddLineParameter("Line", "L", "Resulting Line", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Point3d center = Point3d.Unset;
            if (!DA.GetData(0, ref center))
                return;
            Vector3d direction = Vector3d.Unset;
            if (!DA.GetData(1, ref direction))
                return;
            double length = 0;
            if (!DA.GetData(2, ref length))
                return;

            direction.Unitize();
            var start = (Vector3d)center - direction * (length / 2);

            DA.SetData(0, new Line((Point3d)start, direction, length));
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
            get { return new Guid("2aeb31a5-59e8-47c3-a9fb-300a195affa7"); }
        }
    }
}