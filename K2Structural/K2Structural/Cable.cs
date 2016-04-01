using System;
using System.Collections.Generic;
using KangarooSolver;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace K2Structural
{
    public class Cable : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the Cable class.
        /// </summary>
        public Cable()
          : base("Cable", "Cable",
              "A K2 cable element with pre-stress option",
              "K2Eng", "0 Elements")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddLineParameter("Line", "Ln", "Line representing the cable element [m]", GH_ParamAccess.item);
            pManager.AddNumberParameter("E-Modulus", "E", "E-Modulus of the material [MPa]", GH_ParamAccess.item);
            pManager.AddNumberParameter("Area", "A", "Cross-section area [mm2]", GH_ParamAccess.item);
            pManager.AddNumberParameter("PreTension", "P", "Optional pre-tension [kN]", GH_ParamAccess.item);
            pManager[3].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Cable", "Cable", "Cable element with force and stress output", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //Input
            Line line = new Line();
            DA.GetData(0, ref line);

            double eModulus = 0.0;
            DA.GetData(1, ref eModulus);

            double area = 0.0;
            DA.GetData(2, ref area);

            double preStress = 0.0;
            if (this.Params.Input[3].SourceCount != 0)
            {
                DA.GetData(3, ref preStress);
            }


            //Create instance of bar
            GoalObject cableElement = new CableGoal(line, eModulus, area, preStress);


            //Output
            DA.SetData(0, cableElement);
        }


        public class CableGoal : GoalObject
        {
            double restLenght;
            double area;

            public CableGoal(Line L, double E, double A, double F)
            {
                restLenght = L.From.DistanceTo(L.To);
                area = A;

                PPos = new Point3d[2] { L.From, L.To };
                Move = new Vector3d[2];
                Weighting = new double[2] { (2 * E * A) / restLenght, (2 * E * A) / restLenght };           //Units: [N/m]

                //Adjust restlenght if prestressed bar
                restLenght -= (F * 1000 * restLenght) / (E * A);            //Units: [m]
            }

            public override void Calculate(List<KangarooSolver.Particle> p)
            {
                Point3d ptStart = p[PIndex[0]].Position;             //get the current position of the particle at the start of the line
                Point3d ptEnd = p[PIndex[1]].Position;             //get the current position of the particle at the end of the line

                //Calculate force direction
                Vector3d forceDir = new Vector3d(ptEnd - ptStart);  //force direction pointing from start of line to end
                double currentLength = forceDir.Length;
                forceDir.Unitize();

                //Calculate extension
                double extension = currentLength - restLenght;

                //Test if cable is in tension, otherwise not active (zero force)
                Vector3d forceStart = new Vector3d(0, 0, 0);
                Vector3d forceEnd = new Vector3d(0, 0, 0);
                if (extension > 0.0)
                {
                    forceStart = forceDir * (extension / 2);                //has to point to exact point according to Hooke's Law. Divide by 2 as the bar is extended in both directions with the same amount
                    forceEnd = -forceDir * (extension / 2);
                }

                //Set move vectors
                Move[0] = forceStart;
                Move[1] = forceEnd;
            }

            //Stress in bar (ONE VALUE PER LINE ELEMENT)
            public override object Output(List<KangarooSolver.Particle> p)
            {
                double force = Weighting[0] * Move[0].Length;

                //output the start and end particle index, the extended line, the force in [kN] and the stress in [MPa]
                var Data = new object[5] { PIndex[0], PIndex[1], new Line(p[PIndex[0]].Position, p[PIndex[1]].Position), force / 1000.0, force / area };
                return Data;
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
                return Properties.Resources.Cable;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("{efc6c913-9056-4825-b417-01083d2316bc}"); }
        }
    }
}