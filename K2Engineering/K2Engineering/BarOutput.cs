using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace K2Engineering
{
    public class BarOutput : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the CastBarOutput class.
        /// </summary>
        public BarOutput()
          : base("BarOutput", "BarOutput",
              "Extract the output of the Bar goal",
              "K2Eng", "3 Results")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("BarData", "BD", "The BarData from the output of the Bar/Cable goal", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddLineParameter("Line", "Ln", "The updated line geometry", GH_ParamAccess.item);
            pManager.AddNumberParameter("AxialForce", "F", "The axial force [kN] in the bar (- is compression)", GH_ParamAccess.item);
            pManager.AddNumberParameter("AxialStress", "sigmaA", "The axial stress [MPa] in the bar (- is compression)", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //Input
            DataTypes.BarData barData = new DataTypes.BarData();
            DA.GetData(0, ref barData);

            //Extract properties
            Line ln = barData.BarLine;
            double force = barData.Force;
            double stress = barData.Stress;

            //Output
            DA.SetData(0, ln);
            DA.SetData(1, Math.Round(force,6));
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
                return Properties.Resources.BarOutput;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("{95d2e6d6-399c-4ec4-843f-fc6335ced616}"); }
        }
    }
}