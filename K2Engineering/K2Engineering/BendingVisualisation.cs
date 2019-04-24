using System;
using System.Collections.Generic;
using System.Drawing;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System.Linq;

namespace K2Engineering
{
    public class BendingVisualisation : GH_Component
    {
        //Class properties
        List<Color> colours;
        List<Line> lines;


        /// <summary>
        /// Initializes a new instance of the BendingVisualisation class.
        /// </summary>
        public BendingVisualisation()
          : base("BendingVisualisation", "BendingDisplay",
              "Visualise the bending stress with colour (blue=low, green=medium, red=high)",
              "K2Eng", "4 Display")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPlaneParameter("Bending planes", "pl", "The bending planes", GH_ParamAccess.list);
            pManager.AddNumberParameter("Bending moments", "M", "The bending moments", GH_ParamAccess.list);
            pManager.AddNumberParameter("ScaleFactor", "sc", "The scale factor of the lines", GH_ParamAccess.item, 0.5);
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
            List<Plane> planes = new List<Plane>();
            if (!DA.GetDataList(0, planes)) { return; }

            List<double> moments = new List<double>();
            if (!DA.GetDataList(1, moments)) { return; }

            double scale = 1.0;
            DA.GetData(2, ref scale);


            //Remap stress values to colours and lines
            colours = new List<Color>();
            lines = new List<Line>();

            double colourMax = 4 * 255.0;
            double colourMin = 0.0;
            double lengthMax = 1.0 * scale;
            double lengthMin = 0.1 * scale;

            double momentMin = moments.Min();
            double momentMax = moments.Max();
            double momentRange = momentMax - momentMin;

            //if the min stress is zero then the smallest length is zero
            if (Math.Round(momentMin, 1) == 0.0)
            {
                lengthMin = 0.0;
            }


            for (int i = 0; i < moments.Count; i++)
            {
                //in case of constant stress (not equal to zero)
                int tMapColour = Convert.ToInt32(colourMax / 2.0);
                double tMapLength = lengthMax / 2.0;

                //If both max and min stress equals zero then the length is constant zero
                if (Math.Round(momentMax, 1) == 0.0 && Math.Round(momentMin, 1) == 0.0)
                {
                    tMapLength = 0.0;
                }

                else if (Math.Round(momentRange, 1) != 0.0)
                {
                    double t = (moments[i] - momentMin) / momentRange;
                    tMapColour = Convert.ToInt32(t * (colourMax - colourMin) + colourMin);
                    tMapLength = t * (lengthMax - lengthMin) + lengthMin;
                }

                Color c = new Color();
                if (tMapColour <= 1 * 255)
                {
                    c = Color.FromArgb(0, tMapColour, 255);
                }
                else if (tMapColour > 1 * 255 && tMapColour <= 2 * 255)
                {
                    c = Color.FromArgb(0, 255, 255 - (tMapColour - (1 * 255)));
                }
                else if (tMapColour > 2 * 255 && tMapColour <= 3 * 255)
                {
                    c = Color.FromArgb(tMapColour - (2 * 255), 255, 0);
                }
                else
                {
                    c = Color.FromArgb(255, 255 - (tMapColour - (3 * 255)), 0);
                }

                Vector3d dir = new Vector3d(planes[i].YAxis);
                dir.Unitize();
                dir *= tMapLength;
                Line l = new Line(planes[i].Origin, planes[i].Origin + dir);

                colours.Add(c);
                lines.Add(l);
            }
        }


        //Custom preview of lines with colours
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
                return Properties.Resources.BendingDisplay;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("{d9f0db2c-973c-4516-8fd4-34719d5a6b0e}"); }
        }
    }
}