using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using GH_IO.Serialization;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace GH.MiscToolbox.Components
{
    public class SaveArchiveComponent : GH_Component, IGH_VariableParameterComponent

    {
        /// <summary>
        /// Initializes a new instance of the SaveArchiveComponent class.
        /// </summary>
        public SaveArchiveComponent()
          : base("Save Archive", "SaveArchive",
              "Save Grasshopper objects to a Grasshopper file format",
              "MiscToolbox", "IO")
        {
            Params.ParameterNickNameChanged += ExpireSolution;
        }

        private void ExpireSolution(object sender, GH_ParamServerEventArgs e)
        {
            ExpireSolution(true);
        }

        bool binary;

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Path", "P", "Path to save data unto", GH_ParamAccess.item);
            pManager.AddGenericParameter("Data", "D", "Data to archive", GH_ParamAccess.tree);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Path", "P", "Path with the used extention", GH_ParamAccess.item);
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

            var archive = new GH_Archive();

            if (Params.Input.Count < 2)
                return;

            for (int i = 1; i < Params.Input.Count; i++)
            {
                IGH_Param input = Params.Input[i];
                var name = input.NickName;
                var tree = input.VolatileData as GH_Structure<IGH_Goo>;
                archive.AppendObject(tree, name);
            }

            if (binary)
                path = Path.ChangeExtension(path, ".gh");
            else
                path = Path.ChangeExtension(path, ".ghx");

            archive.WriteToFile(path, true, false);

            DA.SetData(0, path);
        }

        public bool CanInsertParameter(GH_ParameterSide side, int index)
        {
            if (side == GH_ParameterSide.Input && index > 0)
                return true;
            return false;
        }

        public bool CanRemoveParameter(GH_ParameterSide side, int index)
        {
            if (side == GH_ParameterSide.Input && index > 0)
                return true;
            return false;
        }

        public IGH_Param CreateParameter(GH_ParameterSide side, int index)
        {
            var param = new Param_GenericObject(); //This is the type of Input parameter. You can override that. see next example for custom menus

            //Easy way to autopopulate the names
            param.Name = GH_ComponentParamServer.InventUniqueNickname("ABCDEFGHIJKLMNOPQRSTUVWXYZ", Params.Input);
            param.NickName = param.Name;
            param.Description = "Property Name";
            param.Optional = true;
            param.Access = GH_ParamAccess.tree;
            return param;
        }

        public bool DestroyParameter(GH_ParameterSide side, int index)
        {
            return true;
        }

        public void VariableParameterMaintenance()
        {
            return;
        }

        protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu)
        {
            base.AppendAdditionalComponentMenuItems(menu);
            ToolStripMenuItem item1 = Menu_AppendItem(menu, "Binary", Menu_Binary, true, binary);
        }

        private void Menu_Binary(object sender, EventArgs e)
        {
            binary = !binary;
            this.ExpireSolution(true);
        }

        public override bool Read(GH_IReader reader)
        {
            binary = reader.GetBoolean("binary");
            return base.Read(reader);
        }

        public override bool Write(GH_IWriter writer)
        {
            writer.SetBoolean("binary", binary);
            return base.Write(writer);
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
            get { return new Guid("ddee9e34-a03e-41d1-852a-d76e7068d38d"); }
        }
    }
}