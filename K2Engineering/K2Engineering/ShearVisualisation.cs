using System;
using System.Collections.Generic;
using System.Drawing;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System.Linq;

namespace K2Engineering
{
    public class ShearVisualisation : GH_Component
    {

        //Class properties
        List<Color> colours;
        List<Polyline> polylines;


        /// <summary>
        /// Initializes a new instance of the ShearVisualisation class.
        /// </summary>
        public ShearVisualisation()
          : base("ShearVisualisation", "ShearDisplay",
              "Visualise the shear forces with colour (blue=low, green=medium, red=high)",
              "K2Eng", "4 Display")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddLineParameter("Line", "ln", "Line segments", GH_ParamAccess.list);
            pManager.AddVectorParameter("ShearVectors", "V", "Shear vector for each line segment", GH_ParamAccess.list);
            pManager.AddNumberParameter("ScaleFactor", "sc", "A scale factor", GH_ParamAccess.item, 0.5);
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
            List<Line> lines = new List<Line>();
            if (!DA.GetDataList(0, lines)) { return; }

            List<Vector3d> shearVectors = new List<Vector3d>();
            if (!DA.GetDataList(1, shearVectors)) { return; }

            double scale = 0.5;
            DA.GetData(2, ref scale);


            //Calculate

            List<double> shearValues = new List<double>();
            foreach(Vector3d s in shearVectors)
            {
                shearValues.Add(s.Length);
            }

            //Remap shear values to colours and polylines
            colours = new List<Color>();
            polylines = new List<Polyline>();

            double colourMax = 4 * 255.0;
            double colourMin = 0.0;
            double lengthMax = 1.0 * scale;
            double lengthMin = 0.1 * scale;

            double minShear = shearValues.Min();
            double maxShear = shearValues.Max();
            double shearRange = maxShear - minShear;

            //if the min shear value is zero then the smallest length is zero
            if (Math.Round(minShear, 4) == 0.0)
            {
                lengthMin = 0.0;
            }


            for (int i = 0; i < shearValues.Count; i++)
            {
                //in case of constant shear (not equal to zero)
                int tMapColour = Convert.ToInt32(colourMax / 2.0);
                double tMapLength = lengthMax;

                //If both max and min shear equals zero then the length is constant zero
                if (Math.Round(maxShear, 4) == 0.0 && Math.Round(minShear, 4) == 0.0)
                {
                    tMapLength = 0.0;
                }

                else if (Math.Round(shearRange, 4) != 0.0)
                {
                    double t = (shearValues[i] - minShear) / shearRange;
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

                colours.Add(c);

                //Create polylines from shear directions and mapped lengths
                Vector3d dir = shearVectors[i];
                dir.Unitize();
                dir *= tMapLength;

                List<Point3d> polylinePts = new List<Point3d>();
                polylinePts.Add(lines[i].From);
                polylinePts.Add(lines[i].From + dir);
                polylinePts.Add(lines[i].To + dir);
                polylinePts.Add(lines[i].To);

                Polyline pl = new Polyline(polylinePts);
                polylines.Add(pl);
            }

        }


        //Custom preview of lines with colours
        public override void DrawViewportWires(IGH_PreviewArgs args)
        {
            base.DrawViewportWires(args);
            if (Hidden) { return; }             //if the component is hidden
            if (Locked) { return; }              //if the component is locked

            if (polylines != null)
            {
                for (int i = 0; i < polylines.Count; i++)
                {
                    if (polylines[i] != null && polylines[i].IsValid)
                    {
                        args.Display.DrawPolyline(polylines[i], colours[i], 2);
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
                return Properties.Resources.ShearDisplay;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("{d488d4b0-242c-4aeb-bace-6c65ab059d5a}"); }
        }
    }
}