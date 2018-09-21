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
              "A support with output of reaction force in [kN] and reaction moment in [kNm]",
              "K2Eng", "1 Supports")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPlaneParameter("SupportPlane", "pl", "The global support plane. By default the support type is set to rigid", GH_ParamAccess.item);
            pManager.AddBooleanParameter("XFixed", "X", "Set to true if the X direction is fixed", GH_ParamAccess.item, true);
            pManager.AddBooleanParameter("YFixed", "Y", "Set to true if the Y direction is fixed", GH_ParamAccess.item, true);
            pManager.AddBooleanParameter("ZFixed", "Z", "Set to true if the Z direction is fixed", GH_ParamAccess.item, true);
            pManager.AddBooleanParameter("RXFixed", "RX", "Set to true if the rotation about the X axis is fixed", GH_ParamAccess.item, true);
            pManager.AddBooleanParameter("RYFixed", "RY", "Set to true if the rotation about the Y axis is fixed", GH_ParamAccess.item, true);
            pManager.AddBooleanParameter("RZFixed", "RZ", "Set to true if the rotation about the Z axis is fixed", GH_ParamAccess.item, true);
            pManager.AddNumberParameter("Strength", "strength", "The strength of a spring to fix the point in the desired directions", GH_ParamAccess.item, 1e15);
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
            Plane plane = new Plane();
            DA.GetData(0, ref plane);

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

            double weight = 1.0;
            DA.GetData(7, ref weight);


            //Create support goal
            GoalObject support = new Support6DOFGoal(plane, isXFixed, isYFixed, isZFixed, isRXFixed, isRYFixed, isRZFixed, weight);


            //Output
            DA.SetData(0, support);
        }

        //Define goal
        public class Support6DOFGoal : GoalObject
        {
            Plane TargetPlane = new Plane();
            bool xFixed;
            bool yFixed;
            bool zFixed;
            bool rxFixed;
            bool ryFixed;
            bool rzFixed;

            public Support6DOFGoal(Plane pl, bool x, bool y, bool z, bool rx, bool ry, bool rz, double k)
            {
                TargetPlane = pl;
                xFixed = x;
                yFixed = y;
                zFixed = z;
                rxFixed = rx;
                ryFixed = ry;
                rzFixed = rz;

                PPos = new Point3d[1] {pl.Origin};
                Move = new Vector3d[1];
                Weighting = new double[1] {k};

                InitialOrientation = new Plane[3] {pl, pl, pl};
                Torque = new Vector3d[3];
                TorqueWeighting = new double[3] {k, k, k};
            }

            public override void Calculate(List<KangarooSolver.Particle> p)
            {
                Plane currentPlane = p[PIndex[0]].Orientation;

                //Translation
                Vector3d translation = TargetPlane.Origin - currentPlane.Origin;
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

                //X-axis rotation vector
                Vector3d RX = Vector3d.CrossProduct(currentPlane.XAxis, TargetPlane.XAxis);     //vector perpendicular to the two axes (original and updated plane axes)
                double angleRX = Math.Asin(RX.Length);                                          //angle bewteen the two vectors
                RX.Unitize();
                RX *= angleRX;                                                                  //angle-axis representation

                //Y-axis rotation vector
                Vector3d RY = Vector3d.CrossProduct(currentPlane.YAxis, TargetPlane.YAxis);
                double angleRY = Math.Asin(RY.Length);
                RY.Unitize();
                RY *= angleRY;

                //Z-axis rotation vector
                Vector3d RZ = Vector3d.CrossProduct(currentPlane.ZAxis, TargetPlane.ZAxis);
                double angleRZ = Math.Asin(RZ.Length);
                RZ.Unitize();
                RZ *= angleRZ;

                //releases
                if (!rxFixed)
                {
                    RX = new Vector3d(0,0,0);
                    TorqueWeighting[0] = 1.0;
                }

                if (!ryFixed)
                {
                    RY = new Vector3d(0, 0, 0);
                    TorqueWeighting[1] = 1.0;
                }

                if (!rzFixed)
                {
                    RZ = new Vector3d(0, 0, 0);
                    TorqueWeighting[2] = 1.0;
                }

                //rotation vector
                Torque[0] = RX;
                Torque[1] = RY;
                Torque[2] = RZ;
            }

            //Output position of support and reaction force. Force in [kN]
            public override object Output(List<KangarooSolver.Particle> p)
            {
                //TODO: Create support data object to store output information
                var supportData = new List<Object>();
                supportData.Add(p[PIndex[0]].Orientation);
                supportData.Add(Move[0] * Weighting[0] * 1e-3);             //Reaction force in [kN]
                supportData.Add(Torque[0] * TorqueWeighting[0] * 1e-3);     //Reaction moment in [kNm]
                supportData.Add(Torque[1] * TorqueWeighting[1] * 1e-3);     //Reaction moment in [kNm]
                supportData.Add(Torque[2] * TorqueWeighting[2] * 1e-3);     //Reaction moment in [kNm]

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
                return null;
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