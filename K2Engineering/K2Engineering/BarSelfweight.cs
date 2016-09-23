using System;
using System.Collections.Generic;
using Rhino;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace K2Engineering
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
            pManager.AddNumberParameter("NodalLengths", "L", "The nodal lengths [m]", GH_ParamAccess.list);
            pManager.AddNumberParameter("CrossSectionArea", "A", "The cross section area in [mm2]", GH_ParamAccess.item);
            pManager.AddNumberParameter("MaterialDensity", "rho", "The material density in [kg/m3]", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddVectorParameter("NodalSelfweight", "S", "The nodal selfweight in [N]", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //Input
            List<double> barLengths = new List<double>();
            DA.GetDataList(0, barLengths);

            double area = 0.0;
            DA.GetData(1, ref area);

            double rho = 0.0;
            DA.GetData(2, ref rho);


            //Calculate
            List<Vector3d> selfweight = calcNodalSelfweight(barLengths, area, rho);


            //Output
            DA.SetDataList(0, selfweight);
        }


        //Methods

        //Calculate nodal selfweight
        List<Vector3d> calcNodalSelfweight(List<double> lengths, double area, double rho)
        {
            List<Vector3d> nodalForces = new List<Vector3d>();

            Vector3d dir = new Vector3d(0, 0, -1);

            for (int i=0; i<lengths.Count; i++)
            {
                Vector3d force = dir * lengths[i] * area * 1e-6 * rho * 9.82;      //Units: [N]
                nodalForces.Add(force);
            }

            return nodalForces;
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