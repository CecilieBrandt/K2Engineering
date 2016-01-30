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
            : base("ElasticRodElement", "Rod",
                "A goal that represents an elastic rod element with bending stiffness only. It outputs the bending plane, the curvature radius (mm) and the bending stress (MPa)",
                "K2Struct", "Elements")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddLineParameter("LineA", "LnA", "Line (mm) representing the first segment", GH_ParamAccess.item);
            pManager.AddLineParameter("LineB", "LnB", "Line (mm) representing the consecutive segment", GH_ParamAccess.item);
            pManager.AddNumberParameter("E-Modulus", "E", "E-Modulus of the material in MPa", GH_ParamAccess.item);
            pManager.AddNumberParameter("Inertia", "I", "The moment of inertia in mm4", GH_ParamAccess.item);
            pManager.AddNumberParameter("z-distance", "z", "The distance from the section axis to the extreme fiber in mm", GH_ParamAccess.item);
            pManager.AddIntegerParameter("RestAngleOption", "Opt", "Specify the rest angle. 1: straight. 2: current angle", GH_ParamAccess.item, 1);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("R", "Rod", "Elastic rod element with stress output", GH_ParamAccess.item);
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

            int angleOpt = 1;
            DA.GetData(5, ref angleOpt);
            if (angleOpt < 1)
            {
                angleOpt = 1;
            }
            else if (angleOpt > 2)
            {
                angleOpt = 2;
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
            int restAngleOpt;
            
            double radiusInitial;


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
                Weighting = new double[4] { E*I, E*I, E*I, E*I }; // Weighting is an array of doubles for how strongly the goal affects each point 

                //Other
                lineA = LA;
                lineB = LB;
                eModulus = E;
                inertia = I;
                zDist = z;
                restAngleOpt = opt;

                //Rest position options
                radiusInitial = 0.0;        //straight
                
                if (opt == 2)               //restangle
                {
                    Point3d cc = calcCircumcenter(LA.From, LA.To, LB.To);
                    if (cc.CompareTo(LA.From) == 0)
                    {
                        radiusInitial = 0.0;
                    }
                    else
                    {
                        radiusInitial = cc.DistanceTo(LA.To);
                    }
                        
                }
            }

            //Calculate the center of the circle passing through the three points (circumcenter of triangle)
            //The first point is returned in case of colinear points (infinite radius)
            private Point3d calcCircumcenter(Point3d P0, Point3d P1, Point3d P2)
            {
                //Direction vectors
                Vector3d V10 = P0 - P1;
                Vector3d V12 = P2 - P1;
                Vector3d n = Vector3d.CrossProduct(V10, V12);
                Vector3d V10_perp = Vector3d.CrossProduct(n, V10);
                Vector3d V12_perp = Vector3d.CrossProduct(V12, n);
                V10_perp.Unitize();
                V12_perp.Unitize();
                Point3d V10_mid = P1 + V10 / 2;
                Point3d V12_mid = P1 + V12 / 2;

                Point3d CC = new Point3d();

                //Calculate line intersection of bisectors

                //Check that the bisectors are not parallel
                Vector3d bisectorCross = Vector3d.CrossProduct(V10_perp, V12_perp);

                //Check that the bisectors lie in the same plane
                Vector3d midBisectorCross = Vector3d.CrossProduct((V12_mid - V10_mid), V12_perp);
                int parallel = bisectorCross.IsParallelTo(midBisectorCross);

                if (Math.Round(bisectorCross.Length, 2) != 0.0 && parallel != 0)
                {
                    double paramA = midBisectorCross.Length / bisectorCross.Length;
                    if (parallel == -1)
                    {
                        paramA *= -1;
                    }

                    CC = V10_mid + paramA * V10_perp;
                }
                else
                {
                    CC = P0;            // set to the first point to indicate it is invalid i.e. the curvature is infinite
                }

                return CC;
            }


            public override void Calculate(List<KangarooSolver.Particle> p)
            {
                //Directions
                Point3d P0 = p[PIndex[0]].Position;             //get the current position of the particle at the start of the line
                Point3d P1 = p[PIndex[1]].Position;
                Point3d P2 = p[PIndex[2]].Position;
                Point3d P3 = p[PIndex[3]].Position;

                Vector3d V01 = P1 - P0;
                Vector3d V23 = P3 - P2;
                Vector3d V03 = P3 - P0;

                Vector3d n = Vector3d.CrossProduct(-V01, V23);
                Vector3d shearA = Vector3d.CrossProduct(-V01, n);
                Vector3d shearB = Vector3d.CrossProduct(V23, n);
                shearA.Unitize();
                shearB.Unitize();

                Point3d cc = calcCircumcenter(P0, P1, P3);
                bool colinear = false;
                if (cc.CompareTo(P0) == 0)
                {
                    colinear = true;
                }

                //Make sure shear direction is creating a moment in the right direction to straighten out
                if (cc.DistanceTo(P1 + shearA) > cc.DistanceTo(P1 - shearA))
                {
                    shearA.Reverse();
                }
                if (cc.DistanceTo(P1 + shearB) > cc.DistanceTo(P1 - shearB))
                {
                    shearB.Reverse();
                }

                //Calculate the magnitude of the shear force
                double radiusCurrent = cc.DistanceTo(P1);
                double shearAVal;
                double shearBVal;

                if (restAngleOpt == 1)              //aims to be a straight line
                {
                    if (colinear)
                    {
                        shearAVal = 0.0;
                        shearBVal = 0.0;
                    }
                    else
                    {
                        shearAVal = 1.0 / (radiusCurrent * V01.Length);
                        shearBVal = 1.0 / (radiusCurrent * V23.Length);
                    }
                }
                else                                 //aims to maintain initial angle
                {
                    if (colinear)
                    {
                        shearAVal = -1.0 / (radiusInitial * V01.Length);
                        shearBVal = -1.0 / (radiusInitial * V23.Length);
                        //shearAVal = 0.0;
                        //shearBVal = 0.0;
                    }
                    else
                    {
                        shearAVal = (1.0 / V01.Length) * ((1.0 / radiusCurrent) - (1.0 / radiusInitial));
                        shearBVal = (1.0 / V23.Length) * ((1.0 / radiusCurrent) - (1.0 / radiusInitial));
                    }

                }

                shearA *= shearAVal;
                shearB *= shearBVal;

                Move[0] = -shearA;
                Move[1] = shearA;
                Move[2] = shearB;
                Move[3] = -shearB;
            }

            


            /*
            //Stress at the point between two line segments
            public override object Output(List<KangarooSolver.Particle> p)
            {
                Point3d P0 = p[PIndex[0]].Position;             //get the current position of the particle at the start of the line
                Point3d P1 = p[PIndex[1]].Position;
                Point3d P2 = p[PIndex[2]].Position;
                Point3d P3 = p[PIndex[3]].Position;

                Point3d CC = calcCircumcenter(P0, P1, P3);

                //Vector3d vRadius;
                double radius;
                double moment;
                Plane plane;

                //check if CC equals P0. If yes, the three points are colinear > infinite curvature > zero moment
                if (CC.CompareTo(P0) == 0){
                    radius = double.NaN;
                    moment = 0.0;
                    plane = new Plane(P1, new Vector3d(0, 0, 0));
                } 
                else {
                    Vector3d vRadius = CC - P1;
                    radius = vRadius.Length;
                    moment = (eModulus * inertia) / radius;
                    plane = new Plane(P1, vRadius, Vector3d.CrossProduct(Vector3d.CrossProduct(P0 - P1, P3 - P2), vRadius) );
                }

                var Data = new List<Object>();
                Data.Add(plane);
                Data.Add(radius);
                Data.Add((moment*zDist)/inertia);
                    
                return Data;
            }
            */
            

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
