using System;
using System.Collections.Generic;
using Plankton;
using KangarooSolver;
using Grasshopper.Kernel;
using Rhino.Geometry;
using Grasshopper;
using Grasshopper.Kernel.Data;

namespace K2Structural
{
    public class SoapFilm : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the SoapFilm class.
        /// </summary>
        public SoapFilm()
          : base("SoapFilm", "SoapFilm",
              "A K2 anisotropic soap film goal",
              "K2Structural", "Elements")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("PMeshWarp", "pMeshW", "The plankton mesh with the first edge in each triangle following the warp direction", GH_ParamAccess.item);
            pManager.AddNumberParameter("WarpStress", "warpStress", "The warp stress", GH_ParamAccess.item);
            pManager.AddNumberParameter("WeftStress", "weftStress", "The weft stress", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("SoapFilmGoals", "SF", "The soap film goals", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //INPUT
            PlanktonMesh pMeshW = new PlanktonMesh();
            DA.GetData(0, ref pMeshW);

            double warpStress = 0.0;
            DA.GetData(1, ref warpStress);

            double weftStress = 0.0;
            DA.GetData(2, ref weftStress);


            //CALCULATE
            List<IGoal> soapFilmGoals = createSoapFilmGoals(pMeshW, warpStress, weftStress);


            //OUTPUT
            DA.SetDataList(0, soapFilmGoals);
        }


        //METHODS

        //Create a list of soap-film goals
        List<IGoal> createSoapFilmGoals(PlanktonMesh wPMesh, double warpStress, double weftStress)
        {
            List<IGoal> soapFilmGoals = new List<IGoal>();

            for (int i = 0; i < wPMesh.Faces.Count; i++)
            {
                int[] faceVertices = wPMesh.Faces.GetFaceVertices(i);
                Point3d P0 = new Point3d(wPMesh.Vertices[faceVertices[0]].X, wPMesh.Vertices[faceVertices[0]].Y, wPMesh.Vertices[faceVertices[0]].Z);
                Point3d P1 = new Point3d(wPMesh.Vertices[faceVertices[1]].X, wPMesh.Vertices[faceVertices[1]].Y, wPMesh.Vertices[faceVertices[1]].Z);
                Point3d P2 = new Point3d(wPMesh.Vertices[faceVertices[2]].X, wPMesh.Vertices[faceVertices[2]].Y, wPMesh.Vertices[faceVertices[2]].Z);

                Point3d[] faceVerticesXYZ = new Point3d[3] { P0, P1, P2 };

                GoalObject sf = new SoapFilmElement(faceVerticesXYZ, warpStress, weftStress);
                soapFilmGoals.Add(sf);
            }

            return soapFilmGoals;
        }


        //K2 Soap film goal
        public class SoapFilmElement : GoalObject
        {
            double sWarp;
            double sWeft;
            double[] angles;            //out of curiosity

            //NB! the array of points have to be sorted ccw in a face and with index 0 and 1 belonging to the edge in the warp direction
            public SoapFilmElement(Point3d[] pts, double warpStress, double weftStress)
            {
                sWarp = warpStress;
                sWeft = weftStress;
                angles = new double[3] { 0.0, 0.0, 0.0 };

                PPos = pts;
                Move = new Vector3d[3];
                Weighting = new double[3] { 1.0, 1.0, 1.0 };        // Set equal and constant as the move vectors are already scaled relative to each other
            }

            public override void Calculate(List<KangarooSolver.Particle> p)
            {
                Point3d PA = p[PIndex[0]].Position;
                Point3d PB = p[PIndex[1]].Position;
                Point3d PC = p[PIndex[2]].Position;

                //edge vectors
                Vector3d AB = PB - PA;          //follows the warp direction
                Vector3d BC = PC - PB;
                Vector3d CA = PA - PC;

                //normal vector
                Vector3d n = Vector3d.CrossProduct(AB, BC);
                n.Unitize();

                //height from edge AB
                Vector3d weftDir = Vector3d.CrossProduct(n, AB);
                weftDir.Unitize();
                double heightAB = Math.Abs(Vector3d.Multiply(BC, weftDir));

                //angle opposite to edge
                double angleAB = Vector3d.VectorAngle(-BC, CA);
                double angleBC = Vector3d.VectorAngle(-CA, AB);
                double angleCA = Vector3d.VectorAngle(-AB, BC);

                //magnitude of tension forces in the bars
                double tensionAB = 0.0;
                double tensionBC = 0.0;
                double tensionCA = 0.0;

                if (angleAB != Math.PI / 2.0)
                {
                    tensionAB = (heightAB / 2.0) * (sWarp - sWeft) + (sWeft * AB.Length) / (2.0 * Math.Tan(angleAB));
                }

                if (angleBC != Math.PI / 2.0)
                {
                    tensionBC = (sWeft * BC.Length) / (2.0 * Math.Tan(angleBC));
                }

                if (angleCA != Math.PI / 2.0)
                {
                    tensionCA = (sWeft * CA.Length) / (2.0 * Math.Tan(angleCA));
                }

                //Halfen the tension force to be compatible with half extension in each end of the bar F = kx <=> F/2 = k (x/2)
                tensionAB *= 0.5;
                tensionBC *= 0.5;
                tensionCA *= 0.5;

                //sum of forces in each node
                AB.Unitize();
                BC.Unitize();
                CA.Unitize();
                Vector3d fA = AB * tensionAB + (-CA * tensionCA);
                Vector3d fB = BC * tensionBC + (-AB * tensionAB);
                Vector3d fC = CA * tensionCA + (-BC * tensionBC);

                //Set vector direction and magnitude
                Move[0] = fA;
                Move[1] = fB;
                Move[2] = fC;

                //angles
                angles[0] = Vector3d.VectorAngle(fA, BC);
                angles[1] = Vector3d.VectorAngle(fB, CA);
                angles[2] = Vector3d.VectorAngle(fC, AB);
            }


            public override object Output(List<KangarooSolver.Particle> p)
            {
                var Data = new object[3] { angles[0], angles[1], angles[2] };
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
                return null;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("{7a474a6c-512c-4197-9091-ccc769d00389}"); }
        }
    }
}