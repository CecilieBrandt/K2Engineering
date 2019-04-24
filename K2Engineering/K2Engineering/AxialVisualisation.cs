using System;
using System.Collections.Generic;
using System.Drawing;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace K2Engineering
{
    public class AxialVisualisation : GH_Component
    {
        //Lines, colours and thickness for previewing, declared as class properties
        private List<Line> lines;
        private List<Color> colours;
        private List<int> thickness;

        /// <summary>
        /// Initializes a new instance of the AxialVisualisation class.
        /// </summary>
        public AxialVisualisation()
          : base("AxialVisualisation", "AxialDisplay",
              "Visualise the axial forces with colour and line weight (blue=tension, green=neutral, red=compression)",
              "K2Eng", "4 Display")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddLineParameter("Line", "Ln", "The lines to display", GH_ParamAccess.list);
            pManager.AddNumberParameter("Axial forces", "F", "The axial forces", GH_ParamAccess.list);
            pManager.AddIntegerParameter("ThicknessMax", "tmax", "The maximum line thickness", GH_ParamAccess.item, 10);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //Input
            lines = new List<Line>();
            if (!DA.GetDataList(0, lines)) { return; }
            if(lines == null || lines.Count == 0) { return; }

            List<double> stresses = new List<double>();
            if (!DA.GetDataList(1, stresses)) { return; }

            int tmax = 10;
            DA.GetData(2, ref tmax);



            //Properties to calculate
            colours = new List<Color>();
            thickness = new List<int>();

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
            int widthMax = tmax;

            foreach (int s in stressesInt)
            {
                int tMap = widthMin;       //default thickness in case of constant stress values

                if (stressRange != 0.0)
                {
                    double t = (Math.Abs(s) - minStress) / stressRange;
                    tMap = Convert.ToInt32((t * (widthMax - widthMin)) + widthMin);
                }

                thickness.Add(tMap);
            }
        }


        //Custom preview of lines with colours and thickness
        public override void DrawViewportWires(IGH_PreviewArgs args)
        {
            base.DrawViewportWires(args);
            if (Hidden) { return; }             //if the component is hidden
            if (Locked) { return; }              //if the component is locked

            if (lines != null)
            {
                for (int i = 0; i < lines.Count; i++)
                {
                    if (lines[i] != null && lines[i].IsValid)
                    {
                        args.Display.DrawLine(lines[i], colours[i], thickness[i]);
                    }
                }
            }

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
                return Properties.Resources.AxialDisplay;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("{c2bd6c3c-314b-47c2-b716-5b0f2f532e7f}"); }
        }
    }
}