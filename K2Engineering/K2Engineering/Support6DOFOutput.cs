using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace K2Engineering
{
    public class Support6DOFOutput : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the Support6DOFOutput class.
        /// </summary>
        public Support6DOFOutput()
          : base("Support6DOFOutput", "Support6DOFOutput",
              "Extract the output of the 6 DOF Support goal",
              "K2Eng", "6 Utility")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Support6DOFData", "SD", "The Support6DOFData from the output of the 6 DOF Support goal", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddPlaneParameter("SupportPlane", "pl", "The support plane", GH_ParamAccess.item);
            pManager.AddVectorParameter("ReactionForce", "RF", "The reaction force [kN]", GH_ParamAccess.item);
            pManager.AddVectorParameter("ReactionMoment", "RM", "The reaction moment [kNm]", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //Input
            DataTypes.Support6DOFData supportData = new DataTypes.Support6DOFData();
            DA.GetData(0, ref supportData);

            //Extract properties
            Plane pln = supportData.Pln;
            Vector3d reactionForce = supportData.ReactionForce;
            Vector3d reactionMoment = supportData.ReactionMoment;

            //Output
            DA.SetData(0, pln);
            DA.SetData(1, reactionForce);
            DA.SetData(2, reactionMoment);
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
                return Properties.Resources.Support6DOFOutput;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("000e9754-d494-431d-8f4a-5760b976d0bb"); }
        }
    }
}