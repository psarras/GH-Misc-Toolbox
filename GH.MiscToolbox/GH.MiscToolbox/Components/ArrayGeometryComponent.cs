using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace GH.MiscToolbox.Components
{
    public class ArrayGeometryComponent : GH_Component
    {
        public enum TypeOfArray
        {
            EqualNumber,
            EqualLengthNumber,
            EqualLength
        }

        public Dictionary<TypeOfArray, string> contextMenuValues = new Dictionary<TypeOfArray, string>
        {
            {TypeOfArray.EqualNumber, "Equal Number" },
            {TypeOfArray.EqualLengthNumber, "Equal Length Number" },
            {TypeOfArray.EqualLength, "Equal Length" },
        };

        private TypeOfArray current;

        /// <summary>
        /// Initializes a new instance of the ArrayGeometryComponent class.
        /// </summary>
        public ArrayGeometryComponent()
          : base("Array Geometry", "ArrayGeometry",
              "Array geometries on a grid",
              "MiscToolbox", "Utilities")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGeometryParameter("Geometry", "G", "Geometry to array", GH_ParamAccess.tree);
            pManager.AddNumberParameter("Spacing", "S", "Spacing between the bounding box of the geometry", GH_ParamAccess.item, 0);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGeometryParameter("Geometry", "G", "Geometry to array", GH_ParamAccess.tree);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_Structure<IGH_GeometricGoo> geometry;
            if (!DA.GetDataTree(0, out geometry))
                return;
            double spacing = 0;
            if (!DA.GetData(1, ref spacing))
                return;

            int items = geometry.PathCount;
            List<GeometryBase> geometryBases = new List<GeometryBase>();
            List<Box> boxes = new List<Box>();
            for (int i = 0; i < geometry.PathCount; i++)
            {
                BoundingBox box = BoundingBox.Unset;
                for (int j = 0; j < geometry.Branches[i].Count; j++)
                {
                    var bb = geometry.Branches[i][j].GetBoundingBox(Transform.Identity);
                    if (!box.IsValid)
                        box = bb;
                    else
                        box.Union(bb);
                }
                boxes.Add((new Box(box)).CenterBox());
            }

            int size = (int)Math.Ceiling(Math.Sqrt(1.0 * items));
            int sizeY = size;
            int c = 0;
            var newgeometry = new GH_Structure<IGH_GeometricGoo>();
            double width = boxes.Select(x => x.X.Length).Max();
            double depth = boxes.Select(x => x.Y.Length).Max();

            double ratioA = width / depth;
            double ratioB = depth / width;

            if (current == TypeOfArray.EqualLength)
            {
                CalculateXY(width + spacing, depth + spacing, items, out size, out sizeY);
            }

            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < sizeY; j++)
                {
                    Point3d points = new Point3d();

                    switch (current)
                    {
                        case TypeOfArray.EqualNumber:
                            points = new Point3d(i * (width + spacing), j * (depth + spacing), 0);
                            break;
                        case TypeOfArray.EqualLengthNumber:
                            var p2 = new Point3d(i * (width / ratioA + spacing), j * (depth + spacing), 0);
                            var p3 = new Point3d(i * (width + spacing), j * (depth / ratioB + spacing), 0);
                            points = width > depth ? p3 : p2;
                            break;
                        case TypeOfArray.EqualLength:
                            points = new Point3d(i * (width + spacing), j * (depth + spacing), 0);

                            break;
                        default:
                            break;
                    }

                    var to = new Plane(points, Vector3d.ZAxis);

                    if (c < geometry.PathCount)
                    {
                        var branch = new List<IGH_GeometricGoo>();
                        for (int k = 0; k < geometry.Branches[c].Count; k++)
                        {
                            var from = boxes[c].Plane;
                            var copy = geometry.Branches[c][k].Duplicate() as IGH_GeometricGoo;
                            branch.Add(copy.Transform(Transform.PlaneToPlane(from, to)));
                        }
                        newgeometry.AppendRange(branch, new GH_Path(c));
                    }
                    c++;

                }
            }
            DA.SetDataTree(0, newgeometry);
            //DA.SetDataList(1, boxes);

        }

        public void CalculateXY(double width, double height, int max, out int x, out int y)
        {
            double ratio = 0;
            if (width > height)
            {
                ratio = height / width;
                x = (int)Math.Ceiling(Math.Sqrt(ratio * max));
                y = (int)(max / (1.0 * x));
            }
            else
            {
                ratio = width / height;
                y = (int)Math.Ceiling(Math.Sqrt(max * ratio));
                x = (int)(max / (1.0 * y));
            }

            // 
            /// ratio = y / x;
            /// x = y /ratio
            /// 
            /// x * y = max; 
            /// x = max / y;
            /// max / y = y / ratio
            /// max = y2 / ratio 
            /// max * ratio = y2
            // 
            // 


            // y = (ratio * max) / y;
        }

        protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu)
        {
            base.AppendAdditionalComponentMenuItems(menu);
            ToolStripMenuItem itemA = Menu_AppendItem(menu, contextMenuValues[TypeOfArray.EqualLength], Menu_EqualLength, true, current == TypeOfArray.EqualLength);

            ToolStripMenuItem itemC = Menu_AppendItem(menu, contextMenuValues[TypeOfArray.EqualLengthNumber], Menu_EqualLengthNumber, true, current == TypeOfArray.EqualLengthNumber);

            ToolStripMenuItem itemB = Menu_AppendItem(menu, contextMenuValues[TypeOfArray.EqualNumber], Menu_EqualNumber, true, current == TypeOfArray.EqualNumber);
        }

        private void Menu_EqualNumber(object sender, EventArgs e)
        {
            current = TypeOfArray.EqualNumber;
            ExpireSolution(true);
        }

        private void Menu_EqualLengthNumber(object sender, EventArgs e)
        {
            current = TypeOfArray.EqualLengthNumber;
            ExpireSolution(true);
        }

        private void Menu_EqualLength(object sender, EventArgs e)
        {
            current = TypeOfArray.EqualLength;
            ExpireSolution(true);
        }
        public override bool Write(GH_IO.Serialization.GH_IWriter writer)
        {
            // First add our own field.
            writer.SetInt32("Type", (int)current);
            // Then call the base class implementation.
            return base.Write(writer);
        }

        public override bool Read(GH_IO.Serialization.GH_IReader reader)
        {
            // First read our own field.
            current = (TypeOfArray)reader.GetInt32("Type");
            // Then call the base class implementation.
            return base.Read(reader);
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
            get { return new Guid("2c87530e-377a-49ce-8549-392095c409e1"); }
        }
    }
}