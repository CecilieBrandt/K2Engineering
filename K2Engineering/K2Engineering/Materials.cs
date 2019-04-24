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
              "List of predefined material properties (use as guidance only)",
              "K2Eng", "5 Utility")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddIntegerParameter("MaterialIndex", "MatId", "0:Steel, 1:Aluminium, 2:Timber, 3:ETFE, 4:GFRP, 5:Carbon fiber, 6:Kevlar, 7:Dyneema", GH_ParamAccess.item, 0);
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

                //www.metsateollisuus.fi/uploads/2017/03/30041750/887.pdf
            }

            //ETFE
            else if (index == 3)
            {
                name = "ETFE";
                density = 1750;
                E = 965;
                fy = 48;

                //www.pronatindustries.com/wp-content/uploads/2015/03/Norton-ETFE.pdf
            }

            //GFRP
            else if (index == 4)
            {
                name = "GFRP";
                density = 2100;
                E = 40e3;
                fy = 900;
                
                //www.fibrolux.com/main/knowledge/properties
            }

            //Carbon fiber
            else if (index == 5)
            {
                name = "Carbon fiber";
                density = 1760;
                E = 230e3;
                fy = 3500;

                //www.siltex.eu/wp-content/uploads/2011/03/Carbon-Data-Sheet.pdf
            }

            //Kevlar
            else if(index == 6)
            {
                name = "Kevlar (49)";
                density = 1440;
                E = 112.4e3;
                fy = 2800;

                //www.dupont.com/content/dam/dupont/products-and-services/fabrics-fibers-and-nonwovens/fibers/documents/Kevlar_Technical_Guide.pdf
            }

            //Dyneema
            else
            {
                name = "Dyneema";
                density = 980;
                E = 116e3;
                fy = 3600;

                //www.issuu.com/eurofibers/docs/name8f0d44
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
                return Properties.Resources.Material;
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