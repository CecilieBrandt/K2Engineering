using System;
using System.Collections.Generic;
using KangarooSolver;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace ElasticRodElement
{
    public class ElasticRodElementComponent : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public ElasticRodElementComponent()
            : base("RodElement", "Rod",
                "A goal that represents an elastic rod with bending stiffness only. It outputs the bending plane, bending moment [kNm] and the bending stress [MPa]",
                "K2Struct", "Elements")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddLineParameter("LineA", "LnA", "Line representing the first segment [mm]", GH_ParamAccess.item);
            pManager.AddLineParameter("LineB", "LnB", "Line representing the consecutive segment [mm]", GH_ParamAccess.item);
            pManager.AddNumberParameter("E-Modulus", "E", "E-Modulus of the material [MPa]", GH_ParamAccess.item);
            pManager.AddNumberParameter("Inertia", "I", "The moment of inertia [mm4]", GH_ParamAccess.item);
            pManager.AddNumberParameter("z-distance", "z", "The distance from the section axis to the extreme fiber [mm]", GH_ParamAccess.item);
            pManager.AddIntegerParameter("RestAngleOption", "Opt", "Specify the rest angle. 0: straight. 1: current angle", GH_ParamAccess.item, 0);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("R", "Rod", "Elastic rod element with moment and stress output", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //Input
            Line lineA = new Line();
            DA.GetData(0, ref lineA);

            Line lineB = new Line();
            DA.GetData(1, ref lineB);

            double eModulus = 0.0;
            DA.GetData(2, ref eModulus);

            double inertia = 0.0;
            DA.GetData(3, ref inertia);

            double zDist = 0.0;
            DA.GetData(4, ref zDist);

            int angleOpt = 0;
            DA.GetData(5, ref angleOpt);
            if (angleOpt < 0)
            {
                angleOpt = 0;
            }
            else if (angleOpt > 1)
            {
                angleOpt = 1;
            }


            //Create instance of elastic rod element
            GoalObject rodElement = new Rod(lineA, lineB, eModulus, inertia, zDist, angleOpt);


            //Output
            DA.SetData(0, rodElement);
        }

        public class Rod : GoalObject
        {
            Line lineA;
            Line lineB;
            double eModulus;
            double inertia;
            double zDist;
            double restAngle;

            public Rod(Line LA, Line LB, double E, double I, double z, int opt)
            {
                //K2 requirements
                PPos = new Point3d[4] { LA.From, LA.To, LB.From, LB.To };     // PPos must contain an array of the points this goal acts on
                if (LA.To.CompareTo(LB.From) != 0)
                {
                    PPos[2] = LB.To;
                    PPos[3] = LB.From;
                }
                Move = new Vector3d[4];       // Move is an array of vectors, one for each PPos
                Weighting = new double[4] { E * I, E * I, E * I, E * I }; // Weighting is an array of doubles for how strongly the goal affects each point 

                //Other
                lineA = LA;
                lineB = LB;
                eModulus = E;
                inertia = I;
                zDist = z;

                if (opt == 0)
                {
                    restAngle = Math.PI;
                }
                else if (opt == 1)
                {
                    restAngle = Vector3d.VectorAngle(new Vector3d(PPos[0] - PPos[1]), new Vector3d(PPos[3] - PPos[2]));
                }
            }


            public override void Calculate(List<KangarooSolver.Particle> p)
            {
                Point3d P0 = p[PIndex[0]].Position;             //get the current position of the particle at the start of the line
                Point3d P1 = p[PIndex[1]].Position;
                Point3d P2 = p[PIndex[2]].Position;
                Point3d P3 = p[PIndex[3]].Position;

                Vector3d V01 = P1 - P0;
                Vector3d V23 = P3 - P2;
                Vector3d V03 = P3 - P0;
                double currentAngle = restAngle - Vector3d.VectorAngle(-V01, V23);

                Vector3d n = Vector3d.CrossProduct(-V01, V23);
                Vector3d shearA = Vector3d.CrossProduct(-V01, n);
                Vector3d shearB = Vector3d.CrossProduct(V23, n);
                shearA.Unitize();
                shearB.Unitize();

                double shearAVal = (2.0 * Math.Sin(currentAngle)) / (V01.Length * V03.Length);
                double shearBVal = (2.0 * Math.Sin(currentAngle)) / (V23.Length * V03.Length);

                shearA *= shearAVal;
                shearB *= shearBVal;

                Move[0] = shearA;
                Move[1] = -shearA;
                Move[2] = shearB;
                Move[3] = -shearB;

            }

            
            //Stress at the point between two line segments
            public override object Output(List<KangarooSolver.Particle> p)
            {
                Point3d P0 = p[PIndex[0]].Position;             //get the current position of the particle at the start of the line
                Point3d P1 = p[PIndex[1]].Position;
                Point3d P2 = p[PIndex[2]].Position;
                Point3d P3 = p[PIndex[3]].Position;
                Vector3d V01 = P1 - P0;
                Vector3d V23 = P3 - P2;

                double moment = Move[0].Length * Weighting[0] * V01.Length;
                double bendingStress = (moment * zDist) / inertia;

                V01.Unitize();
                V23.Unitize();

                Plane pl = new Plane(P1, V01, V23);
                Vector3d average = (-V01 + V23) / 2.0;

                if (average.Length == 0.0 && Move[1].Length != 0.0)      //if the vectors are parallel and the move vector is non-zero then use the move vector to specify bending plane. This happens if the structure is straight and tries to maintain that state but is influenced by out-of-plane forces
                {
                    pl = new Plane(P1, V01, Move[1]);
                    average = Move[1];
                }

                average.Unitize();
                double rotation = Vector3d.VectorAngle(pl.YAxis, average);
                pl.Rotate(rotation, pl.ZAxis);

                //Output the particle index of the shared point between the two consecutive line segments, the bending plane, the moment [kNm] and the stress [MPa]
                var Data = new Object[4];
                Data[0] = PIndex[1];
                Data[1] = pl;
                Data[2] = moment*1e-6;
                Data[3] = bendingStress;

                return Data;
            }

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
            get { return new Guid("{a352c800-8324-4366-9bba-9e44dd01c4be}"); }
        }
    }
}
