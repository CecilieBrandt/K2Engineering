using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace K2Engineering
{
    public class CastRodOutput : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the CastRodOutput class.
        /// </summary>
        public CastRodOutput()
          : base("CastRodOutput", "RodOutput",
              "Cast the output of the rod goal",
              "K2Eng", "6 Utility")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("RodOutput", "O", "The output from the rod goal", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddIntegerParameter("PIndexBending", "PI12", "The particle index associated with the calculated moment", GH_ParamAccess.item);
            pManager.AddPlaneParameter("Plane", "pl", "The bending plane", GH_ParamAccess.item);
            pManager.AddNumberParameter("BendingMoment", "M", "The bending moment [kNm]", GH_ParamAccess.item);
            pManager.AddNumberParameter("BendingStress", "stressB", "The bending stress [MPa]", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //Input
            Object[] output = new Object[3];
            DA.GetData(0, ref output);

            //Casting
            int PIndex = (int)output[0];
            Plane pl = (Plane)output[1];
            double moment = (double)output[2];
            double stress = (double)output[3];

            //Output
            DA.SetData(0, PIndex);
            DA.SetData(1, pl);
            DA.SetData(2, Math.Round(moment, 3));
            DA.SetData(3, Math.Round(stress, 1));
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
                return Properties.Resources.RodOutput;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("{280510b1-3f9e-42a0-a30d-2282ebae97b5}"); }
        }
    }
}