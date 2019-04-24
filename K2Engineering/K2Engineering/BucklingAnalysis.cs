using System;
using System.Collections.Generic;
using KangarooSolver;
using Grasshopper.Kernel;
using Rhino.Geometry;
using Grasshopper;
using Grasshopper.Kernel.Data;

namespace K2Engineering
{
    public class BucklingAnalysis : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the BucklingAnalysis class.
        /// </summary>
        public BucklingAnalysis()
          : base("BucklingAnalysis", "Buckling",
              "Calculate the buckling load factor from a nonlinear load-displacement analysis",
              "K2Eng", "3 Results")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Goals", "PGoals", "Permanent goals e.g. bar, rod and support goals as FLATTENED list", GH_ParamAccess.list);
            pManager.AddGenericParameter("Load", "LGoals", "K2Structural load goals", GH_ParamAccess.list);
            pManager.AddNumberParameter("LFStart", "LFstart", "The start load factor", GH_ParamAccess.item);
            pManager.AddNumberParameter("LFStep", "LFstep", "The load factor step size", GH_ParamAccess.item);
            pManager.AddNumberParameter("Angle", "alfa", "Specify the allowed angle deviation between the tangent to the load-displacement curve and vertical in [degrees]", GH_ParamAccess.item, 15.0);
            pManager.AddNumberParameter("Displ", "dMax", "Specify the maximum allowed displacement for a vertex in [m]", GH_ParamAccess.item, 1.0);
            pManager.AddNumberParameter("Threshold", "thres", "The total kinetic energy threshold to define equilibrium", GH_ParamAccess.item, 1e-15);
            pManager.AddIntegerParameter("IterationCount", "iter", "Specify the maximum number of equilibrium iterations for each load increment", GH_ParamAccess.item, 100);
            pManager.AddBooleanParameter("OutputAll?", "opt", "Output option. If true, displacements and goal results are output for each load increment. If false, only for the last two", GH_ParamAccess.item, false);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("BucklingLoadFactor", "BLF", "The buckling load factor [-]", GH_ParamAccess.item);
            pManager.AddNumberParameter("LoadFactors", "LF", "Load factor for each iteration", GH_ParamAccess.list);
            pManager.AddNumberParameter("DisplacementRMS", "dRMS", "The displacements as a RMS value in [m] for each iteration", GH_ParamAccess.list);
            pManager.AddPointParameter("VertexPositions", "V", "The vertex positions for each iteration", GH_ParamAccess.tree);
            pManager.AddGenericParameter("GoalOutput", "O", "The output from the goals for each iteration. The data structure is identical to the PGoals input", GH_ParamAccess.tree);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //-----------------------------------------------------------------INPUT----------------------------------------------------------------------------//

            List<IGoal> permanentGoals = new List<IGoal>();
            DA.GetDataList(0, permanentGoals);

            List<IGoal> loadGoals = new List<IGoal>();
            DA.GetDataList(1, loadGoals);

            double fStart = 1.0;
            DA.GetData(2, ref fStart);

            double fStep = 0.1;
            DA.GetData(3, ref fStep);

            double angle = 15.0;
            DA.GetData(4, ref angle);
            angle *= Math.PI / 180.0;

            double displ = 2.0;
            DA.GetData(5, ref displ);

            double threshold = 1e-15;
            DA.GetData(6, ref threshold);

            int equilibriumIter = 100;
            DA.GetData(7, ref equilibriumIter);

            bool opt = false;
            DA.GetData(8, ref opt);

            int maxIterations = 1000;


            //--------------------------------------------------------------CALCULATE----------------------------------------------------------------------------//

            //-------------------VALUES TO STORE----------------------------//
            //Position lists
            List<Point3d> initialPositions = new List<Point3d>();
            List<Point3d> previousPositions = new List<Point3d>();
            List<Point3d> currentPositions = new List<Point3d>();
            DataTree<Point3d> vertexPositions = new DataTree<Point3d>();

            //Goal output lists
            List<object> previousGOutput = new List<object>();
            List<object> currentGOutput = new List<object>();
            DataTree<object> outputGoals = new DataTree<object>();

            //Load factors and displacements
            List<double> loadfactors = new List<double>();
            List<double> displacementsRMS = new List<double>();


            //-------------------K2 PHYSICAL SYSTEM----------------------------//
            //Initialise Kangaroo solver
            var PS = new KangarooSolver.PhysicalSystem();
            var GoalList = new List<IGoal>();

            //Assign indexes to the particles in each Goal
            foreach (IGoal pG in permanentGoals)
            {
                PS.AssignPIndex(pG, 0.01);
                GoalList.Add(pG);
            }

            foreach (IGoal lG in loadGoals)
            {
                PS.AssignPIndex(lG, 0.01);
                GoalList.Add(lG);
            }

            //Store initial loads
            List<Vector3d> initialLoads = new List<Vector3d>();
            for (int i = 0; i < loadGoals.Count; i++)
            {
                initialLoads.Add(loadGoals[i].Move[0]);
            }


            //-------------------INITIALISE VALUE LISTS----------------------------//
            //Initial vertex positions
            Point3d[] initPos = PS.GetPositionArray();
            foreach (Point3d pt in initPos)
            {
                initialPositions.Add(pt);
                previousPositions.Add(pt);
                currentPositions.Add(pt);
            }

            //Initial goal output
            List<object> initGOutput = PS.GetOutput(GoalList);
            for (int i = 0; i < permanentGoals.Count; i++)
            {
                previousGOutput.Add(initGOutput[i]);
                currentGOutput.Add(initGOutput[i]);
            }


            //-------------------LOAD INCREMENT LOOP----------------------------//
            bool run = true;
            int iter = 0;

            double LF;
            double BLF = 0.0;
            double preRMS = 0.0;

            while (run && iter < maxIterations)
            {
                LF = fStart + (fStep * iter);
                loadfactors.Add(LF);

                //Scale load goals in each iteration
                for (int i = 0; i < loadGoals.Count; i++)
                {
                    int index = GoalList.Count - loadGoals.Count + i;
                    Vector3d scaledLoad = initialLoads[i] * LF;
                    GoalList[index] = new KangarooSolver.Goals.Unary(GoalList[index].PIndex[0], scaledLoad);
                }


                //Solve equilibrium for given load increment
                int counter = 0;
                do
                {
                    PS.Step(GoalList, true, threshold);
                    counter++;
                } while (PS.GetvSum() > threshold && counter < equilibriumIter);



                //Update value lists
                GH_Path path = new GH_Path(iter);

                //Get new equilibrium positions and update position lists
                Point3d[] newPositions = PS.GetPositionArray();

                for (int k = 0; k < initialPositions.Count; k++)
                {
                    previousPositions[k] = currentPositions[k];
                    currentPositions[k] = newPositions[k];

                    if (opt)
                    {
                        vertexPositions.Add(newPositions[k], path);
                    }
                }

                //Get new goal output and update goal output lists
                List<object> newGOutput = PS.GetOutput(GoalList);
                for (int m = 0; m < permanentGoals.Count; m++)
                {
                    previousGOutput[m] = currentGOutput[m];
                    currentGOutput[m] = newGOutput[m];

                    if (opt)
                    {
                        outputGoals.Add(newGOutput[m], path);
                    }
                }



                //Does buckling occur?
                List<Vector3d> nodalDisplacements = calcDisplacement(currentPositions, initialPositions);
                double curRMS = calcDisplacementsRMS(nodalDisplacements);
                displacementsRMS.Add(curRMS);

                bool buckled = isBuckled(curRMS, preRMS, iter, fStart, fStep, angle);
                bool deflected = isDeflectionTooBig(nodalDisplacements, displ);

                if (buckled || deflected)
                {
                    run = false;
                    BLF = LF - fStep;
                }

                //Update
                preRMS = curRMS;
                iter++;
            }


            //-----------------------FLAG BUCKLED STATE----------------------------//
            if (BLF >= 1.0)
            {
                this.Message = "Works!";
            }
            else
            {
                this.Message = "Buckles!";
            }


            //-----------------------WARNING----------------------------//
            //If the maximum number of iterations has been reached
            if (iter == maxIterations)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Buckling did not occur within " + maxIterations + " load increments. Adjust load-step");
            }


            //-----------------------UPDATE VALUE LISTS----------------------------//
            //If opt is false then add results from last two iterations
            if (!opt)
            {
                for (int i = 0; i < currentPositions.Count; i++)
                {
                    vertexPositions.Add(previousPositions[i], new GH_Path(0));
                    vertexPositions.Add(currentPositions[i], new GH_Path(1));
                }

                for (int j = 0; j < currentGOutput.Count; j++)
                {
                    outputGoals.Add(previousGOutput[j], new GH_Path(0));
                    outputGoals.Add(currentGOutput[j], new GH_Path(1));
                }

            }


            //---------------------------------------------------------------OUTPUT-------------------------------------------------------------------------------//

            DA.SetData(0, BLF);
            DA.SetDataList(1, loadfactors);
            DA.SetDataList(2, displacementsRMS);
            DA.SetDataTree(3, vertexPositions);
            DA.SetDataTree(4, outputGoals);
        }


        //---------------------------------------------------------------METHODS-------------------------------------------------------------------------------//

        //Calculate displacement between load increment
        List<Vector3d> calcDisplacement(List<Point3d> currentPositions, List<Point3d> previousPositions)
        {
            List<Vector3d> displ = new List<Vector3d>();
            for (int i = 0; i < currentPositions.Count; i++)
            {
                Vector3d d = currentPositions[i] - previousPositions[i];
                displ.Add(d);
            }

            return displ;
        }


        //Calculate the RMS value of all vertex displacements in [m]
        double calcDisplacementsRMS(List<Vector3d> displacements)
        {
            double rms = 0.0;

            foreach (Vector3d v in displacements)
            {
                rms += Math.Pow(v.Length, 2);
            }

            rms /= displacements.Count;
            rms = Math.Round(Math.Sqrt(rms), 3);

            return rms;
        }


        //Does buckling occur
        bool isBuckled(double curRMS, double preRMS, int iter, double fStart, double fStep, double angleCriteria)
        {
            bool isBuckled = false;

            Vector3d vertical = new Vector3d(0, 1, 0);
            Vector3d tangent = new Vector3d();

            if (iter == 0)
            {
                tangent = new Vector3d(fStart, curRMS - preRMS, 0);
            }
            else
            {
                tangent = new Vector3d(fStep, curRMS - preRMS, 0);
            }
            tangent.Unitize();
            double angle = Vector3d.VectorAngle(tangent, vertical);

            //test against angle criteria
            if (angle < angleCriteria)
            {
                isBuckled = true;

                if (iter == 0)
                {
                    this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Buckling occured during the first load step. Decrease FStart");
                    this.Message = "Buckles!";
                }
                else
                {
                    this.AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "Buckling detected from angle criteria: " + Math.Round(angle * 180.0 / Math.PI, 1));
                }
            }

            return isBuckled;
        }


        //Displacement criteria
        bool isDeflectionTooBig(List<Vector3d> nodalDisplacements, double dMax)
        {
            bool isDeflectionTooBig = false;

            for (int i = 0; i < nodalDisplacements.Count; i++)
            {
                //If the deflection in a vertex is more than 2.0 meters then it is assumed that buckling must have occured before that point
                if (nodalDisplacements[i].Length > dMax)
                {
                    isDeflectionTooBig = true;
                    this.AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "Buckling detected from maximum displacement criteria");
                    break;
                }
            }

            return isDeflectionTooBig;
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
                return Properties.Resources.Buckling;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("{307319c5-1cec-464d-9501-ed7825cd0c2d}"); }
        }
    }
}