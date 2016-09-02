using System;
using System.Collections.Generic;
using Rhino;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System.Drawing;
using System.Linq;

namespace K2Engineering
{
    public class DisplacementVisualisation : GH_Component
    {
        private List<Line> lines;
        private List<Color> colours;


        /// <summary>
        /// Initializes a new instance of the DisplacementVisualisation class.
        /// </summary>
        public DisplacementVisualisation()
          : base("DisplacementVisualisation", "DisplVis",
              "Visualise the nodal displacements by colouring the connected bars",
              "K2Eng", "5 Display")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddLineParameter("Lines", "ln", "The lines to display", GH_ParamAccess.list);
            pManager.AddPointParameter("DisplacedNodes", "pFinal", "The displaced nodes", GH_ParamAccess.list);
            pManager.AddNumberParameter("Displacement", "displ", "The nodal displacements", GH_ParamAccess.list);
            pManager.AddColourParameter("ColourRange", "c", "The colour range (use gradient)", GH_ParamAccess.list);
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
            List<Line> bars = new List<Line>();
            if(!DA.GetDataList(0, bars)) { return; }

            List<Point3d> nodes = new List<Point3d>();
            if(!DA.GetDataList(1, nodes)) { return; }

            List<double> displacements = new List<double>();
            if(!DA.GetDataList(2, displacements)) { return; }

            List<Color> gradientColours = new List<Color>();
            if(!DA.GetDataList(3, gradientColours)) { return; }


            //Calculate
            lines = new List<Line>();
            for(int i=0; i<bars.Count; i++)
            {
                lines.Add(bars[i]);
            }

            List<double> barDispl = calcBarDisplacements(bars, nodes, displacements);
            colours = mapDisplToColour(barDispl, gradientColours);

        }


        //Methods

        //Calculate the displacement for each line (max value of end points)
        List<double> calcBarDisplacements(List<Line> bars, List<Point3d> nodes, List<double> displ)
        {
            List<double> barDispl = new List<double>();

            int count = 0;
            foreach(Line ln in bars)
            {
                Point3d ptStart = ln.From;
                Point3d ptEnd = ln.To;

                int indexStart = nodes.IndexOf(ptStart);
                int indexEnd = nodes.IndexOf(ptEnd);

                double dStart = 0.0;
                double dEnd = 0.0;
                if (indexStart != -1 && indexEnd != -1)
                {
                    dStart = displ[indexStart];
                    dEnd = displ[indexEnd];
                }
                else
                {
                    this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Unable to find end point indexes of line " + count + " amongst the nodes");
                }

                double dMax = dStart;
                if (dEnd > dStart)
                {
                    dMax = dEnd;
                }

                barDispl.Add(dMax);
                count++;
            }

            return barDispl;
        }


        //Map bar displacements to colours (one colour per line)
        List<Color> mapDisplToColour(List<double> barDispl, List<Color> gradient)
        {
            List<Color> barColours = new List<Color>();

            double dMin = barDispl.Min();
            double dMax = barDispl.Max();

            double gIndexMin = 0.0;
            double gIndexMax = gradient.Count-1;

            for(int i=0; i<barDispl.Count; i++)
            {
                double d_normal = (barDispl[i] - dMin) / (dMax - dMin);
                double d_map = gIndexMin + d_normal * (gIndexMax - gIndexMin);

                int id = Convert.ToInt32(d_map);
                barColours.Add(gradient[id]);
            }

            return barColours;
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
                        args.Display.DrawLine(lines[i], colours[i], 2);
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
                return Properties.Resources.DisplVisualisation;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("{c0cc73f2-a542-44de-98d4-05a828cde909}"); }
        }
    }
}