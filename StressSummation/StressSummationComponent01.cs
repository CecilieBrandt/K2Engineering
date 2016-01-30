using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using Rhino.Geometry;
using Grasshopper.Kernel.Data;
using Grasshopper;

namespace StressSummation
{
    public class StressSummationComponent : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public StressSummationComponent()
            : base("StressSummation", "StressTotal",
                "Summation of axial and bending stresses",
                "K2Struct", "Results")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddIntegerParameter("PIndexAxialStart", "PIndexA[s]", "The start PIndexes of the bar elements", GH_ParamAccess.list);
            pManager.AddIntegerParameter("PIndexAxialEnd", "PIndexA[e]", "The end PIndexes of the bar elements", GH_ParamAccess.list);
            pManager.AddNumberParameter("AxialStress", "stressA", "The axial stresses", GH_ParamAccess.list);
            pManager.AddIntegerParameter("PIndexBending", "PIndexB", "The PIndexes of the rod elements", GH_ParamAccess.list);
            pManager.AddNumberParameter("BendingStress", "stressB", "The bending stresses", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("TotalStress", "sTotal", "Summation of axial and bending stress with output of maximum value for each bar element", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //Input
            List<int> pIndexStart = new List<int>();
            DA.GetDataList(0, pIndexStart);

            List<int> pIndexEnd = new List<int>();
            DA.GetDataList(1, pIndexEnd);

            List<double> stressA = new List<double>();
            DA.GetDataList(2, stressA);

            List<int> pIndexB = new List<int>();
            DA.GetDataList(3, pIndexB);

            List<double> stressB = new List<double>();
            DA.GetDataList(4, stressB);


            //Calculate stress summation
            List<double> stressTotal = new List<double>();

            for (int i = 0; i < stressA.Count; i++)
            {
                //axial stress value in start and end of line
                double stressStart = stressA[i];
                double stressEnd = stressA[i];

                //find corresponding bending index and stress value
                int bStart = pIndexB.IndexOf(pIndexStart[i]);
                int bEnd = pIndexB.IndexOf(pIndexEnd[i]);
                
                double stressBStart = 0.0;
                if (bStart != -1)
                {
                    stressBStart = stressB[bStart];
                }

                double stressBEnd = 0.0;
                if (bEnd != -1)
                {
                    stressBEnd = stressB[bEnd];
                }

                //Calculate sum in btoh ends
                //start
                if (stressStart < 0.0)
                {
                    stressStart -= Math.Abs(stressBStart);
                }
                else
                {
                    stressStart += Math.Abs(stressBStart);
                }

                //end
                if (stressEnd < 0.0)
                {
                    stressEnd -= Math.Abs(stressBEnd);
                }
                else
                {
                    stressEnd += Math.Abs(stressBEnd);
                }

                //Find maximum of the two values to output maximum stress in bar
                double stressMax = Math.Max(Math.Abs(stressStart), Math.Abs(stressEnd));
                stressTotal.Add(stressMax);
            }


            //Output
            DA.SetDataList(0, stressTotal);
        }

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                // You can add image files to your project resources and access them like this:
                //return Resources.IconForThisComponent;
                return null;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("{3a048339-c69b-438b-a5f9-b71bdd0fb7dd}"); }
        }
    }
}
