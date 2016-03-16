using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace K2Structural
{
    public class Displacements : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the Displacements class.
        /// </summary>
        public Displacements()
          : base("Displacements", "Displ",
              "Calculate the nodal displacements [mm]",
              "K2Structural", "3 Results")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("InitialPositions", "pInit", "The initial positions", GH_ParamAccess.list);
            pManager.AddPointParameter("FinalPositions", "pFinal", "The final positions", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddVectorParameter("TotalDisplacement", "dSum", "The total displacement of each node [mm]", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //Input
            List<Point3d> initialPositions = new List<Point3d>();
            DA.GetDataList(0, initialPositions);

            List<Point3d> finalPositions = new List<Point3d>();
            DA.GetDataList(1, finalPositions);


            //Calculate displacements
            List<Vector3d> displSum = new List<Vector3d>();

            for (int i = 0; i < initialPositions.Count; i++)
            {
                Vector3d displ = finalPositions[i] - initialPositions[i];
                displ *= 1000;
                displSum.Add(displ);
            }


            //Output
            DA.SetDataList(0, displSum);
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
                return Properties.Resources.Displacement;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("{d18d48d0-d9ed-4f1f-9712-70fd08a395c1}"); }
        }
    }
}