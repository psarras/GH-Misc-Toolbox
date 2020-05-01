using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace GH.MiscToolbox.Components.Utilities
{
    public class MinVolumeBoundingBoxComponent : GH_Component
    {
        /// <summary>
        /// Heavely based on this discussion: https://discourse.mcneel.com/t/minimum-oriented-bounding-box-implementation-in-grasshopper-python-script-node/64344/61
        /// Thanks Mitch and Ril
        /// </summary>
        public MinVolumeBoundingBoxComponent()
          : base("MinVolume BoundingBox", "MinBB",
              "Find a minimum BoundingBox using brute force in regards to rotation, around Z axis",
              "MiscToolbox", "Utilities")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGeometryParameter("Geometry", "G", "Geometry around boundingbox", GH_ParamAccess.list);
            pManager.AddNumberParameter("Degrees", "D", "Controls the accuracy. The smaller the number the more rotations will be checked", GH_ParamAccess.item, 1);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddBoxParameter("Box", "B", "Min Volume BoundingBox", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<GeometryBase> geometryBases = new List<GeometryBase>();
            if (!DA.GetDataList(0, geometryBases))
                return;

            var plane = Plane.WorldXY;
            
            m_rotation_factor = 1;
            if (!DA.GetData(1, ref m_rotation_factor))
                return;
            m_rotations = (int)(90 / m_rotation_factor);

            plane = RotateAllCombinationsOfPlane(geometryBases, plane);
            var bb = GetBox(geometryBases, plane);

            DA.SetData(0, bb);
        }

        private int m_rotations = 90; // default
        private double m_rotation_factor;
        const double TO_RADIANS = Math.PI / 180;

        private Plane RotateAllCombinationsOfPlane(List<GeometryBase> geo, Plane initial_plane)
        {
            // --------------------------------------
            // Rotate about X axis of initial plane
            // --------------------------------------
            var rotation_axis = initial_plane.XAxis;
            var rotation = GetRotationAtMinimumVolume(geo, initial_plane, rotation_axis);
            var plane_x = initial_plane;
            plane_x.Rotate(rotation, rotation_axis);

            // --------------------------------------
            // Rotate about Y axis of rotated plane_x
            // --------------------------------------
            rotation_axis = plane_x.YAxis;
            rotation = GetRotationAtMinimumVolume(geo, plane_x, rotation_axis);
            var plane_y = plane_x;
            plane_y.Rotate(rotation, rotation_axis);

            // --------------------------------------
            // Rotate about Z axis of rotated plane_y
            // --------------------------------------
            rotation_axis = plane_y.ZAxis;
            rotation = GetRotationAtMinimumVolume(geo, plane_y, rotation_axis);
            var plane_z = plane_y;
            plane_z.Rotate(rotation, rotation_axis);

            // =====================================
            // Another round of rotations starting
            // from the 3 axes of the last plane_z
            // =====================================

            // --------------------------------------
            // Rotate about X axis of rotated plane_z
            // --------------------------------------
            rotation_axis = plane_z.XAxis;
            rotation = GetRotationAtMinimumVolume(geo, plane_z, rotation_axis);
            var plane_x_refined = plane_z;
            plane_x_refined.Rotate(rotation, rotation_axis);

            // --------------------------------------
            // Rotate about Y axis of refined plane_z
            // --------------------------------------
            rotation_axis = plane_x_refined.YAxis;
            rotation = GetRotationAtMinimumVolume(geo, plane_x_refined, rotation_axis);
            var plane_y_refined = plane_x_refined;
            plane_y_refined.Rotate(rotation, rotation_axis);

            // --------------------------------------
            // Rotate about Z axis of refined plane_y
            // --------------------------------------
            rotation_axis = plane_y_refined.ZAxis;
            rotation = GetRotationAtMinimumVolume(geo, plane_y_refined, rotation_axis);
            var plane_z_refined = plane_y_refined;
            plane_z_refined.Rotate(rotation, rotation_axis);

            return plane_z_refined; // the lastly rotated plane
        }

        /// <summary>
        /// Returns the rotation (in radians) of the smallest volume. The parallel loop
        /// performs m_rotations rotations of 1º each (90 rotations is default)
        /// </summary>
        private double GetRotationAtMinimumVolume(List<GeometryBase> geo, Plane start_plane, Vector3d rotation_axis)
        {
            var rotated_volumes = new double[m_rotations];
            var rad_angle_factor = m_rotation_factor * TO_RADIANS; // calculate once

            // Let the .NET platform take care of spreading out the for-loop iterations
            // on  different threads. To avoid data races we use local block variables
            // inside the for-loop, and results are put into array slots, each result into
            // its own index in the array, which prevents "collisions" (meaning, different
            // threads overwriting each others values).
            System.Threading.Tasks.Parallel.For(0, m_rotations, i =>
            {
                // Make a fresh new rotation starting from the original plane on each try.
                // A local variable (_plane) is required here in order to "isolate" it from
                // other threads operating simultaneously on other indexes (i), so they will
                // have their own copy of _plane, and therefore we don't overwrite each other.
                // Take this as  a general rule for Parallel.For-blocks
                var _plane = start_plane;

                // The rad_angle_factor is converting the integer ("degrees") into radians AND
                // transforms it into a fraction of a degree if the input (D) is greater than 180
                _plane.Rotate(i * rad_angle_factor, rotation_axis);

                // Since each thread works with different indexes (i), and when done, assigning
                // each result value into "its own array index", then no conflict can occur be-
                // tween threads so that any one thread would overwrite the result produced by
                // another thread, and therefore no data-races will occur. This is a red-neck
                // approach which ALWAYS works if the size of the array can be known in advance
                BoundingBox bb = GetBoundingBox(geo, _plane);
                rotated_volumes[i] = bb.ToBrep().GetVolume();//.Volume;
            });

            // now find that index (degree of rotation) at which we had the smallest BoundingBox
            var rotation = IndexOfMinimumVolume(ref rotated_volumes);
            // Convert Degrees to Radians before returning the angle
            return rotation * rad_angle_factor;
        }

        private static BoundingBox GetBoundingBox(List<GeometryBase> geo, Plane plane)
        {
            var bb = GetBoundingBoxWorld(geo.First(), plane);
            geo.ForEach(x =>
            {
                bb.Union(GetBoundingBoxWorld(x, plane));
            });
            return bb;
        }

        private static Box GetBox(List<GeometryBase> geo, Plane plane)
        {
            geo.First().GetBoundingBox(plane, out Box worldBox);
            geo.ForEach(x => 
            {
                x.GetBoundingBox(plane, out Box b);
                b.GetCorners().ToList().ForEach(y => worldBox.Union(y));
            });
            return worldBox;
        }

        public static BoundingBox GetBoundingBoxWorld(GeometryBase geometryBase, Plane plane)
        {
            return geometryBase.GetBoundingBox(plane);
        }

        /// <summary>
        /// Returns the index of the smallest volume recorded in the array
        /// </summary>
        /// <returns>Returns the index of the smalles volume, which also is
        /// equal to the degree at which this volume was achieved.</returns>
        private double IndexOfMinimumVolume(ref double[] recorded_volumes)
        {
            var min_index = -1;
            var min_value = Double.MaxValue;
            for (var i = 0; i < recorded_volumes.Length; i++)
            {
                if (recorded_volumes[i] < min_value)
                {
                    min_index = i;
                    min_value = recorded_volumes[i];
                }
            }
            return min_index; // Index is the same as the degree
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
            get { return new Guid("63ce695a-11dc-49ce-9443-508c12936113"); }
        }
    }
}