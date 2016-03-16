using System;
using System.Collections.Generic;
using System.Linq;
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
              "Calculate the nodal displacements",
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
            pManager.AddVectorParameter("TotalDisplacements", "vDispl", "The total displacement of each node [mm]", GH_ParamAccess.list);
            pManager.AddNumberParameter("MaximumDisplacementX", "Xmax", "The maximum displacement in the x direction [mm]", GH_ParamAccess.item);
            pManager.AddNumberParameter("MaximumDisplacementY", "Ymax", "The maximum displacement in the y direction [mm]", GH_ParamAccess.item);
            pManager.AddNumberParameter("MaximumDisplacementZ", "Zmax", "The maximum displacement in the z direction [mm]", GH_ParamAccess.item);
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
            List<double> displX = new List<double>();
            List<double> displY = new List<double>();
            List<double> displZ = new List<double>();

            for (int i = 0; i < initialPositions.Count; i++)
            {
                Vector3d displ = finalPositions[i] - initialPositions[i];
                displ *= 1000;

                displSum.Add(displ);
                displX.Add(displ.X);
                displY.Add(displ.Y);
                displZ.Add(displ.Z);
            }

            //Maximum values
            double xMax = calcMaximumAbsVal(displX);
            double yMax = calcMaximumAbsVal(displY);
            double zMax = calcMaximumAbsVal(displZ);

            //Output
            DA.SetDataList(0, displSum);
            DA.SetData(1, xMax);
            DA.SetData(2, yMax);
            DA.SetData(3, zMax);
        }

        //Method
        //Calculate the maximum absolute value from a list
        double calcMaximumAbsVal(List<double> values)
        {
            double max = values.Max();
            double min = values.Min();

            double maxAbsVal = max;
            if(Math.Abs(min) > max)
            {
                maxAbsVal = min;
            }

            return maxAbsVal;
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