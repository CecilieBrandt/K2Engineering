using System;
using System.Collections.Generic;
using System.Linq;
using Plankton;
using Grasshopper;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;

namespace K2Engineering
{
    public class PMeshExt: PlanktonMesh
    {

        public PMeshExt(PlanktonMesh source): base(source)
        {

        }



        //----------------------------------------- EXTRACT FACE EDGES AS CCW LINES-------------------------------------------//
        public Line[] extractFaceEdges(int faceIndex)
        {
            int[] faceHalfedges = this.Faces.GetHalfedges(faceIndex);
            Line[] edgesCCW = new Line[faceHalfedges.Length];

            for (int i = 0; i < faceHalfedges.Length; i++)
            {
                int startVertex = this.Halfedges[faceHalfedges[i]].StartVertex;
                Point3d start = new Point3d(this.Vertices[startVertex].X, this.Vertices[startVertex].Y, this.Vertices[startVertex].Z);
                int nextHalfedge = this.Halfedges[faceHalfedges[i]].NextHalfedge;
                int endVertex = this.Halfedges[nextHalfedge].StartVertex;
                Point3d end = new Point3d(this.Vertices[endVertex].X, this.Vertices[endVertex].Y, this.Vertices[endVertex].Z);

                edgesCCW[i] = new Line(start, end);
            }

            return edgesCCW;
        }



        //----------------------------------------- CALCULATE FACE NORMAL-------------------------------------------//
        //Note: calculate face normal (not normalised) as average of cross products of edge pairs (n-gon) 
        //If the face is a triangle, the magnitude of the face normal equals the area

        public Vector3d calcFaceNormal(int faceIndex)
        {
            Line[] edgesCCW = extractFaceEdges(faceIndex);

            //Shift edges
            Line[] edgesCCW_shift = new Line[edgesCCW.Length];
            for (int j = 0; j < edgesCCW.Length; j++)
            {
                edgesCCW_shift[j] = edgesCCW[(j + 1) % edgesCCW.Length];
            }

            //Calculate face normal vector from cross product of edge vectors
            Vector3d normal = new Vector3d(0, 0, 0);
            for (int k = 0; k < edgesCCW.Length; k++)
            {
                normal += (Vector3d.CrossProduct(new Vector3d(edgesCCW[k].To - edgesCCW[k].From), new Vector3d(edgesCCW_shift[k].To - edgesCCW_shift[k].From)) / 2.0);
            }
            normal = normal / edgesCCW.Length;

            return normal;
        }



        //----------------------------------------- CALCULATE VERTEX NORMAL-------------------------------------------//
        //Note: calculate vertex normal as weighted average of the adjacent face normals (not normalised)

        public Vector3d calcVertexNormal(int vertexIndex)
        {
            int[] adjFaces = this.Vertices.GetVertexFaces(vertexIndex);

            Vector3d vNormal = new Vector3d(0, 0, 0);
            foreach (int faceIndex in adjFaces)
            {
                if (faceIndex != -1)
                {
                    vNormal += calcFaceNormal(faceIndex);
                }
            }
            vNormal.Unitize();

            return vNormal;
        }



        //----------------------------------------- CALCULATE VERTEX NORMALS (ALL)-------------------------------------------//
        public Vector3d[] calcVertexNormals()
        {
            Vector3d[] vertexNormals = new Vector3d[this.Vertices.Count];

            for (int i = 0; i < this.Vertices.Count; i++)
            {
                vertexNormals[i] = calcVertexNormal(i);
            }

            return vertexNormals;
        }



        //-----------------------------------------DETERMINE IF A FACE HAS OBTUSE/RIGHT ANGLES-------------------------------------------//
        //Note: outputs vertex index of obtuse angle. -1 if non-obtuse
        public bool isFaceObtuse(int faceIndex, out int vertexIndex)
        {
            bool isObtuse = false;
            vertexIndex = -1;

            int[] faceHalfedges = this.Faces.GetHalfedges(faceIndex);
            Line[] edgesCCW = extractFaceEdges(faceIndex);

            //Shift edges
            Line[] edgesCCW_shift = new Line[edgesCCW.Length];
            for (int j = 0; j < edgesCCW.Length; j++)
            {
                edgesCCW_shift[j] = edgesCCW[(j + 1) % edgesCCW.Length];
            }

            for (int i = 0; i < edgesCCW.Length; i++)
            {
                Vector3d edge0 = new Vector3d(edgesCCW[i].From - edgesCCW[i].To);       //reverse direction
                Vector3d edge1 = new Vector3d(edgesCCW_shift[i].To - edgesCCW_shift[i].From);
                edge0.Unitize();
                edge1.Unitize();

                double dotProduct = Vector3d.Multiply(edge1, edge0);

                if (dotProduct <= 0.0)       //Obtuse or right angle
                {
                    isObtuse = true;
                    vertexIndex = this.Halfedges[this.Halfedges[faceHalfedges[i]].NextHalfedge].StartVertex;
                }
            }

            return isObtuse;
        }



        //-----------------------------------------CALCULATE THE ASSOCIATED VERTEX AREA FOR AN OBTUSE FACE-------------------------------------------//
        public double calcObtuseVertexArea(bool isVertexObtuse, Vector3d faceNormal, Plane plnProj)
        {
            double faceArea = faceNormal.Length;
            double areaFactor = 1.0;

            if (plnProj.IsValid)
            {
                faceNormal.Unitize();
                areaFactor = Math.Abs(Vector3d.Multiply(faceNormal, plnProj.Normal));
            }

            double vertexArea = faceArea / 4.0;
            if (isVertexObtuse)
            {
                vertexArea = faceArea / 2.0;
            }

            vertexArea *= areaFactor;

            return vertexArea;
        }



        //-----------------------------------------EXTRACT NECESSARY DATA FOR A NON-OBTUSE FACE-------------------------------------------//
        public void extractNonObtuseData(int vertexIndex, int faceIndex, out Line PR, out Line PQ, out double angleR, out double angleQ)
        {
            Line[] faceEdges = extractFaceEdges(faceIndex);
            int[] faceHalfedges = this.Faces.GetHalfedges(faceIndex);

            int halfedgeOutIndex = -1;
            for(int i=0; i<faceHalfedges.Length; i++)
            {
                int startVertex = this.Halfedges[faceHalfedges[i]].StartVertex;

                if(startVertex == vertexIndex)
                {
                    halfedgeOutIndex = i;
                    break;
                }
            }

            Line[] faceEdges_shift = new Line[faceEdges.Length];
            for(int j=0; j< faceEdges.Length; j++)
            {
                faceEdges_shift[j] = faceEdges[(j+halfedgeOutIndex) % faceHalfedges.Length];
            }

            PQ = faceEdges_shift[0];
            PR = faceEdges_shift[2];
            angleQ = Vector3d.VectorAngle(new Vector3d(faceEdges_shift[0].From - faceEdges_shift[0].To), new Vector3d(faceEdges_shift[1].To - faceEdges_shift[1].From));
            angleR = Vector3d.VectorAngle(new Vector3d(faceEdges_shift[1].From - faceEdges_shift[1].To), new Vector3d(faceEdges_shift[2].To - faceEdges_shift[2].From));
        }



        //-----------------------------------------CALCULATE THE ASSOCIATED VERTEX AREA FOR A NON-OBTUSE FACE-------------------------------------------//
        public double calcNonObtuseVertexArea(int vertexIndex, int faceIndex, Vector3d faceNormal, Plane plnProj)
        {
            Line PR;
            Line PQ;
            double angleR;
            double angleQ;
            extractNonObtuseData(vertexIndex, faceIndex, out PR, out PQ, out angleR, out angleQ);

            double vertexArea = (1 / 8.0) * ((Math.Pow(PR.Length, 2) * (1 / Math.Tan(angleQ))) + (Math.Pow(PQ.Length, 2) * (1 / Math.Tan(angleR))));
            double areaFactor = 1.0;

            if (plnProj.IsValid)
            {
                faceNormal.Unitize();
                areaFactor = Math.Abs(Vector3d.Multiply(faceNormal, plnProj.Normal));
            }

            vertexArea *= areaFactor;

            return vertexArea;
        }



        //-----------------------------------------CALCULATE THE VERTEX VORONOI AREA-------------------------------------------//
        public Vector3d calcVertexVoronoiArea(int vertexIndex, Plane plnProj)
        {
            int[] vertexFaces = this.Vertices.GetVertexFaces(vertexIndex);

            Vector3d[] faceNormals = new Vector3d[vertexFaces.Length];
            for (int i = 0; i < vertexFaces.Length; i++)
            {
                if(vertexFaces[i] != -1)
                {
                    faceNormals[i] = calcFaceNormal(vertexFaces[i]);
                }
                else
                {
                    faceNormals[i] = new Vector3d(0, 0, 0);
                }
            }

            double vertexVoronoiArea = 0.0;

            for (int j= 0; j<vertexFaces.Length; j++)
            {
                if(vertexFaces[j] != -1)
                {
                    int obtuseVertexIndex;
                    bool isObtuse = isFaceObtuse(vertexFaces[j], out obtuseVertexIndex);

                    if (isObtuse)
                    {
                        bool isVertexObtuse = false;
                        if (vertexIndex == obtuseVertexIndex)
                        {
                            isVertexObtuse = true;
                        }

                        vertexVoronoiArea += calcObtuseVertexArea(isVertexObtuse, faceNormals[j], plnProj);
                    }
                    else
                    {
                        vertexVoronoiArea += calcNonObtuseVertexArea(vertexIndex, vertexFaces[j], faceNormals[j], plnProj);
                    }
                } 
            }

            Vector3d vertexNormal = calcVertexNormal(vertexIndex);
            Vector3d vertexNormalArea = Vector3d.Multiply(vertexVoronoiArea, vertexNormal);

            return vertexNormalArea;
        }



        //-----------------------------------------CALCULATE THE VERTEX VORONOI AREAS (ALL)-------------------------------------------//
        public Vector3d[] calcVertexVoronoiAreas(Plane plnProj)
        {
            Vector3d[] vertexAreas = new Vector3d[this.Vertices.Count];

            for(int i=0; i<this.Vertices.Count; i++)
            {
                vertexAreas[i] = calcVertexVoronoiArea(i, plnProj);
            }

            return vertexAreas;
        }




        //-----------------------------------------TEST IF THE MESH IS TRIANGULATED-------------------------------------------//
        public bool isMeshTriangulated()
        {
            bool isTriangulated = false;

            int count = 0;
            for(int i=0; i<this.Faces.Count; i++)
            {
                int[] faceVertices = this.Faces.GetFaceVertices(i);
                if(faceVertices.Length == 3)
                {
                    count++;
                }
            }

            if(count == this.Faces.Count)
            {
                isTriangulated = true;
            }

            return isTriangulated;
        }

        

        //-----------------------------------------CREATE LIST OF VERTICES AS POINT3D-------------------------------------------//
        public Point3d[] convertVerticesToXYZ()
        {
            Point3d[] verticesXYZ = new Point3d[this.Vertices.Count];

            for (int i = 0; i < this.Vertices.Count; i++)
            {
                verticesXYZ[i] = new Point3d(this.Vertices[i].X, this.Vertices[i].Y, this.Vertices[i].Z);
            }

            return verticesXYZ;
        }



        //-----------------------------------------CREATE PMESH PROJECTED TO XY PLANE-------------------------------------------//
        /*
        public PMeshExt projectMeshToXY()
        {
            PMeshExt pMeshE = new PMeshExt(this);

            for(int i=0; i< pMeshE.Vertices.Count; i++)
            {
                pMeshE.Vertices[i].Z = 0.0f;
            }

            return pMeshE;
        }
        */


    }
}
