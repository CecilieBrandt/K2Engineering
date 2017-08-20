using System;
using System.Collections.Generic;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace K2Engineering
{
    public class Pretension : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the Pretension class.
        /// </summary>
        public Pretension()
          : base("Pretension", "PT",
              "Calculate pretension distribution in a cablenet",
              "K2Eng", "6 Utility")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("RestLength", "L0", "The rest length in [m]", GH_ParamAccess.tree);
            pManager.AddNumberParameter("Strength", "S0", "The strength of the cable to achieve its rest length [-]", GH_ParamAccess.tree);
            pManager.AddNumberParameter("FFLength", "L1", "The length of the form-found cable in [m]", GH_ParamAccess.tree);
            pManager.AddNumberParameter("MaxPretension", "PTmax", "The maximum value of the pretension in [N]", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("PTDistribution", "PTDistr", "The pretension distribution in [N]", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //Input
            GH_Structure<GH_Number> restLengths;
            DA.GetDataTree(0, out restLengths);

            GH_Structure<GH_Number> strengths;
            DA.GetDataTree(1, out strengths);

            GH_Structure<GH_Number> ffLengths;
            DA.GetDataTree(2, out ffLengths);

            double maxPT = 0.0;
            DA.GetData(3, ref maxPT);


            //Calculate

            //List of forces in each cable (not scaled according to max desirable pretension yet)
            GH_Structure<GH_Number> pDistr_unscaled = new GH_Structure<GH_Number>();

            for (int i = 0; i < restLengths.PathCount; i++)
            {
                GH_Path path = restLengths.Paths[i];
                int branchCount = restLengths.get_Branch(i).Count;

                for (int j = 0; j < branchCount; j++)
                {
                    double L0 = restLengths.Branches[i][j].Value;
                    double L1 = ffLengths.Branches[i][j].Value;
                    double extension = L1 - L0;

                    double k = strengths.Branches[i][0].Value;
                    double force = extension * k;

                    pDistr_unscaled.Append(new GH_Number(force), path);
                }
            }


            //List of forces in each cable scaled according to to desirable max value
            GH_Structure<GH_Number> pDistr_scaled = new GH_Structure<GH_Number>();
            double maxForce = getMaxValueFromTree(pDistr_unscaled);

            for (int i = 0; i < restLengths.PathCount; i++)
            {
                GH_Path path = restLengths.Paths[i];
                int branchCount = restLengths.get_Branch(i).Count;

                for (int j = 0; j < branchCount; j++)
                {
                    double p = pDistr_unscaled.Branches[i][j].Value;
                    double pNormal = p / maxForce;
                    double pScale = pNormal * maxPT;

                    pDistr_scaled.Append(new GH_Number(pScale), path);
                }
            }


            //Output
            DA.SetDataTree(0, pDistr_scaled);
        }


        //Methods
        //Get maximum value from datatree
        double getMaxValueFromTree(GH_Structure<GH_Number> tree)
        {
            double max = -1;

            foreach (GH_Number num in tree)
            {
                double val = num.Value;

                if (val > max)
                {
                    max = val;
                }
            }

            return max;
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
            get { return new Guid("81e1fe0e-6f4d-4539-898a-5549eec9f351"); }
        }
    }
}