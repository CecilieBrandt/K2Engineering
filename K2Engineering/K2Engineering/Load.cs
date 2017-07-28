using System;
using System.Collections.Generic;
using KangarooSolver;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace K2Engineering
{
    public class Load : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the Load class.
        /// </summary>
        public Load()
          : base("Load", "Load",
              "A K2 nodal load goal",
              "K2Eng", "2 Load")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Point", "pt", "The point where the load acts", GH_ParamAccess.item);
            pManager.AddVectorParameter("NodalForce", "F", "The nodal force in [N]", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("LoadGoal", "L", "The K2 nodal load goal which outputs the updated position of the point where the load acts and the load in [kN]", GH_ParamAccess.item);
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

            Vector3d force = new Vector3d();
            DA.GetData(1, ref force);


            //Calculate
            GoalObject load = new LoadGoal(pt, force);

            //Output
            DA.SetData(0, load);
        }


        public class LoadGoal : GoalObject
        {
            Vector3d load;

            public LoadGoal(Point3d pt, Vector3d force)
            {
                load = force;

                PPos = new Point3d[1] {pt};
                Move = new Vector3d[1] {load};
                Weighting = new double[1] {1.0};
            }

            
            public override void Calculate(List<KangarooSolver.Particle> p)
            {
                Move[0] = load;
            }
            

            //Output updated position of node where the load acts. Load in [kN]
            public override object Output(List<KangarooSolver.Particle> p)
            {
                //var Data = new object[2] {p[PIndex[0]].Position, Move[0] * Weighting[0] * 1e-3};
                DataTypes.PointLoadData Data = new DataTypes.PointLoadData(p[PIndex[0]].Position, Move[0] * Weighting[0] * 1e-3);
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
                return Properties.Resources.Load;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("{64e19caa-a444-4ac4-bbc1-5bf4d62dd406}"); }
        }
    }
}