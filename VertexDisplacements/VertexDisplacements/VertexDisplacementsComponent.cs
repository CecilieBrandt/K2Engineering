using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace VertexDisplacements
{
    public class VertexDisplacementsComponent : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public VertexDisplacementsComponent()
            : base("Displacements", "Displ",
                "Calculate the displacement of each point [mm]",
                "K2Struct", "Results")
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
            pManager.AddVectorParameter("TotalDisplacement", "dSum", "The total displacement of each point [mm]", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
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
                displSum.Add(displ);
            }


            //Output
            DA.SetDataList(0, displSum);
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
            get { return new Guid("{3aef34ae-8abd-46f9-8490-c49464925a9a}"); }
        }
    }
}
