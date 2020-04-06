using System;
using System.Collections.Generic;
using System.IO;
using GH_IO.Serialization;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace GH.MiscToolbox.Components
{
    public class LoadArchiveComponent : GH_Component, IGH_VariableParameterComponent
    {
        /// <summary>
        /// Initializes a new instance of the LoadArchiveComponent class.
        /// </summary>
        public LoadArchiveComponent()
          : base("Load Archive", "LoadArchive",
              "Load Grasshopper objects from a Grasshopper file format",
              "MiscToolbox", "IO")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Path", "P", "Path to load data from", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
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

            if (!File.Exists(path))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"Cannot find file: {path}");
                return;
            }

            var archive = new GH_Archive();
            archive.ReadFromFile(path);


            if (Checked(archive))
            {
                Params.Output.Clear();
                for (int i = 0; i < archive.GetRootNode.ChunkCount; i++)
                {
                    var name = archive.GetRootNode.Chunks[i].Name;
                    var param = new Param_GenericObject
                    {
                        NickName = name,
                        Access = GH_ParamAccess.tree
                    };

                    Params.RegisterOutputParam(param);
                }

                Params.OnParametersChanged();

                ExpireSolution(true);

            }
            else
            {
                for (int i = 0; i < archive.GetRootNode.ChunkCount; i++)
                {
                    var name = archive.GetRootNode.Chunks[i].Name;
                    var d = new GH_Structure<IGH_Goo>();
                    var success = archive.ExtractObject(d, name);
                    DA.SetDataTree(i, d);
                }
            }
        }

        private bool Checked(GH_Archive archive)
        {
            if (Params.Output.Count != archive.GetRootNode.ChunkCount)
                return true;

            for (int i = 0; i < archive.GetRootNode.ChunkCount; i++)
            {
                var name = archive.GetRootNode.Chunks[i].Name;
                Params.Output[i].NickName = name;
            }
            Params.OnParametersChanged();

            return false;
        }

        public bool CanInsertParameter(GH_ParameterSide side, int index)
        {
            return false;
        }

        public bool CanRemoveParameter(GH_ParameterSide side, int index)
        {
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
            return false;
        }

        public void VariableParameterMaintenance()
        {
            return;
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
            get { return new Guid("0e02dea3-869a-46b4-83ce-5ec978bb018a"); }
        }
    }
}