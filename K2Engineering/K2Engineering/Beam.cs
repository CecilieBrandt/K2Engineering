using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace K2Engineering
{
    public class Beam : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the Beam class.
        /// </summary>
        public Beam()
          : base("Beam", "Beam",
              "A goal that represents a beam element with biaxial bending and torsion behaviour",
              "K2Eng", "0 Elements")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPlaneParameter("StartPlane", "startPln", "The start plane of the beam", GH_ParamAccess.item);
            pManager.AddPlaneParameter("EndPlane", "endPln", "The end plane of the beam", GH_ParamAccess.item);
            pManager.AddNumberParameter("E-modulus", "E", "Young's modulus in [MPa]", GH_ParamAccess.item);
            pManager.AddNumberParameter("G-modulus", "G", "The shear modulus in [MPa]", GH_ParamAccess.item);
            pManager.AddNumberParameter("Iy", "Iy", "The moment of inertia about the cross section y-axis in [mm4]", GH_ParamAccess.item);
            pManager.AddNumberParameter("Iz", "Iz", "The moment of inertia about the cross section z-axis in [mm4]", GH_ParamAccess.item);
            pManager.AddNumberParameter("It", "It", "The torsional moment of inertia in [mm4]", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Beam", "Beam", "The beam goal", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //Input
            Plane startPln = new Plane();
            DA.GetData(0, ref startPln);

            Plane endPln = new Plane();
            DA.GetData(1, ref endPln);

            double eModulus = 0.0;
            DA.GetData(2, ref eModulus);

            double gModulus = 0.0;
            DA.GetData(3, ref gModulus);

            double inertiaY = 0.0;
            DA.GetData(4, ref inertiaY);

            double inertiaZ = 0.0;
            DA.GetData(5, ref inertiaZ);

            double inertiaT = 0.0;
            DA.GetData(6, ref inertiaT);


            //Calculate
            GoalObject beamElement = new BeamGoal(startPln, endPln, eModulus, gModulus, inertiaY, inertiaZ, inertiaT);


            //Output
            DA.SetData(0, beamElement);
        }

        //Define beam goal
        public class BeamGoal : GoalObject
        {
            Plane P0;
            Plane P1;
            double restLength;
            double E;
            double G;
            double Iy;
            double Iz;
            double It;

            //From K2 goal
            Plane P0R;
            Plane P1R;

            double TX1, TX2, TY1, TY2, twist;

            public BeamGoal(Plane startPlane, Plane endPlane, double eModulus, double gModulus, double inertiaY, double inertiaZ, double inertiaT)
            {
                PPos = new Point3d[2] {startPlane.Origin, endPlane.Origin};
                Move = new Vector3d[2];
                Weighting = new double[2] {1.0, 1.0};           //Units: [N*m2]

                Torque = new Vector3d[2];
                TorqueWeighting = new double[2] { 1.0, 1.0 };

                //Other
                P0 = startPlane;
                P1 = endPlane;
                restLength = startPlane.Origin.DistanceTo(endPlane.Origin);
                E = eModulus;
                G = gModulus;
                Iy = inertiaY;
                Iz = inertiaZ;
                It = inertiaT;

                //From K2 goal
                P0.Transform(Transform.ChangeBasis(Plane.WorldXY, StartNode));
                P1.Transform(Transform.ChangeBasis(Plane.WorldXY, EndNode));
                P0R = startPlane;
                P1R = endPlane;
            }


            public override void Calculate(List<KangarooSolver.Particle> p)
            {
                Point3d P0 = p[PIndex[0]].Position;
                Point3d P1 = p[PIndex[1]].Position;

                Vector3d V01 = P1 - P0;

                //From K2 goal
                //get the current positions/orientations of the nodes
                Plane NodeACurrent = p[PIndex[0]].Orientation;
                Plane NodeBCurrent = p[PIndex[1]].Orientation;

                //get the initial orientations of the beam end frames..
                P0R = P0;
                P1R = P1;
                //..and transform them to get the current beam end frames
                P0R.Transform(Transform.PlaneToPlane(Plane.WorldXY, NodeACurrent));
                P1R.Transform(Transform.PlaneToPlane(Plane.WorldXY, NodeBCurrent));

                //axial (ignoring elongation due to bowing for now)
                Vector3d Current = P1R.Origin - P0R.Origin;
                double CurrentLength = Current.Length;
                double Stretch = CurrentLength - RestLength;
                Vector3d AxialMove = 0.5 * (Current / CurrentLength) * Stretch;

                Vector3d X1 = P0R.XAxis;
                Vector3d Y1 = P0R.YAxis;
                Vector3d X2 = P1R.XAxis;
                Vector3d Y2 = P1R.YAxis;

                //bend angles
                Vector3d UnitCurrent = Current;
                UnitCurrent.Unitize();
                TX1 = Y1 * UnitCurrent;
                TX2 = Y2 * UnitCurrent;
                TY1 = X1 * UnitCurrent;
                TY2 = X2 * UnitCurrent;

                //twist
                twist = ((X1 * Y2) - (X2 * Y1)) / 2.0;

                //moments
                Vector3d Moment1 = (X1 * TX1) - (Y1 * TY1);
                Vector3d Moment2 = (X2 * TX2) - (Y2 * TY2);

                Torque[0] = -0.25 * (Moment1 + twist * Current);
                Torque[1] = -0.25 * (Moment2 - twist * Current);
                TorqueWeighting[0] = TorqueWeighting[1] = E * A;

                //  shears
                Vector3d SY1 = 0.25 * Vector3d.CrossProduct(TX1 * X1, Current);
                Vector3d SX1 = 0.25 * Vector3d.CrossProduct(TY1 * Y1, Current);
                Vector3d SY2 = 0.25 * Vector3d.CrossProduct(TX2 * X2, Current);
                Vector3d SX2 = 0.25 * Vector3d.CrossProduct(TY2 * Y2, Current);

                Move[0] = AxialMove + SX1 - SY1 + SX2 - SY2;
                Move[1] = -Move[0];




            }


            //Output moment in [kNm] and stress in [MPa]
            public override object Output(List<KangarooSolver.Particle> p)
            {
                List<object> DataOut = new List<object>();

                DataOut.Add(P0R);
                DataOut.Add(P1R);
                DataOut.Add(TX1);
                DataOut.Add(TX2);
                DataOut.Add(TY1);
                DataOut.Add(TY2);
                DataOut.Add(twist);

                return DataOut;

                //Create beam data object to store output information
                //DataTypes.RodData rodData = new DataTypes.RodData(PIndex[1], pl, moment * 1e-6, bendingStress);
                //return rodData;
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
                return null;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("f76cfabb-06ab-4869-a60d-c6c4865320ea"); }
        }
    }
}