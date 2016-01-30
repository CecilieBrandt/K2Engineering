using System;
using System.Collections.Generic;
using System.Drawing;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace AxialVisualisation
{
    public class AxialVisualisationComponent : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public AxialVisualisationComponent()
            : base("AxialForceDisplay", "AxialDisplay",
                "Visualise the axial forces with colour and line weight (blue=tension, green=neutral, red=compression)",
                "K2Struct", "Visualisation")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Axial stresses", "stressA", "The axial stresses", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddColourParameter("Colour", "C", "The colour of the bar", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Linewidth", "w", "The line width of the bar in pixels", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //Input
            List<double> stresses = new List<double>();
            DA.GetDataList(0, stresses);


            //Properties to calculate
            List<Color> colours = new List<Color>();
            List<int> thickness = new List<int>();

            //Convert stresses to integers (significant digits) and map to colour simultaneously
            List<int> stressesInt = new List<int>();
            foreach (double val in stresses)
            {
                stressesInt.Add(Convert.ToInt32(val));

                Color c = new Color();

                if (val > 0.0)
                {
                    c = Color.FromArgb(0, 0, 255);    //tension (blue)

                }
                else if (val < 0.0)
                {
                    c = Color.FromArgb(255, 0, 0);    //compression (red)
                }
                else
                {
                    c = Color.FromArgb(0, 255, 0);    //neutral (green)
                }

                colours.Add(c);
            }

            
            //Absolute stress range
            int minStress = Math.Abs(stressesInt[0]);
            int maxStress = Math.Abs(stressesInt[0]);

            foreach (int val in stressesInt)
            {
                if (Math.Abs(val) < minStress)
                {
                    minStress = Math.Abs(val);
                }

                if (Math.Abs(val) > maxStress)
                {
                    maxStress = Math.Abs(val);
                }
            }
            double stressRange = maxStress - minStress;

            //Remap stress values to line widths if the stress range is not constant
            int widthMin = 2;
            int widthMax = 7;

            foreach (int s in stressesInt)
            {
                int tMap = 2;       //default thickness in pixels in case of constant stress values

                if (stressRange != 0.0)
                {
                    double t = (Math.Abs(s) - minStress) / stressRange;
                    tMap = Convert.ToInt32((t * (widthMax - widthMin)) + widthMin);
                }

                thickness.Add(tMap);
            }

            //Output
            DA.SetDataList(0, colours);
            DA.SetDataList(1, thickness);
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
            get { return new Guid("{dd0c1717-4a33-4f07-b0dd-610e361252e6}"); }
        }
    }
}
