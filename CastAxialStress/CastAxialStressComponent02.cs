using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace CastAxialStress
{
    public class CastAxialStressComponent : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public CastAxialStressComponent()
            : base("CastAxialStress", "CAxial",
                "Cast the output of the bar goal",
                "K2Struct", "Visualisation")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("BarOutput", "O", "The output from the bar goal", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddIntegerParameter("PIndexAxialStart", "PIndexA[s]", "The start index of each bar element", GH_ParamAccess.item);
            pManager.AddIntegerParameter("PIndexAxialEnd", "PIndexA[e]", "The end index of each bar element", GH_ParamAccess.item);
            pManager.AddLineParameter("Line", "line", "The line of the updated geometry", GH_ParamAccess.item);
            pManager.AddNumberParameter("AxialStress", "stressA", "The axial stress value in the bar (- is compression)", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //Input
            Object[] output = new Object[4];
            DA.GetData(0, ref output);

            //Casting
            int pIndexStart = (int) output[0];
            int pIndexEnd = (int) output[1];

            Line ln = (Line) output[2];
            double stress = (double) output[3];

            //Output
            DA.SetData(0, pIndexStart);
            DA.SetData(1, pIndexEnd);
            DA.SetData(2, ln);
            DA.SetData(3, Math.Round(stress,1));
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
            get { return new Guid("{263e4d38-8c22-4537-b91e-5ef17d9f34e4}"); }
        }
    }
}
