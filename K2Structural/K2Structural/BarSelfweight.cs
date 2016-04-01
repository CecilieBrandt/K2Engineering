using System;
using System.Collections.Generic;
using Rhino;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace K2Structural
{
    public class BarSelfweight : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the BarSelfweight class.
        /// </summary>
        public BarSelfweight()
          : base("BarSelfweight", "BSelfweight",
              "Calculate the selfweight of bar elements",
              "K2Eng", "2 Load")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddLineParameter("Bars", "bars", "The bar elements [m] with equal cross section properties", GH_ParamAccess.list);
            pManager.AddNumberParameter("CrossSectionArea", "A", "The cross section area in [mm2]", GH_ParamAccess.item);
            pManager.AddNumberParameter("MaterialDensity", "rho", "The material density in [kg/m3]", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("Points", "pts", "A list of points which the forces act on", GH_ParamAccess.list);
            pManager.AddVectorParameter("NodalSelfweight", "Fweight", "The nodal selfweight in [N]", GH_ParamAccess.list);
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

            double area = 0.0;
            DA.GetData(1, ref area);

            double rho = 0.0;
            DA.GetData(2, ref rho);


            //Calculate
            Point3d[] nodes = extractNodes(bars);
            Vector3d[] nodalForces = calcNodalSelfweight(bars, nodes, area, rho);

            //Output
            DA.SetDataList(0, nodes);
            DA.SetDataList(1, nodalForces);
        }


        //Methods

        //Calculate nodal selfweight
        Vector3d[] calcNodalSelfweight(List<Line> bars, Point3d[] nodes, double area, double rho)
        {
            Vector3d[] nodalForces = new Vector3d[nodes.Length];
            //Initialise force array
            for (int i = 0; i < nodalForces.Length; i++)
            {
                nodalForces[i] = new Vector3d(0, 0, 0);
            }

            //force direction
            Vector3d dir = new Vector3d(0, 0, -1.0);

            //Add forces resulting from each bar
            foreach (Line ln in bars)
            {
                int indexStart = Array.IndexOf(nodes, ln.From);
                int indexEnd = Array.IndexOf(nodes, ln.To);

                Vector3d edge = new Vector3d(ln.To - ln.From);
                double length = edge.Length * 0.5;

                Vector3d force = dir * length * area * 1e-6 * rho * 9.82;      //Units: [N]

                nodalForces[indexStart] += force;
                nodalForces[indexEnd] += force;
            }

            return nodalForces;
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



        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return Properties.Resources.Gravity;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("{a2896665-2ab3-4dd6-b660-a69395cdf868}"); }
        }
    }
}