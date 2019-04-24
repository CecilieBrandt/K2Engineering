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
            pManager.AddLineParameter("Bars", "bars", "Bar elements in [m]", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("Points", "pts", "A list of points for which the bar lengths are calculated", GH_ParamAccess.list);
            pManager.AddNumberParameter("NodalLengths", "L", "The nodal lengths", GH_ParamAccess.list);
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
            double tol = 0.001;                                             //Model in meters so tolerance is 1 mm
            Point3d[] nodes = extractNodes(bars, tol);
            double[] nodalLengths = calcNodalLengths(bars, nodes, tol);


            //Output
            DA.SetDataList(0, nodes);
            DA.SetDataList(1, nodalLengths);
        }



        //Methods

        //Calculate lengths coming into each node
        double[] calcNodalLengths(List<Line> bars, Point3d[] nodes, double tol)
        {
            double[] nodalLengths = new double[nodes.Length];
            
            //Initialise length array
            for (int i = 0; i < nodalLengths.Length; i++)
            {
                nodalLengths[i] = 0.0;
            }

            //Add half lengths from each incoming bar
            
            foreach (Line ln in bars)
            {
                Point3d ptStart = ln.From;
                Point3d ptEnd = ln.To;
                double length = ln.Length * 0.5;

                int count = 0;
                for (int i=0; i<nodes.Length; i++)
                {
                    Point3d pt = nodes[i];

                    if(ptStart.DistanceTo(pt) <= tol || ptEnd.DistanceTo(pt) <= tol)
                    {
                        nodalLengths[i] += length;
                        count++;
                    }

                    if(count == 2)
                    {
                        break;
                    }
                }
            }

            return nodalLengths;
        }


        //Create a list of points from bar elements (remove duplicates)
        Point3d[] extractNodes(List<Line> bars, double tol)
        {
            //Create a list of all points
            List<Point3d> nodesAll = new List<Point3d>();
            foreach (Line ln in bars)
            {
                nodesAll.Add(ln.From);
                nodesAll.Add(ln.To);
            }

            //Cull duplicates
            Point3d[] nodes = Point3d.CullDuplicates(nodesAll, tol);

            return nodes;
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