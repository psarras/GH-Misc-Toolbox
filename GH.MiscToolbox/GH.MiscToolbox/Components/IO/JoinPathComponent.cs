using System;
using System.Collections.Generic;
using System.IO;
using Grasshopper.Kernel;

namespace MiscToolbox.Components.IO
{
    public class JoinPathComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the JoinPathComponent class.
        /// </summary>
        public JoinPathComponent()
          : base("JoinPathComponent", "JPath",
              "Description",
              "MiscToolbox", "IO")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Paths", "P", "Paths to combine", GH_ParamAccess.list);
            pManager.AddTextParameter("FileName", "F", "Optional Filename to create", GH_ParamAccess.item);
            pManager.AddTextParameter("Extention", "E", "Extention to use for the file", GH_ParamAccess.item);
            pManager[1].Optional = true;
            pManager[2].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Path", "P", "Resulting Path", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var root = new List<string>();
            if (!DA.GetDataList(0, root))
                return;
            string filename = "";
            if (DA.GetData(1, ref filename) && !filename.Equals(""))
            {
                root.Add(filename);
            }

            var combined = Path.Combine(root.ToArray());
            string extention = "";
            if (DA.GetData(2, ref extention) && !extention.Equals(""))
            {
                combined = Path.ChangeExtension(combined, extention);
            }

            DA.SetData(0, combined);

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
            get { return new Guid("0f6c51cf-8a33-4b22-b6ff-76f11e1d6bb8"); }
        }
    }
}