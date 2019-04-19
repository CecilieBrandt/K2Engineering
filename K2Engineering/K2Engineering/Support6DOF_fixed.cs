using System;
using System.Collections.Generic;
using KangarooSolver;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace K2Engineering
{
    public class Support6DOF_fixed : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the Support6DOF_fixed class.
        /// </summary>
        public Support6DOF_fixed()
          : base("Support6DOF_fixed", "Support6DOF",
              "A 6 DOF fully fixed support with output of reaction force in [kN] and reaction moment in [kNm]",
              "K2Eng", "1 Supports")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPlaneParameter("SupportPlane", "pl", "The support plane", GH_ParamAccess.item);
            pManager.AddNumberParameter("Strength", "strength", "The strength of the support", GH_ParamAccess.item, 1e12);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Support6DOFGoal", "S", "The 6 DOF fixed support goal", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //Input
            Plane pln = new Plane();
            DA.GetData(0, ref pln);

            double strength = 1e12;
            DA.GetData(1, ref strength);

            this.Message = "WIP";


            //Calculate
            GoalObject support = new Support6DOFFixedGoal(pln, strength);


            //Output
            DA.SetData(0, support);
        }

        //Define goal
        public class Support6DOFFixedGoal : GoalObject
        {
            public Plane plnOrig;
            public Plane plnXY;

            public Support6DOFFixedGoal(Plane pln, double k)
            {
                PPos = new Point3d[1] { pln.Origin };

                plnOrig = pln;
                plnXY = Plane.WorldXY;
                plnXY.Origin = pln.Origin;
                InitialOrientation = new Plane[1] { plnXY };

                Move = new Vector3d[1];
                Torque = new Vector3d[1];

                Weighting = new double[1] { k };
                TorqueWeighting = new double[1] { k };
            }

            public override void Calculate(List<KangarooSolver.Particle> p)
            {
                Plane plnCurrent = p[PIndex[0]].Orientation;
                Move[0] = plnXY.Origin - plnCurrent.Origin;

                Quaternion q = Quaternion.Rotation(plnCurrent, plnXY);

                double angle = new double();
                Vector3d axis = new Vector3d();
                q.GetRotation(out angle, out axis);

                if (angle > Math.PI)
                {
                    angle = angle - 2.0 * Math.PI;
                }

                Torque[0] = Vector3d.Multiply(axis, angle);
            }

            public override object Output(List<KangarooSolver.Particle> p)
            {
                Vector3d rf = Vector3d.Multiply(Move[0], Weighting[0]) * 1e-3;              //Units [kN]
                Vector3d rm = Vector3d.Multiply(Torque[0], TorqueWeighting[0]) * 1e-3;      //Units [kNm]

                //Transform global moment vector to original input plane
                Vector3d rmTrans = transformMoment(rm.X, rm.Y, plnXY, plnOrig);
                rmTrans.Z = rm.Z;

                DataTypes.Support6DOFData supportData = new DataTypes.Support6DOFData(plnOrig, rf, rmTrans);
                return supportData;
            }

            public Vector3d transformMoment(double m1x, double m1y, Plane pln1, Plane pln2)
            {
                //Moment = force * distance. Keep force magnitude equal to moment and use unit distance
                //Mx moment (plane 1) is equivalent to
                Vector3d fx = new Vector3d(0, 0, m1x);
                Vector3d dx = pln1.YAxis;

                //My moment (plane 1) is equivalent to
                Vector3d fy = new Vector3d(0, 0, -m1y);
                Vector3d dy = pln1.XAxis;

                //Add to arrays
                Vector3d[] forces = new Vector3d[2] { fx, fy };
                Vector3d[] distances = new Vector3d[2] { dx, dy };

                //Equivalent moment (plane2)
                Vector3d M2 = new Vector3d(0, 0, 0);

                //run through Mx and My (plane1) contribution to moment (plane2)
                for (int i = 0; i < 2; i++)
                {

                    //Contribution about x-axis (plane2)
                    double dotx = Vector3d.Multiply(distances[i], pln2.YAxis);
                    Vector3d dx2 = Vector3d.Multiply(pln2.YAxis, dotx);

                    Vector3d crossx = Vector3d.CrossProduct(dx2, forces[i]);    //vector parallel to x-axis
                    double m2x = forces[i].Length * dx2.Length;                 //moment magnitude

                    if (Vector3d.Multiply(crossx, pln2.XAxis) < 0)
                    {
                        m2x *= -1;                                                //reverse sign of moment
                    }

                    M2 += new Vector3d(m2x, 0, 0);


                    //Contribution about y-axis (plane2)
                    double doty = Vector3d.Multiply(distances[i], pln2.XAxis);
                    Vector3d dy2 = Vector3d.Multiply(pln2.XAxis, doty);

                    Vector3d crossy = Vector3d.CrossProduct(dy2, forces[i]);    //vector parallel to x-axis
                    double m2y = forces[i].Length * dy2.Length;                 //moment magnitude

                    if (Vector3d.Multiply(crossy, pln2.YAxis) < 0)
                    {
                        m2y *= -1;                                                //reverse sign of moment
                    }

                    M2 += new Vector3d(0, m2y, 0);
                }

                return M2;
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
                return Properties.Resources.Support6DOF_fixed;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("1da743cd-3e13-4656-8115-9afaf7baae9b"); }
        }
    }
}