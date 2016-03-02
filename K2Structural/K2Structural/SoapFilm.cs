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
            pManager.AddGenericParameter("PMesh", "pMesh", "A plankton mesh that represents the soap film", GH_ParamAccess.item);
            pManager.AddCurveParameter("WarpPolylines", "warpPl", "Specify the warp polylines", GH_ParamAccess.list);
            pManager.AddNumberParameter("WarpStress", "warpStress", "The warp stress", GH_ParamAccess.item);
            pManager.AddNumberParameter("WeftStress", "weftStress", "The weft stress", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("SoapFilmGoals", "SF", "The soap film goals", GH_ParamAccess.list);
            pManager.AddGenericParameter("GeodesicGoals", "GS", "A goal which pulls the vertices in the warp direction towards geodesic lines without affecting the membrane shape", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //------------------------------------------------------------INPUT------------------------------------------------------------------------//

            PlanktonMesh pMesh = new PlanktonMesh();
            DA.GetData(0, ref pMesh);

            //Test that the mesh is triangulated
            for (int i = 0; i < pMesh.Faces.Count; i++)
            {
                int[] faceIndexes = pMesh.Faces.GetFaceVertices(i);
                if (faceIndexes.Length != 3)
                {
                    this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "The pMesh has to be triangulated");
                }
            }

            //Convert from curve to polyline
            List<Polyline> warpPl = new List<Polyline>();
            List<Curve> warpCrvs = new List<Curve>();
            DA.GetDataList(1, warpCrvs);

            foreach (Curve crv in warpCrvs)
            {
                Polyline pl;
                crv.TryGetPolyline(out pl);
                warpPl.Add(pl);
            }

            //Stresses
            double warpStress = 0.0;
            DA.GetData(2, ref warpStress);

            double weftStress = 0.0;
            DA.GetData(3, ref weftStress);


            //------------------------------------------------------------CALCULATE------------------------------------------------------------------------//

            //Create warp aligned pMesh
            List<int> warpIndexList = createWarpIndexListFromPolylines(pMesh, warpPl);
            PlanktonMesh pMeshW = createWarpPMesh(pMesh, warpIndexList);

            //Create K2 goals
            List<IGoal> soapFilmGoals = createSoapFilmGoals(pMeshW, warpStress, weftStress);
            List<IGoal> geodesicGoals = createGStringGoals(pMeshW, warpStress * 100.0, warpIndexList);


            //------------------------------------------------------------OUTPUT------------------------------------------------------------------------//

            DA.SetDataList(0, soapFilmGoals);
            DA.SetDataList(1, geodesicGoals);
        }


        //------------------------------------------------------------K2 LIST OF GOALS------------------------------------------------------------------------//

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

                GoalObject sf = new SoapFilmGoal(faceVerticesXYZ, warpStress, weftStress);
                soapFilmGoals.Add(sf);
            }

            return soapFilmGoals;
        }

        //Create a list of geodesic goals
        List<IGoal> createGStringGoals(PlanktonMesh wPMesh, double gStrength, List<int> warpIndexList)
        {
            List<IGoal> gStringGoals = new List<IGoal>();

            //run through all internal vertices
            for (int i = 0; i < wPMesh.Vertices.Count; i++)
            {
                if (!wPMesh.Vertices.IsBoundary(i))
                {
                    Point3d cPt = new Point3d(wPMesh.Vertices[i].X, wPMesh.Vertices[i].Y, wPMesh.Vertices[i].Z);
                    Point3d[] ptNeighbours = extractVertexNeighboursXYZ(wPMesh, i);
                    int[] wIndexes = findWarpIndexes(wPMesh, i, warpIndexList);

                    GoalObject gs = new GeodesicGoal(cPt, ptNeighbours, wIndexes, gStrength);
                    gStringGoals.Add(gs);
                }
            }

            return gStringGoals;
        }


        //------------------------------------------------------------K2 SOAP FILM GOAL------------------------------------------------------------------------//

        //K2 Soap film goal
        public class SoapFilmGoal : GoalObject
        {
            double sWarp;
            double sWeft;
            double[] angles;            //out of curiosity

            //NB! the array of points have to be sorted ccw in a face and with index 0 and 1 belonging to the edge in the warp direction
            public SoapFilmGoal(Point3d[] pts, double warpStress, double weftStress)
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

                //make sure to handle the case where tan(90) = infinity. Assumption of non-degenerate triangles and thus tan(0)=0 or tan(180)=0 will never happen
                int dec = 1;
                if (Math.Round(angleAB, dec) != Math.Round(Math.PI / 2.0, dec))
                {
                    tensionAB = (heightAB / 2.0) * (sWarp - sWeft) + (sWeft * AB.Length) / (2.0 * Math.Tan(angleAB));
                }

                if (Math.Round(angleBC, dec) != Math.Round(Math.PI / 2.0, dec))
                {
                    tensionBC = (sWeft * BC.Length) / (2.0 * Math.Tan(angleBC));
                }

                if (Math.Round(angleCA, dec) != Math.Round(Math.PI / 2.0, dec))
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
                Vector3d fA = AB * Math.Abs(tensionAB) + (-CA * Math.Abs(tensionCA));           //make sure that the force is always a tension force even if the angle > PI/2
                Vector3d fB = BC * Math.Abs(tensionBC) + (-AB * Math.Abs(tensionAB));
                Vector3d fC = CA * Math.Abs(tensionCA) + (-BC * Math.Abs(tensionBC));

                //Set vector direction and magnitude
                Move[0] = fA;
                Move[1] = fB;
                Move[2] = fC;

                //angles
                /*
                angles[0] = Vector3d.VectorAngle(fA, BC);
                angles[1] = Vector3d.VectorAngle(fB, CA);
                angles[2] = Vector3d.VectorAngle(fC, AB);
                */
                angles[0] = angleAB;
                angles[1] = angleBC;
                angles[2] = angleCA;
            }


            public override object Output(List<KangarooSolver.Particle> p)
            {
                var Data = new object[3] { angles[0], angles[1], angles[2] };
                return Data;
            }

        }


        //------------------------------------------------------------K2 GEODESIC GOAL------------------------------------------------------------------------//

        //Geodesic goal
        public class GeodesicGoal : GoalObject
        {
            double gStrength;
            int[] wIndexes;
            int vertexValence;


            //pNeighbours are sorted ccw around the center vertex. Warp indexes specify which two neighbour points are in the warp direction
            public GeodesicGoal(Point3d pCenter, Point3d[] pNeighbours, int[] warpIndexes, double strength)
            {
                gStrength = strength;
                wIndexes = warpIndexes;
                vertexValence = pNeighbours.Length;

                //One array with all points (first index is the center vertex)
                Point3d[] positions = new Point3d[pNeighbours.Length + 1];
                positions[0] = pCenter;
                for (int i = 0; i < pNeighbours.Length; i++)
                {
                    positions[i + 1] = pNeighbours[i];
                }

                //Kangaroo2 properties
                PPos = positions;
                Move = new Vector3d[pNeighbours.Length + 1];
                Weighting = new double[pNeighbours.Length + 1];        //Set weighting to zero for all the neighbour vertices

                for (int j = 0; j < pNeighbours.Length + 1; j++)
                {
                    if (j == 0)
                    {
                        Weighting[j] = 1.0;
                    }
                    else
                    {
                        Weighting[j] = 0.0;
                    }
                }
            }


            public override void Calculate(List<KangarooSolver.Particle> p)
            {
                //Points
                Point3d ptC = p[PIndex[0]].Position;
                List<Point3d> ptNeighbours = new List<Point3d>();

                for (int i = 0; i < vertexValence; i++)
                {
                    ptNeighbours.Add(p[PIndex[i + 1]].Position);
                }

                //Edge vectors
                List<Vector3d> edgeVectors = new List<Vector3d>();
                foreach (Point3d pt in ptNeighbours)
                {
                    edgeVectors.Add(pt - ptC);
                }

                List<Vector3d> edgeVectorsShift = new List<Vector3d>();
                for (int j = 0; j < edgeVectors.Count; j++)
                {
                    int index = (j + 1) % edgeVectors.Count;
                    edgeVectorsShift.Add(edgeVectors[index]);
                }

                //Face normals
                List<Vector3d> faceNormals = new List<Vector3d>();
                for (int k = 0; k < edgeVectors.Count; k++)
                {
                    Vector3d fN = 0.5 * Vector3d.CrossProduct(edgeVectors[k], edgeVectorsShift[k]);         //scaled according to the area of the triangle
                    faceNormals.Add(fN);
                }

                //Vertex normal
                Vector3d vN = new Vector3d(0, 0, 0);
                foreach (Vector3d v in faceNormals)
                {
                    vN += v;
                }
                vN.Unitize();

                //Resultant tension force from center vertex
                Vector3d vForce = new Vector3d(0, 0, 0);
                foreach (int i in wIndexes)
                {
                    Vector3d gString = edgeVectors[i];
                    gString.Unitize();
                    gString *= gStrength;
                    Vector3d gcompN = Vector3d.Multiply(gString, vN) * vN;
                    Vector3d gcompT = gString - gcompN;
                    vForce += gcompT;
                }

                //Set move vectors
                Move[0] = vForce;
                for (int i = 0; i < vertexValence; i++)
                {
                    Move[i + 1] = new Vector3d(0, 0, 0);
                }
            }

        }


        //------------------------------------------------------------WARP ALIGNED PMESH------------------------------------------------------------------------//

        //Create new plankton mesh from sorted face topology according to warp direction
        PlanktonMesh createWarpPMesh(PlanktonMesh pMesh, List<int> warpIndexList)
        {
            PlanktonMesh pMeshW = new PlanktonMesh();

            //Add vertices from original pMesh
            for (int i = 0; i < pMesh.Vertices.Count; i++)
            {
                pMeshW.Vertices.Add(pMesh.Vertices[i].X, pMesh.Vertices[i].Y, pMesh.Vertices[i].Z);
            }

            //Add faces with new topology
            for (int i = 0; i < pMesh.Faces.Count; i++)
            {
                int[] faceVerticesSorted = calcFaceVerticesOrder(pMesh, i, warpIndexList);
                pMeshW.Faces.AddFace(faceVerticesSorted[0], faceVerticesSorted[1], faceVerticesSorted[2]);
            }
            return pMeshW;
        }


        //Sort face vertices according to warp direction
        int[] calcFaceVerticesOrder(PlanktonMesh pMesh, int faceIndex, List<int> warpIndexList)
        {
            int[] faceVerticesSorted = new int[3];

            int[] faceHalfedges = pMesh.Faces.GetHalfedges(faceIndex);
            int halfedgeWarp = -1;
            for (int i = 0; i < faceHalfedges.Length; i++)
            {
                int start = pMesh.Halfedges[faceHalfedges[i]].StartVertex;
                int end = pMesh.Halfedges[pMesh.Halfedges[faceHalfedges[i]].NextHalfedge].StartVertex;

                int wIndexStart = warpIndexList.IndexOf(start);
                if (wIndexStart == 0)
                {
                    if (end == warpIndexList[wIndexStart + 1])
                    {
                        halfedgeWarp = faceHalfedges[i];
                    }
                }
                else
                {
                    if (end == warpIndexList[wIndexStart + 1] || end == warpIndexList[wIndexStart - 1])
                    {
                        halfedgeWarp = faceHalfedges[i];
                    }
                }
            }

            if (halfedgeWarp == -1)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Face " + faceIndex + " does not have a warp direction specified");
            }
            else
            {
                int index0 = pMesh.Halfedges[halfedgeWarp].StartVertex;
                int index1 = pMesh.Halfedges[pMesh.Halfedges[halfedgeWarp].NextHalfedge].StartVertex;
                int index2 = pMesh.Halfedges[pMesh.Halfedges[halfedgeWarp].PrevHalfedge].StartVertex;

                faceVerticesSorted[0] = index0;
                faceVerticesSorted[1] = index1;
                faceVerticesSorted[2] = index2;
            }

            return faceVerticesSorted;
        }


        //Create one list of warp vertex indexes
        List<int> createWarpIndexListFromPolylines(PlanktonMesh pMesh, List<Polyline> pl)
        {
            List<int> warpIndexes = new List<int>();

            foreach (Polyline p in pl)
            {
                List<int> polyIndexes = convertPolyToMeshIndexes(pMesh, p);
                foreach (int i in polyIndexes)
                {
                    warpIndexes.Add(i);
                }
                warpIndexes.Add(-1);            //to seperate between polylines
            }
            return warpIndexes;
        }


        //Convert polyline points to mesh indexes
        List<int> convertPolyToMeshIndexes(PlanktonMesh pMesh, Polyline pl)
        {
            List<int> plIndexes = new List<int>();

            List<Point3d> verticesXYZ = extractMeshVerticesXYZ(pMesh);
            List<Point3d> polylinePts = new List<Point3d>();
            for (int i = 0; i < pl.Count; i++)
            {
                polylinePts.Add(pl[i]);
            }

            foreach (Point3d pt in polylinePts)
            {
                int index = verticesXYZ.IndexOf(pt);

                if (index == -1)
                {
                    this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Was not able to find point P(" + pt.X + "," + pt.Y + "," + pt.Z + ") amongst the vertices in the mesh");
                }
                else
                {
                    plIndexes.Add(index);
                }
            }

            return plIndexes;
        }


        //Convert mesh vertices to verticesXYZ
        List<Point3d> extractMeshVerticesXYZ(PlanktonMesh pMesh)
        {
            List<Point3d> verticesXYZ = new List<Point3d>();

            for (int i = 0; i < pMesh.Vertices.Count; i++)
            {
                Point3d pt = new Point3d(pMesh.Vertices[i].X, pMesh.Vertices[i].Y, pMesh.Vertices[i].Z);
                verticesXYZ.Add(pt);
            }
            return verticesXYZ;
        }


        //------------------------------------------------------------GEODESIC WARP INDEXES------------------------------------------------------------------------//

        //Extract vertex neighbours
        Point3d[] extractVertexNeighboursXYZ(PlanktonMesh pMesh, int vIndex)
        {
            Point3d[] vNeighboursXYZ = new Point3d[pMesh.Vertices.GetValence(vIndex)];

            //Neighbours mesh info
            int[] vNeighbours = pMesh.Vertices.GetVertexNeighbours(vIndex);

            for (int i = 0; i < vNeighbours.Length; i++)
            {
                vNeighboursXYZ[i] = new Point3d(pMesh.Vertices[vNeighbours[i]].X, pMesh.Vertices[vNeighbours[i]].Y, pMesh.Vertices[vNeighbours[i]].Z);
            }
            return vNeighboursXYZ;
        }

        //Find the two indexes in the warpIndexList which correspond to the two neighbour vertices in the warp direction
        int[] findWarpIndexes(PlanktonMesh pMesh, int vIndex, List<int> warpIndexList)
        {
            int[] wIndexes = new int[2];

            //find center vertex in warp index list (every vertex has a warp direction)
            int cIndexW = warpIndexList.IndexOf(vIndex);

            //extract index+1 and index-1 (always works because only internal vertices are considered)
            int v0IndexW = warpIndexList[cIndexW - 1];
            int v1IndexW = warpIndexList[cIndexW + 1];

            //find index of these two in vNeighbours
            int[] vNeighbours = pMesh.Vertices.GetVertexNeighbours(vIndex);
            int v0IndexN = Array.IndexOf(vNeighbours, v0IndexW);
            int v1IndexN = Array.IndexOf(vNeighbours, v1IndexW);

            if (v0IndexN != -1 && v1IndexN != -1)
            {
                wIndexes[0] = v0IndexN;
                wIndexes[1] = v1IndexN;
            }
            else
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Not able to find warp direction within the neighbourhood of vertex " + vIndex);
            }

            return wIndexes;
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