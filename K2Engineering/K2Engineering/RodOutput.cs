using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace K2Engineering
{
    public class RodOutput : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the CastRodOutput class.
        /// </summary>
        public RodOutput()
          : base("RodOutput", "RodOutput",
              "Extract the output of the Rod goal",
              "K2Eng", "3 Results")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("RodData", "RD", "The RodData from the output of the Rod goal", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddPlaneParameter("Plane", "pl", "The bending plane", GH_ParamAccess.item);
            pManager.AddNumberParameter("BendingMoment", "M", "The bending moment [kNm]", GH_ParamAccess.item);
            pManager.AddNumberParameter("BendingStress", "sigmaB", "The bending stress [MPa]", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //Input
            DataTypes.RodData rodData = new DataTypes.RodData();
            DA.GetData(0, ref rodData);

            //Extract properties
            Plane pl = rodData.BendingPlane;
            double moment = rodData.Moment;
            double stress = rodData.BendingStress;

            //Output
            DA.SetData(0, pl);
            DA.SetData(1, Math.Round(moment,9));
            DA.SetData(2, Math.Round(stress,3));
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