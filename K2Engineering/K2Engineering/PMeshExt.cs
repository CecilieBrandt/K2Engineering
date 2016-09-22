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



        //----------------------------------------- CALCULATE FACE NORMAL-------------------------------------------//
        //Note: calculate face normal (not normalised) as average of cross products of edge pairs (n-gon)

        public Vector3d calcFaceNormal(int faceIndex)
        {
            //Create face edge vectors
            int[] faceHalfedges = this.Faces.GetHalfedges(faceIndex);

            Vector3d[] edgesCCW = new Vector3d[faceHalfedges.Length];

            for (int i = 0; i < faceHalfedges.Length; i++)
            {
                int startVertex = this.Halfedges[faceHalfedges[i]].StartVertex;
                Point3d start = new Point3d(this.Vertices[startVertex].X, this.Vertices[startVertex].Y, this.Vertices[startVertex].Z);
                int nextHalfedge = this.Halfedges[faceHalfedges[i]].NextHalfedge;
                int endVertex = this.Halfedges[nextHalfedge].StartVertex;
                Point3d end = new Point3d(this.Vertices[endVertex].X, this.Vertices[endVertex].Y, this.Vertices[endVertex].Z);

                edgesCCW[i] = new Vector3d(end - start);
            }

            //Shift edge vectors
            Vector3d[] edgesCCW_shift = new Vector3d[edgesCCW.Length];
            for (int j = 0; j < edgesCCW.Length; j++)
            {
                edgesCCW_shift[j] = edgesCCW[(j + 1) % edgesCCW.Length];
            }

            //Calculate face normal vector from cross product of edge vectors
            Vector3d normal = new Vector3d(0, 0, 0);
            for (int k = 0; k < edgesCCW.Length; k++)
            {
                normal += (Vector3d.CrossProduct(edgesCCW[k], edgesCCW_shift[k]) / 2.0);
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















    }
}
