using System;
using System.Collections.Generic;
using System.Drawing;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace K2Structural
{
    public class LoadVisualisation : GH_Component
    {
        private List<Line> lines;
        private Color c;


        /// <summary>
        /// Initializes a new instance of the LoadVisualisation class.
        /// </summary>
        public LoadVisualisation()
          : base("LoadVisualisation", "LoadDisplay",
              "Visualise the load on the deformed structure",
              "K2Structural", "4 Display")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Position", "pos", "The position of the load", GH_ParamAccess.list);
            pManager.AddVectorParameter("Load", "load", "The nodal load", GH_ParamAccess.list);
            pManager.AddColourParameter("Colour", "c", "The colour of the load", GH_ParamAccess.item, Color.DarkCyan);
            pManager.AddNumberParameter("Scale", "sc", "Scale factor", GH_ParamAccess.item, 1000.0);
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
            List<Point3d> pts = new List<Point3d>();
            DA.GetDataList(0, pts);

            List<Vector3d> loads = new List<Vector3d>();
            DA.GetDataList(1, loads);

            c = Color.DarkCyan;
            DA.GetData(2, ref c);

            double scale = 1.0;
            DA.GetData(3, ref scale);


            //Calculate
            lines = createLoadLines(pts, loads, scale);
        }

        //Methods

        //Create a list of lines from starting points and scaled vectors
        List<Line> createLoadLines(List<Point3d> startingPts, List<Vector3d> loads, double scale)
        {
            List<Line> loadLines = new List<Line>();

            for(int i=0; i<loads.Count; i++)
            {
                Point3d endPt = startingPts[i] + (loads[i] * scale);
                Line ln = new Line(startingPts[i], endPt);
                loadLines.Add(ln);
            }

            return loadLines;
        }


        //Custom preview of lines with colours and thickness
        public override void DrawViewportWires(IGH_PreviewArgs args)
        {
            base.DrawViewportWires(args);
            if (Hidden) { return; }             //if the component is hidden
            if (Locked) { return; }              //if the component is locked

            if (lines.Count != 0)
            {
                for (int i = 0; i < lines.Count; i++)
                {
                    if (lines[i] != null)
                    {
                        args.Display.DrawLine(lines[i], c, 2);
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
                return Properties.Resources.LoadDisplay;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("{1123d5aa-a65b-4ee1-8168-5961cc8b67a0}"); }
        }
    }
}