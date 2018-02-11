using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Rhino.Geometry;
using Plankton;
using KangarooSolver;

namespace K2Engineering
{
    public class Pressure : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the Pressure class.
        /// </summary>
        public Pressure()
          : base("Pressure", "Pressure",
              "A pressure load goal following the principle of the Ideal Gas Law and with forces perpendicular to the mesh faces",
              "K2Eng", "2 Load")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("PMesh", "pMesh", "A plankton mesh in [m]. The mesh has to be triangulated", GH_ParamAccess.item);
            pManager.AddNumberParameter("Pressure", "p", "The pressure in [kN/m2]", GH_ParamAccess.item);
            pManager.AddBooleanParameter("PressureOption", "opt", "If true, the pressure is constant. If false, the amount of molecules is constant but the pressure varies according to the change in volume", GH_ParamAccess.item, true);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("PressureGoal", "P", "The pressure goal", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //Input
            PlanktonMesh pMesh = new PlanktonMesh();
            DA.GetData(0, ref pMesh);

            double pressure = 0.0;
            DA.GetData(1, ref pressure);
            pressure *= 1e3;   //From [kN/m2] to [N/m2]

            bool constantP = true;
            DA.GetData(2, ref constantP);


            //Calculate
            GoalObject pressureGoal = new PressureGoal(pMesh, pressure, constantP);


            //Output
            DA.SetData(0, pressureGoal);

        }


        public class PressureGoal : GoalObject
        {
            //Global properties
            PlanktonMesh pMesh;

            double pres0;
            double pres1;

            double vol0;
            double vol1;

            double mol0;
            double mol1;

            double gasConst;
            double temp;

            bool opt;

            public PressureGoal(PlanktonMesh mesh, double p, bool option)
            {
                pMesh = (PlanktonMesh) mesh;

                //Pressure
                pres0 = p;      // Unit: [N/m2] / [Pa]
                pres1 = pres0;

                //Calculate starting volume
                vol0 = 0.0;         // Unit: [m3]

                for (int j = 0; j < pMesh.Faces.Count; j++)
                {
                    int[] faceVertices = pMesh.Faces.GetFaceVertices(j);

                    Point3d pA = new Point3d(pMesh.Vertices[faceVertices[0]].X, pMesh.Vertices[faceVertices[0]].Y, pMesh.Vertices[faceVertices[0]].Z);
                    Point3d pB = new Point3d(pMesh.Vertices[faceVertices[1]].X, pMesh.Vertices[faceVertices[1]].Y, pMesh.Vertices[faceVertices[1]].Z);
                    Point3d pC = new Point3d(pMesh.Vertices[faceVertices[2]].X, pMesh.Vertices[faceVertices[2]].Y, pMesh.Vertices[faceVertices[2]].Z);

                    Point3d pD = new Point3d(0, 0, 0);

                    Vector3d AB = pB - pA;
                    Vector3d AC = pC - pA;
                    Vector3d AD = pD - pA;

                    Vector3d cross = Vector3d.CrossProduct(AB, -AC);
                    double dot = Vector3d.Multiply(cross, AD);

                    double v = (1.0 / 6.0) * dot;

                    vol0 += v;
                }

                vol1 = vol0;

                //Gas constant and temperature
                gasConst = 8.314472;            // Unit: [J/(K*mol)]
                temp = 273 + 20;                // Unit: [Kelvins] Set to 20 degrees by default

                //Molecules
                mol0 = (pres0 * vol0) / (gasConst * temp);      // Unit: [moles]
                mol1 = mol0;

                opt = option;


                PPos = new Point3d[pMesh.Vertices.Count];
                Move = new Vector3d[pMesh.Vertices.Count];
                Weighting = new double[pMesh.Vertices.Count];

                for (int i = 0; i < pMesh.Vertices.Count; i++)
                {
                    PPos[i] = new Point3d(pMesh.Vertices[i].X, pMesh.Vertices[i].Y, pMesh.Vertices[i].Z);
                    Move[i] = new Vector3d(0, 0, 0);
                    Weighting[i] = 1.0;
                }

            }


            public override void Calculate(List<KangarooSolver.Particle> p)
            {
                //Lists of vertex normals and areas
                Vector3d[] normals = new Vector3d[pMesh.Vertices.Count];
                double[] vertexAreas = new double[pMesh.Vertices.Count];

                for (int j = 0; j < pMesh.Vertices.Count; j++)
                {
                    normals[j] = new Vector3d(0, 0, 0);
                    vertexAreas[j] = 0.0;
                }

                vol1 = 0.0;


                //Run through all mesh faces to calculate vertex normals, areas and current volume
                for (int i = 0; i < pMesh.Faces.Count; i++)
                {

                    //Get face vertices
                    int[] faceVertices = pMesh.Faces.GetFaceVertices(i);

                    Point3d PA = p[PIndex[faceVertices[0]]].Position;
                    Point3d PB = p[PIndex[faceVertices[1]]].Position;
                    Point3d PC = p[PIndex[faceVertices[2]]].Position;

                    Vector3d AB = PB - PA;
                    Vector3d BC = PC - PB;
                    Vector3d CA = PA - PC;

                    Vector3d normal = Vector3d.CrossProduct(AB, BC);
                    double nodalArea = normal.Length / 6.0;
                    normal.Unitize();

                    normals[faceVertices[0]] += normal;
                    normals[faceVertices[1]] += normal;
                    normals[faceVertices[2]] += normal;

                    vertexAreas[faceVertices[0]] += nodalArea;
                    vertexAreas[faceVertices[1]] += nodalArea;
                    vertexAreas[faceVertices[2]] += nodalArea;


                    //Current volume
                    Point3d PD = new Point3d(0, 0, 0);
                    Vector3d AD = PD - PA;
                    Vector3d cross = Vector3d.CrossProduct(AB, CA);
                    double dot = Vector3d.Multiply(cross, AD);
                    vol1 += (1.0 / 6.0) * dot;
                }

                //constant molecules -> pressure adjusts
                if (opt == false)
                {
                    pres1 = (mol0 * gasConst * temp) / vol1;
                }
                //constant pressure -> molecules adjust
                else
                {
                    mol1 = (pres0 * vol1) / (gasConst * temp);
                }


                //Calculate pressure vector acting in each vertex in [N]
                for (int k = 0; k < pMesh.Vertices.Count; k++)
                {
                    Vector3d n = new Vector3d(normals[k]);
                    n.Unitize();
                    Move[k] = pres1 * vertexAreas[k] * n;
                }

            }


            public override object Output(List<KangarooSolver.Particle> p)
            {
                Point3d[] vertices = new Point3d[pMesh.Vertices.Count];
                Vector3d[] pForces = new Vector3d[pMesh.Vertices.Count];

                for (int i = 0; i < pMesh.Vertices.Count; i++)
                {
                    vertices[i] = p[PIndex[i]].Position;
                    pForces[i] = Move[i] * Weighting[i] * 1e-3;       // Unit: [kN]
                }

                //Create pressure data object to store output information
                DataTypes.PressureData presData = new DataTypes.PressureData(vertices, pForces, Math.Round(pres0*1e-3,3), Math.Round(pres1*1e-3,3), Math.Round(vol0,3), Math.Round(vol1,3), Convert.ToInt32(mol0), Convert.ToInt32(mol1));
                return presData;
            }

        }


        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.Pressure;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("dfe7573e-5d67-481b-8226-15fe9b07d3fd"); }
        }
    }
}