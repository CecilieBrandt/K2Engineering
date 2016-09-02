using System;
using System.Collections.Generic;
using Rhino;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace K2Engineering
{
    public class BarLength : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the BarLength class.
        /// </summary>
        public BarLength()
          : base("BarLength", "BLength",
              "Calculate the length of bars coming into a node",
              "K2Eng", "2 Load")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddLineParameter("Bars", "bars", "Bar elements", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("Points", "pts", "A list of points for which the bar lengths are calculated", GH_ParamAccess.list);
            pManager.AddNumberParameter("NodeLengths", "L", "The nodal lengths", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //Input
            List<Line> bars = new List<Line>();
            DA.GetDataList(0, bars);


            //Calculate
            Point3d[] nodes = extractNodes(bars);
            double[] nodalLengths = calcNodalLengths(bars, nodes);


            //Output
            DA.SetDataList(0, nodes);
            DA.SetDataList(1, nodalLengths);
        }



        //Methods

        //Calculate lengths coming into each node
        double[] calcNodalLengths(List<Line> bars, Point3d[] nodes)
        {
            bool isMeters = isModelMeters();
            Point3d[] nodesAdjusted = adjustNodeAccuracy(nodes); 

            double[] nodalLengths = new double[nodes.Length];
            
            //Initialise length array
            for (int i = 0; i < nodalLengths.Length; i++)
            {
                nodalLengths[i] = 0.0;
            }

            //Add half lengths from each incoming bar
            int count = 0;
            foreach (Line ln in bars)
            {
                //Round
                double xStart = Math.Round(ln.FromX, 3);
                double yStart = Math.Round(ln.FromY, 3);
                double zStart = Math.Round(ln.FromZ, 3);

                double xEnd = Math.Round(ln.ToX, 3);
                double yEnd = Math.Round(ln.ToY, 3);
                double zEnd = Math.Round(ln.ToZ, 3);

                if (!isMeters)
                {
                    xStart = Convert.ToInt32(xStart);
                    yStart = Convert.ToInt32(yStart);
                    zStart = Convert.ToInt32(zStart);

                    xEnd = Convert.ToInt32(xEnd);
                    yEnd = Convert.ToInt32(yEnd);
                    zEnd = Convert.ToInt32(zEnd);
                }

                Point3d ptStartA = new Point3d(xStart, yStart, zStart);
                Point3d ptEndA = new Point3d(xEnd, yEnd, zEnd);

                int indexStart = Array.IndexOf(nodesAdjusted, ptStartA);
                int indexEnd = Array.IndexOf(nodesAdjusted, ptEndA);

                if(indexStart == -1 || indexEnd == -1)
                {
                    this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Not able to find start/end index of line " + count);
                }

                Vector3d edge = new Vector3d(ln.To - ln.From);
                double length = edge.Length * 0.5;

                nodalLengths[indexStart] += length;
                nodalLengths[indexEnd] += length;

                count++;
            }

            return nodalLengths;
        }


        //Create a list of points from bar elements (remove duplicates)
        Point3d[] extractNodes(List<Line> bars)
        {
            //Create a list of all points
            List<Point3d> nodesAll = new List<Point3d>();
            foreach (Line ln in bars)
            {
                nodesAll.Add(ln.From);
                nodesAll.Add(ln.To);
            }

            //Cull duplicates
            Point3d[] nodes = Point3d.CullDuplicates(nodesAll, RhinoDoc.ActiveDoc.ModelAbsoluteTolerance);

            return nodes;
        }


        //Determine if model scale is in Meters
        bool isModelMeters()
        {
            bool isMeters = false;

            UnitSystem us = RhinoDoc.ActiveDoc.ModelUnitSystem;
            String unit = us.ToString();
            if (unit.Equals("Meters"))
            {
                isMeters = true;
            }

            return isMeters;
        }


        //Create a list of points with accuracy according to model units
        Point3d[] adjustNodeAccuracy(Point3d [] nodesOriginal)
        {
            bool isMeters = isModelMeters();

            //Create a list of all points with adjusted accuracy
            Point3d[] ptsAdjusted = new Point3d[nodesOriginal.Length];
            for (int i=0; i<nodesOriginal.Length; i++)
            {
                double x = Math.Round(nodesOriginal[i].X, 3);
                double y = Math.Round(nodesOriginal[i].Y, 3);
                double z = Math.Round(nodesOriginal[i].Z, 3);

                if (!isMeters)
                {
                    x = Convert.ToInt32(x);
                    y = Convert.ToInt32(y);
                    z = Convert.ToInt32(z);
                }

                ptsAdjusted[i] = new Point3d(x, y, z);
            }

            return ptsAdjusted;
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
                return Properties.Resources.BarLength;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("{84466009-1456-467c-8f43-8a13b1161b5a}"); }
        }
    }
}