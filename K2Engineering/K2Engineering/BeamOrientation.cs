using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace K2Engineering
{
    public class BeamOrientation : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the BeamOrientation class.
        /// </summary>
        public BeamOrientation()
          : base("BeamOrientation", "BeamOrientation",
              "Compute the start and end plane orientation of a beam element",
              "K2Eng", "6 Utility")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddLineParameter("Line", "Ln", "Line representing the beam element", GH_ParamAccess.item);
            pManager.AddBooleanParameter("FlipVertical", "flipV", "Flip the orientation for a vertical beam", GH_ParamAccess.item, false);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddPlaneParameter("StartPlane", "startPln", "The start plane of the beam. Red axis corresponds to the local y-axis of the cross section and green axis to the local z-axis", GH_ParamAccess.item);
            pManager.AddPlaneParameter("EndPlane", "endPln", "The end plane of the beam. Red axis corresponds to the local y-axis of the cross section and green axis to the local z-axis", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //Input
            Line ln = new Line();
            DA.GetData(0, ref ln);

            bool flip = false;
            DA.GetData(1, ref flip);


            //Calculate
            Plane startPln = new Plane();
            Plane endPln = new Plane();

            Vector3d edge = new Vector3d(ln.To - ln.From);
            edge.Unitize();

            Vector3d crossZ = Vector3d.CrossProduct(edge, Vector3d.ZAxis);

            //If the beam is vertical
            if (crossZ.IsZero)
            {
                Vector3d x = Vector3d.XAxis;
                Vector3d y = Vector3d.YAxis;
                startPln = new Plane(ln.From, x, y);
                endPln = new Plane(ln.To, x, y);

                if (flip)
                {
                    startPln = new Plane(ln.From, -y, x);
                    endPln = new Plane(ln.To, -y, x);
                }
            }
            
            //If the beam is not vertical
            else
            {
                Vector3d x = crossZ;
                Vector3d y = Vector3d.CrossProduct(x, edge);
                startPln = new Plane(ln.From, x, y);
                endPln = new Plane(ln.To, x, y);
            }

            //Output
            DA.SetData(0, startPln);
            DA.SetData(1, endPln);
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
                return Properties.Resources.Orient;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("098b1b46-32dd-4f90-b4ac-c05d6609ea64"); }
        }
    }
}