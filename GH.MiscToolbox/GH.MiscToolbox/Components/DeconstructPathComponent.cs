using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using System.IO;

namespace GH.MiscToolbox.Components
{
    public class DeconstructPathComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the RelativePathComponent class.
        /// </summary>
        public DeconstructPathComponent()
          : base("Deconstruct Path", "DecPath",
              "Deconstruct a file path to its relevant attributes",
              "MiscToolbox", "Path")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Path", "P", "Full or Partial bath", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Drive", "D", "Drive of the path", GH_ParamAccess.item);
            pManager.AddTextParameter("Directory", "D", "Directory of the current path", GH_ParamAccess.item);
            //pManager.AddBooleanParameter("File Exists", "Fe", "If this is a file, does it exist", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Directory Exists", "iD", "If this is a directory", GH_ParamAccess.item);
            pManager.AddTextParameter("Filename", "Fe", "Filename of the file, if it is a file", GH_ParamAccess.item);
            pManager.AddTextParameter("Filename no Extention", "F", "Filename without the extention", GH_ParamAccess.item);
            pManager.AddTextParameter("Extention", "E", "Extention if this is a file", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string path = "";
            if (!DA.GetData(0, ref path))
                return;

            //FileInfo f = new FileInfo(path);
            //var fileValid = f != null;
            //DirectoryInfo d = new DirectoryInfo(path);
            //var dirValid = d != null;

            //if (fileValid || dirValid)
            //{

                DA.SetData(0, Directory.GetDirectoryRoot(path));
                DA.SetData(1, Directory.GetParent(path));
                string extention = Path.GetExtension(path);

                if (!extention.Equals(""))
                {
                    DA.SetData(2, File.Exists(path));
                    DA.SetData(3, Path.GetFileName(path));
                    DA.SetData(4, Path.GetFileNameWithoutExtension(path));
                }
                else
                {
                    DA.SetData(2, Directory.Exists(path));
                }
                DA.SetData(5, extention);
            //}

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
            get { return new Guid("ee082c6e-4522-4994-b2b7-1fe271fccb0b"); }
        }
    }
}