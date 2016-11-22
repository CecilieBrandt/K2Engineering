using System;
using System.Collections.Generic;
using KangarooSolver;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace K2Engineering
{
    public class Support : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the Support class.
        /// </summary>
        public Support()
          : base("Support", "Support",
              "A support with output of reaction force in kN",
              "K2Eng", "1 Supports")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("PinnedSupportPt", "pt", "The point to function as a support", GH_ParamAccess.item);
            pManager.AddBooleanParameter("XFixed", "X", "Set to true if the X direction is fixed", GH_ParamAccess.item, true);
            pManager.AddBooleanParameter("YFixed", "Y", "Set to true if the Y direction is fixed", GH_ParamAccess.item, true);
            pManager.AddBooleanParameter("ZFixed", "Z", "Set to true if the Z direction is fixed", GH_ParamAccess.item, true);
            pManager.AddNumberParameter("Strength", "strength", "The strength of a spring to fix the point in the desired directions", GH_ParamAccess.item, 1e15);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("SimpleSupportGoal", "S", "The simple support goal", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //Input
            Point3d supportPt = new Point3d();
            DA.GetData(0, ref supportPt);

            bool isXFixed = true;
            DA.GetData(1, ref isXFixed);

            bool isYFixed = true;
            DA.GetData(2, ref isYFixed);

            bool isZFixed = true;
            DA.GetData(3, ref isZFixed);

            double weight = 1.0;
            DA.GetData(4, ref weight);

            //Warning if no direction is fixed
            if (!isXFixed && !isYFixed && !isZFixed)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "The specified point is free to move!");
            }


            //Create simple support goal
            GoalObject support = new SupportGoal(supportPt, isXFixed, isYFixed, isZFixed, weight);


            //Output
            DA.SetData(0, support);
        }


        //Define goal
        public class SupportGoal : GoalObject
        {
            Point3d Target = new Point3d();
            bool xFixed;
            bool yFixed;
            bool zFixed;

            public SupportGoal(Point3d Pt, bool x, bool y, bool z, double k)
            {
                PPos = new Point3d[1] { Pt };     // PPos must contain an array of the points this goal acts on
                Move = new Vector3d[1];       // Move is an array of vectors, one for each PPos
                Weighting = new double[1] { k }; // Weighting is an array of doubles for how strongly the goal affects each point

                Target = Pt;
                xFixed = x;
                yFixed = y;
                zFixed = z;
            }

            public override void Calculate(List<KangarooSolver.Particle> p)
            {
                Point3d currentPt = p[PIndex[0]].Position;             //get the current position of the particle
                Vector3d moveTotal = Target - currentPt;

                if (!xFixed)
                {
                    moveTotal.X = 0.0;
                }

                if (!yFixed)
                {
                    moveTotal.Y = 0.0;
                }

                if (!zFixed)
                {
                    moveTotal.Z = 0.0;
                }

                Move[0] = moveTotal;
            }

            public override object Output(List<KangarooSolver.Particle> p)
            {
                var Data = new object[2] { p[PIndex[0]].Position, Move[0] * Weighting[0] * 1e-3 };                //output support point with reaction force in KN

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
                return Properties.Resources.Support;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("{2672a780-8cb4-4da5-bd5f-67ddcbeed60b}"); }
        }
    }
}