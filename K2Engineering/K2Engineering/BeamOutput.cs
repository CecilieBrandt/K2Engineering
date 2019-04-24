using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace K2Engineering
{
    public class BeamOutput : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the BeamOutput class.
        /// </summary>
        public BeamOutput()
          : base("BeamOutput", "BeamOutput",
              "Extract the output of the Beam goal",
              "K2Eng", "3 Results")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("BeamData", "BD", "The BeamData from the output of the Beam goal", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddPlaneParameter("P0", "P0", "Local start plane", GH_ParamAccess.item);
            pManager.AddPlaneParameter("P1", "P1", "Local end plane", GH_ParamAccess.item);
            pManager.AddNumberParameter("N", "N", "The normal force in [kN]", GH_ParamAccess.item);
            pManager.AddNumberParameter("Vy", "Vy", "The shear force parallel to the local y-axis in [kN]", GH_ParamAccess.item);
            pManager.AddNumberParameter("Vz", "Vz", "The shear force parallel to the local z-axis in [kN]", GH_ParamAccess.item);
            pManager.AddNumberParameter("Mt", "Mt", "The torsional moment in [kNm]", GH_ParamAccess.item);
            pManager.AddNumberParameter("My0", "My0", "The bending moment about the local y-axis at the start node in [kNm]", GH_ParamAccess.item);
            pManager.AddNumberParameter("Mz0", "Mz0", "The bending moment about the local z-axis at the start node in [kNm]", GH_ParamAccess.item);
            pManager.AddNumberParameter("My1", "My1", "The bending moment about the local y-axis at the end node in [kNm]", GH_ParamAccess.item);
            pManager.AddNumberParameter("Mz1", "Mz1", "The bending moment about the local z-axis at the end node in [kNm]", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //Input
            DataTypes.BeamData beamData = new DataTypes.BeamData();
            DA.GetData(0, ref beamData);

            this.Message = "WIP";

            //Output
            DA.SetData(0, beamData.P0);
            DA.SetData(1, beamData.P1);
            DA.SetData(2, beamData.N);
            DA.SetData(3, beamData.Vy);
            DA.SetData(4, beamData.Vz);
            DA.SetData(5, beamData.Mx);
            DA.SetData(6, beamData.My0);
            DA.SetData(7, beamData.Mz0);
            DA.SetData(8, beamData.My1);
            DA.SetData(9, beamData.Mz1);
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
                return Properties.Resources.BeamOutput;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("d64e3058-838d-4eec-8163-5e65a33f090c"); }
        }
    }
}