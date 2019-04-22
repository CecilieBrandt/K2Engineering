using System;
using System.Collections.Generic;
using KangarooSolver;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace K2Engineering
{
    public class Support6DOF : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the Support6DOF class.
        /// </summary>
        public Support6DOF()
          : base("Support6DOF", "Support6DOF",
              "A 6 DOF support (global coordinate system) with output of reaction force in [kN] and reaction moment in [kNm]",
              "K2Eng", "1 Supports")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("SupportPt", "pt", "The support point. By default the support type is set to fully fixed", GH_ParamAccess.item);
            pManager.AddBooleanParameter("XFixed", "X", "Set to true if the X direction is fixed", GH_ParamAccess.item, true);
            pManager.AddBooleanParameter("YFixed", "Y", "Set to true if the Y direction is fixed", GH_ParamAccess.item, true);
            pManager.AddBooleanParameter("ZFixed", "Z", "Set to true if the Z direction is fixed", GH_ParamAccess.item, true);
            pManager.AddBooleanParameter("RXFixed", "RX", "Set to true if the rotation about the X axis is fixed", GH_ParamAccess.item, true);
            pManager.AddBooleanParameter("RYFixed", "RY", "Set to true if the rotation about the Y axis is fixed", GH_ParamAccess.item, true);
            pManager.AddBooleanParameter("RZFixed", "RZ", "Set to true if the rotation about the Z axis is fixed", GH_ParamAccess.item, true);
            pManager.AddNumberParameter("Strength", "strength", "The strength of the support", GH_ParamAccess.item, 1e9);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Support6DOFGoal", "S", "The 6 DOF support goal", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //Input
            Point3d pt = new Point3d();
            DA.GetData(0, ref pt);

            bool isXFixed = true;
            DA.GetData(1, ref isXFixed);

            bool isYFixed = true;
            DA.GetData(2, ref isYFixed);

            bool isZFixed = true;
            DA.GetData(3, ref isZFixed);

            bool isRXFixed = true;
            DA.GetData(4, ref isRXFixed);

            bool isRYFixed = true;
            DA.GetData(5, ref isRYFixed);

            bool isRZFixed = true;
            DA.GetData(6, ref isRZFixed);

            double strength = 1e9;
            DA.GetData(7, ref strength);

            this.Message = "WIP";


            //Create support goal
            GoalObject support = new Support6DOFGoal(pt, isXFixed, isYFixed, isZFixed, isRXFixed, isRYFixed, isRZFixed, strength);


            //Output
            DA.SetData(0, support);
        }

        //Define goal
        public class Support6DOFGoal : GoalObject
        {
            public Plane plnOrig;
            bool xFixed;
            bool yFixed;
            bool zFixed;
            bool rxFixed;
            bool ryFixed;
            bool rzFixed;

            public Support6DOFGoal(Point3d pt, bool x, bool y, bool z, bool rx, bool ry, bool rz, double k)
            {
                plnOrig = Plane.WorldXY;
                plnOrig.Origin = pt;

                xFixed = x;
                yFixed = y;
                zFixed = z;
                rxFixed = rx;
                ryFixed = ry;
                rzFixed = rz;

                PPos = new Point3d[1] {pt};
                Move = new Vector3d[1];
                Weighting = new double[1] {k};

                InitialOrientation = new Plane[1] { plnOrig };
                Torque = new Vector3d[1];
                TorqueWeighting = new double[1] {k};
            }

            public override void Calculate(List<KangarooSolver.Particle> p)
            {
                Plane plnCurrent = p[PIndex[0]].Orientation;

                //Translation
                Vector3d translation = new Vector3d(plnOrig.Origin - plnCurrent.Origin);
                if (!xFixed)
                {
                    translation.X = 0.0;
                }

                if (!yFixed)
                {
                    translation.Y = 0.0;
                }

                if (!zFixed)
                {
                    translation.Z = 0.0;
                }

                Move[0] = translation;


                //Rotation
                Quaternion q = Quaternion.Rotation(plnCurrent, plnOrig);

                double angle = new double();
                Vector3d axis = new Vector3d();
                q.GetRotation(out angle, out axis);

                if (angle > Math.PI)
                {
                    angle = angle - 2.0 * Math.PI;
                }

                Vector3d rotation = Vector3d.Multiply(axis, angle);

                if (!rxFixed)
                {
                    rotation.X = 0.0;
                }

                if (!ryFixed)
                {
                    rotation.Y = 0.0;
                }

                if (!rzFixed)
                {
                    rotation.Z = 0.0;
                }

                Torque[0] = rotation;
            }

            //Output position of support and reaction force. Force in [kN]
            public override object Output(List<KangarooSolver.Particle> p)
            {
                Plane pln = p[PIndex[0]].Orientation;
                Vector3d rf = Vector3d.Multiply(Move[0], Weighting[0]) * 1e-3;              //Units [kN]
                Vector3d rm = Vector3d.Multiply(Torque[0], TorqueWeighting[0]) * 1e-3;      //Units [kNm]

                DataTypes.Support6DOFData supportData = new DataTypes.Support6DOFData(pln, rf, rm);
                return supportData;
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
                return Properties.Resources.Support6DOF;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("3e9e7e8a-5f44-4409-93d6-4750f4bb954a"); }
        }
    }
}