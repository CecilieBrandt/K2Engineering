using System;
using System.Collections.Generic;
using System.Drawing;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System.Linq;


namespace BendingVisualisation
{
    public class BendingVisualisationComponent : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public BendingVisualisationComponent()
            : base("BendingStressDisplay", "BendingDisplay",
                "Visualise the bending stress with colour (blue=low, green=medium, red=high)",
                "K2Struct", "Visualisation")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPlaneParameter("Bending planes", "pl", "The bending planes", GH_ParamAccess.list);
            pManager.AddNumberParameter("Bending stresses", "stressB", "The bending stresses", GH_ParamAccess.list);
            pManager.AddNumberParameter("ScaleFactor", "sc", "The scale factor of the lines", GH_ParamAccess.item, 500);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddLineParameter("Line", "ln", "Line representing bendingstress level", GH_ParamAccess.list);
            pManager.AddColourParameter("Colour", "C", "The colour of the bar", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //Input
            List<Plane> planes = new List<Plane>();
            DA.GetDataList(0, planes);

            List<double> stresses = new List<double>();
            DA.GetDataList(1, stresses);

            double scale = 1.0;
            DA.GetData(2, ref scale);


            //Remap stress values to colours and lines
            List<Color> colours = new List<Color>();
            List<Line> lines = new List<Line>();

            double colourMax = 4*255.0;
            double colourMin = 0.0;
            double lengthMax = 1.0*scale;
            double lengthMin = 0.1*scale;

            double minStress = stresses.Min();
            double maxStress = stresses.Max();
            double stressRange = maxStress - minStress;

            //if the min stress is zero then the smallest length is zero
            if(Math.Round(minStress, 1) == 0.0){
                    lengthMin = 0.0;
            }


            for (int i = 0; i < stresses.Count; i++)
            {
                //in case of constant stress (not equal to zero)
                int tMapColour = Convert.ToInt32(colourMax / 2.0);
                double tMapLength = lengthMax / 2.0;

                //If both max and min stress equals zero then the length is constant zero
                if (Math.Round(maxStress, 1) == 0.0 && Math.Round(minStress, 1) == 0.0)
                {
                    tMapLength = 0.0;
                }

                else if (Math.Round(stressRange, 1) != 0.0)
                {
                    double t = (stresses[i] - minStress) / stressRange;
                    tMapColour = Convert.ToInt32( t * (colourMax - colourMin) + colourMin);
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


            //Output
            DA.SetDataList(0, lines);
            DA.SetDataList(1, colours);
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
            get { return new Guid("{e4fb47b8-f425-4c79-a3fb-f0920530a158}"); }
        }
    }
}
