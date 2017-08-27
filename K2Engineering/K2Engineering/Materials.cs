using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace K2Engineering
{
    public class Materials : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the Materials class.
        /// </summary>
        public Materials()
          : base("Materials", "Mat",
              "List of predefined material properties",
              "K2Eng", "6 Utility")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddIntegerParameter("MaterialIndex", "MatId", "0:Steel, 1:Aluminium, 2:Timber, 3:GFRP, 4:Carbon fiber, 5:ETFE, 6:Dyneema, 7:Kevlar", GH_ParamAccess.item, 0);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Material name", "Name", "The name of the material", GH_ParamAccess.item);
            pManager.AddNumberParameter("Density", "rho", "The density of the material in [kg/m3]", GH_ParamAccess.item);
            pManager.AddNumberParameter("Young's Modulus", "E", "Young's Modulus in [MPa]", GH_ParamAccess.item);
            pManager.AddNumberParameter("Strength", "fy", "The yield strength of the material in [MPa]", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //Input
            int index = 0;
            DA.GetData(0, ref index);


            //Calculate
            string name;
            double density;
            double E;
            double fy;

            //Steel
            if (index == 0)
            {
                name = "Steel (S355)";
                density = 7850;
                E = 2.1e5;
                fy = 355;
            }

            //Aluminium
            else if (index == 1)
            {
                name = "Aluminium (6061-T6)";
                density = 2700;
                E = 70e3;
                fy = 240;
            }

            //Timber
            else if (index == 2)
            {
                name = "Birch plywood (t=6.5 mm)";
                density = 680;
                E = (12737 + 4763) / 2.0;
                fy = (50.9 + 29) / 2.0;
            }

            //GFRP
            else if (index == 3)
            {
                name = "GFRP";
                density = 2100;
                E = 40e3;
                fy = 900;
            }

            //Carbon fiber
            else if (index == 4)
            {
                name = "Carbon fiber";
                density = 1600;
                E = (125e3 + 181e3) / 2.0;
                fy = 900;
            }

            //ETFE
            else if (index == 5)
            {
                name = "ETFE";
                density = 1700;
                E = 960;
                fy = 20;
            }

            //Dyneema
            else if (index == 6)
            {
                name = "Dyneema";
                density = 990;
                E = (55e3 + 172e3) / 2.0;
                fy = 1400;
            }

            //Kevlar
            else
            {
                name = "Kevlar (49)";
                density = 1440;
                E = (70.5e3 + 112.4e3) / 2.0;
                fy = 3000;
            }


            //Output
            DA.SetData(0, name);
            DA.SetData(1, density);
            DA.SetData(2, E);
            DA.SetData(3, fy);
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
            get { return new Guid("7ecb0b99-b4b4-4655-bded-5595997e820a"); }
        }
    }
}