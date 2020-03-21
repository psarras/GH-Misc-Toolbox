using System;
using System.Collections.Generic;
using System.IO;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace GH.MiscToolbox.Components
{
    public class RelativePathsComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the RelativePathsComponent class.
        /// </summary>
        public RelativePathsComponent()
          : base("Relative Paths", "RelPaths",
              "Description", "MiscToolbox", "Path")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Update", "U", "Use this to update the component. What you put will be ignored", GH_ParamAccess.tree);
            pManager[0].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Grasshopper", "G", "Grasshopper file relevant Path", GH_ParamAccess.item);
            pManager.AddTextParameter("Rhino", "R", "Rhino file relevant path", GH_ParamAccess.item);
            pManager.AddTextParameter("Current", "C", "Current file path", GH_ParamAccess.item);
            pManager.AddTextParameter("Temp", "T", "Temp path", GH_ParamAccess.item);
            pManager.AddTextParameter("Random Filename", "R", "Generated random file name from system", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            DA.SetData(0, Rhino.RhinoDoc.ActiveDoc.Path);
            DA.SetData(1, OnPingDocument().FilePath);
            DA.SetData(2, Directory.GetCurrentDirectory());
            DA.SetData(3, Path.GetTempPath());
            DA.SetData(4, Path.GetRandomFileName());
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
            get { return new Guid("b452e0ff-7a27-4f2a-8aed-1f1f51a1bb9f"); }
        }
    }
}