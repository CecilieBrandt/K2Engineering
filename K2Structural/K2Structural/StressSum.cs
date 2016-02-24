using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using Rhino.Geometry;
using Grasshopper.Kernel.Data;
using Grasshopper;

namespace K2Structural
{
    public class StressSum : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the StressSum class.
        /// </summary>
        public StressSum()
          : base("StressSum", "StressSum",
              "Summation of axial and bending stresses for each line segment",
              "K2Structural", "Results")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddIntegerParameter("PIndexStart", "PI0", "The start particle index of each bar element", GH_ParamAccess.list);
            pManager.AddIntegerParameter("PIndexEnd", "PI1", "The end particle index of each bar element", GH_ParamAccess.list);
            pManager.AddNumberParameter("AxialStress", "stressA", "The axial stresses [MPa]", GH_ParamAccess.list);
            pManager.AddIntegerParameter("PIndexBending", "PI12", "The particle index of each shared point between two consecutive line segments", GH_ParamAccess.list);
            pManager.AddNumberParameter("BendingStress", "stressB", "The bending stresses [MPa]", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("TotalStress", "sTotal", "Summation of axial and bending stresses, which outputs the maximum value for each bar element [MPa]", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
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
            get { return new Guid("{66e00a08-ccb7-4dd3-8a5e-908e7a30613c}"); }
        }
    }
}