using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace K2Engineering
{
    public class CastSupportOutput : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the CastSupportOutput class.
        /// </summary>
        public CastSupportOutput()
          : base("CastSupportOutput", "SupportOutput",
              "Cast the output of the support goal",
              "K2Eng", "6 Utility")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("SupportData", "SD", "The SupportData from the output of the Support goal", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("SupportPt", "pt", "The support point", GH_ParamAccess.item);
            pManager.AddVectorParameter("ReactionForce", "RF", "The reaction force [kN]", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //Input
            DataTypes.SupportData supportData = new DataTypes.SupportData();
            DA.GetData(0, ref supportData);

            //Extract properties
            Point3d pt = supportData.Location;
            Vector3d reactionForce = supportData.Reaction;

            //Output
            DA.SetData(0, pt);
            DA.SetData(1, reactionForce);
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
                return Properties.Resources.SupportOutput;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("{ab0bb292-8e4a-46d3-8890-955f0c00bc9b}"); }
        }
    }
}