using System;
using System.Collections.Generic;
using KangarooSolver;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace K2Engineering
{
    public class Bar : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the Bar class.
        /// </summary>
        public Bar()
          : base("Bar", "Bar",
              "A goal that represents a bar element with axial stiffness only. It outputs the extended/shortened line geometry, the axial force and stress value (- compression, + tension)",
              "K2Eng", "0 Elements")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddLineParameter("Line", "Ln", "Line representing the bar element [m]", GH_ParamAccess.item);
            pManager.AddNumberParameter("E-Modulus", "E", "E-Modulus of the material [MPa]", GH_ParamAccess.item);
            pManager.AddNumberParameter("Area", "A", "Cross-section area [mm2]", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("B", "Bar", "Bar element with force and stress output", GH_ParamAccess.item);
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


            //Create instance of bar
            GoalObject barElement = new BarGoal(line, eModulus, area);


            //Output
            DA.SetData(0, barElement);
        }


        public class BarGoal : GoalObject
        {
            double restLenght;
            bool isCompressionMember;
            double area;

            public BarGoal(Line L, double E, double A)
            {
                restLenght = L.From.DistanceTo(L.To);
                isCompressionMember = true;
                area = A;

                PPos = new Point3d[2] { L.From, L.To };
                Move = new Vector3d[2];
                Weighting = new double[2] { (2 * E * A) / restLenght, (2 * E * A) / restLenght };           // Units: [N/m]
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

                if (extension > 0.0)
                {
                    isCompressionMember = false;
                }
                else if (extension < 0.0)
                {
                    isCompressionMember = true;
                }

                //Set vector direction and magnitude
                Move[0] = forceDir * (extension / 2);                 //has to point to exact point according to Hooke's Law. Divide by 2 as the bar is extended in both directions with the same amount
                Move[1] = -forceDir * (extension / 2);
            }

            //Stress in bar (ONE VALUE PER LINE ELEMENT)
            public override object Output(List<KangarooSolver.Particle> p)
            {
                double factor = 1.0;
                if (isCompressionMember)
                {
                    factor = -1.0;
                }

                double force = factor * Weighting[0] * Move[0].Length;          //Units: [N]

                //output the start and end particle index, the extended/shortened line, the force in [kN] and the stress in [MPa]
                //var Data = new object[5] { PIndex[0], PIndex[1], new Line(p[PIndex[0]].Position, p[PIndex[1]].Position), force / 1000.0, force / area };
                DataTypes.BarData Data = new DataTypes.BarData(PIndex[0], PIndex[1], new Line(p[PIndex[0]].Position, p[PIndex[1]].Position), force / 1000.0, force / area);
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
                return Properties.Resources.Bar;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("{ccf6dc32-7c3c-4836-94c2-e43e1e3c4f0d}"); }
        }
    }
}