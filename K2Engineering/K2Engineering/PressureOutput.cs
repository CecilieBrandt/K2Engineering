using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace K2Engineering
{
    public class PressureOutput : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the PressureOutput class.
        /// </summary>
        public PressureOutput()
          : base("PressureOutput", "PressureOutput",
              "Extract the output of the pressure goal",
              "K2Eng", "6 Utility")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("PressureData", "PD", "The PressureData from the output of the Pressure goal", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("Positions", "pos", "The positions of the load", GH_ParamAccess.list);
            pManager.AddVectorParameter("Loads", "loads", "The nodal loads in [kN]", GH_ParamAccess.list);
            pManager.AddNumberParameter("PressureStart", "presStart", "The pressure at the start in [kN/m2]", GH_ParamAccess.item);
            pManager.AddNumberParameter("PressureEnd", "presEnd", "The pressure at the end in [kN/m2]", GH_ParamAccess.item);
            pManager.AddNumberParameter("VolumeStart", "volStart", "The volume at the start in [m3]", GH_ParamAccess.item);
            pManager.AddNumberParameter("VolumeEnd", "volEnd", "The volume at the end in [m3]", GH_ParamAccess.item);
            pManager.AddIntegerParameter("MoleculesStart", "molStart", "The amount of molecules at the start in [mole]", GH_ParamAccess.item);
            pManager.AddIntegerParameter("MoleculesEnd", "molEnd", "The amount of molecules at the end in [mole]", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //Input
            DataTypes.PressureData presData = new DataTypes.PressureData();
            DA.GetData(0, ref presData);

            //Extract properties
            Point3d[] pts = new Point3d[presData.Locations.Length];
            Vector3d[] forces = new Vector3d[presData.Loads.Length];

            for(int i=0; i<presData.Locations.Length; i++)
            {
                pts[i] = presData.Locations[i];
                forces[i] = presData.Loads[i];
            }

            double p0 = presData.PresStart;
            double p1 = presData.PresEnd;
            double v0 = presData.VolStart;
            double v1 = presData.VolEnd;
            int mol0 = presData.MolStart;
            int mol1 = presData.MolEnd;


            //Output
            DA.SetDataList(0, pts);
            DA.SetDataList(1, forces);
            DA.SetData(2, p0);
            DA.SetData(3, p1);
            DA.SetData(4, v0);
            DA.SetData(5, v1);
            DA.SetData(6, mol0);
            DA.SetData(7, mol1);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.PressureOutput;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("a2cfee5b-fb59-4092-b25b-dea8203ea8a5"); }
        }
    }
}