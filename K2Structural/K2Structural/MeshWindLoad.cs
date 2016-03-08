using System;
using System.Collections.Generic;
using Plankton;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace K2Structural
{
    public class MeshWindLoad : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MeshWindLoad class.
        /// </summary>
        public MeshWindLoad()
          : base("MeshWindLoad", "MWind",
              "Calculate a simplified wind-load on a mesh",
              "K2Structural", "2 Load")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("PlanktonMesh", "pMesh", "A plankton mesh in [mm]", GH_ParamAccess.item);
            pManager.AddNumberParameter("WindLoad", "Q", "The wind load in [kN/m2]", GH_ParamAccess.item, 1.0);
            pManager.AddVectorParameter("WindDirection", "dir", "The wind-direction", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("Points", "pts", "A list of points which the forces act on", GH_ParamAccess.list);
            pManager.AddVectorParameter("NodalWindLoad", "Fwind", "The nodal wind loads in [N]", GH_ParamAccess.list);
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

            double windload = 0.0;
            DA.GetData(1, ref windload);

            Vector3d dir = new Vector3d();
            DA.GetData(2, ref dir);
            dir.Unitize();


            //Calculate
            List<Point3d> verticesXYZ = extractVerticesXYZ(pMesh);
            List<Vector3d> nodalLoads = calcNodalWindLoads(pMesh, windload, dir);

            //Output
            DA.SetDataList(0, verticesXYZ);
            DA.SetDataList(1, nodalLoads);
        }


        //Methods

        //--------------------------------------------------------------------WIND LOAD---------------------------------------------------------//
        //Calculate nodal wind loads
        List<Vector3d> calcNodalWindLoads(PlanktonMesh pMesh, double windload, Vector3d dir)
        {
            List<Vector3d> nodalLoads = new List<Vector3d>();

            for (int i = 0; i < pMesh.Vertices.Count; i++)
            {
                Vector3d normal = calcVertexNormal(pMesh, i);

                //Calculate projection onto wind direction
                double proj = Vector3d.Multiply(normal, dir);
                normal *= proj;

                //Wind load
                double vertexArea = calcVertexVoronoiArea(pMesh, i);
                Vector3d qw = normal * vertexArea * windload * 1e-3;               // Magnitude in [N]

                nodalLoads.Add(qw);
            }

            return nodalLoads;
        }

        //--------------------------------------------------------------------VERTEX NORMALS---------------------------------------------------------//

        // Calculate the face normal (not normalised)
        Vector3d calcFaceNormal(PlanktonMesh pMesh, int f)
        {
            int[] faceVertices = pMesh.Faces.GetFaceVertices(f);
            List<Point3d> faceVerticesXYZ = new List<Point3d>();
            for (int i = 0; i < faceVertices.Length; i++)
            {
                faceVerticesXYZ.Add(new Point3d(pMesh.Vertices[faceVertices[i]].X, pMesh.Vertices[faceVertices[i]].Y, pMesh.Vertices[faceVertices[i]].Z));
            }

            Vector3d edge0 = new Vector3d(faceVerticesXYZ[1] - faceVerticesXYZ[0]);
            Vector3d edge1 = new Vector3d(faceVerticesXYZ[2] - faceVerticesXYZ[1]);

            Vector3d areaNormal = 0.5 * Vector3d.CrossProduct(edge0, edge1);

            return areaNormal;
        }


        // Calculate the vertex normals as weighted average of the adjacent face normals
        Vector3d calcVertexNormal(PlanktonMesh pMesh, int v)
        {
            int[] adjFaces = pMesh.Vertices.GetVertexFaces(v);
            Vector3d vNormal = new Vector3d(0, 0, 0);
            foreach (int face in adjFaces)
            {
                if (face != -1)
                {
                    vNormal += calcFaceNormal(pMesh, face);
                }
            }
            vNormal.Unitize();
            return vNormal;
        }


        //--------------------------------------------------------------------VERTEX AREAS---------------------------------------------------------//

        /* calculate the voronoi area (cotangent if non-obtuse else 1/2 or 1/4 T). Hybrid approach which handles obtuse triangles as well */
        public double calcVertexVoronoiArea(PlanktonMesh pMesh, int vertexIndex)
        {
            double areaV = 0.0;

            //adjacent triangular faces
            int[] vertexFaces = pMesh.Vertices.GetVertexFaces(vertexIndex);

            foreach (int faceIndex in vertexFaces)
            {
                if (faceIndex != -1)
                {
                    //if obtuse angle exists in triangle
                    if (isObtuse(pMesh, faceIndex))
                    {
                        double areaT = calcAreaTriangle(pMesh, faceIndex, vertexIndex);

                        //if v is the location of the obtuse angle
                        if (calcVertexAngle(pMesh, faceIndex, vertexIndex) >= Math.PI / 2)
                        {
                            areaV += areaT / 2;
                        }
                        else
                        {
                            areaV += areaT / 4;
                        }
                    }
                    //if non-obtuse
                    else
                    {
                        areaV += calcNonObtuseVoronoiArea(pMesh, faceIndex, vertexIndex);
                    }
                }
            }
            return areaV;
        }

        //Edge vectors from a vertex in a face
        public Vector3d[] getEdgeVectors(PlanktonMesh pMesh, int faceIndex, int vertexIndex)
        {
            int[] faceVertices = pMesh.Faces.GetFaceVertices(faceIndex);
            int indexV = Array.IndexOf(faceVertices, vertexIndex);

            //Shifted list of vertices as pointXYZ
            Vector3d[] faceVerticesXYZ = new Vector3d[faceVertices.Length];

            for (int i = 0; i < faceVertices.Length; i++)
            {
                int indexShift = (i + indexV) % faceVertices.Length;
                faceVerticesXYZ[i] = new Vector3d(pMesh.Vertices[faceVertices[indexShift]].X, pMesh.Vertices[faceVertices[indexShift]].Y, pMesh.Vertices[faceVertices[indexShift]].Z);
            }

            Vector3d[] edgeVectors = new Vector3d[2];
            edgeVectors[0] = Vector3d.Subtract(faceVerticesXYZ[1], faceVerticesXYZ[0]);
            edgeVectors[1] = Vector3d.Subtract(faceVerticesXYZ[2], faceVerticesXYZ[0]);

            return edgeVectors;
        }


        //AREA
        /* calculate area of a triangle as magnitude of cross product divided by 2 */
        public double calcAreaTriangle(PlanktonMesh pMesh, int faceIndex, int vertexIndex)
        {
            //edge vectors from any vertex in face
            Vector3d[] edgeVectors = getEdgeVectors(pMesh, faceIndex, vertexIndex);

            Vector3d vecArea = Vector3d.CrossProduct(edgeVectors[0], edgeVectors[1]);
            double area = Math.Abs(vecArea.Length / 2.0);

            return area;
        }


        //VERTEX ANGLE
        /* calculate the angle at a specific vertex in a face (n-gon) */
        public double calcVertexAngle(PlanktonMesh pMesh, int faceIndex, int vertexIndex)
        {
            Vector3d[] edgeVectors = getEdgeVectors(pMesh, faceIndex, vertexIndex);
            double angle = Vector3d.VectorAngle(edgeVectors[0], edgeVectors[1]);

            return angle;
        }


        //OBTUSE TEST
        /* test if there exists an obtuse/right angle in a given face (n-gon) */
        public bool isObtuse(PlanktonMesh pMesh, int faceIndex)
        {
            bool obtuse = false;

            int[] faceVertices = pMesh.Faces.GetFaceVertices(faceIndex);
            foreach (int i in faceVertices)
            {
                double angle = calcVertexAngle(pMesh, faceIndex, i);
                if (angle >= Math.PI / 2)
                {
                    obtuse = true;
                    break;
                }
            }
            return obtuse;
        }


        //NON-OBTUSE VORONOI AREA
        public double calcNonObtuseVoronoiArea(PlanktonMesh pMesh, int faceIndex, int vertexIndex)
        {
            Vector3d[] edgeVectors = getEdgeVectors(pMesh, faceIndex, vertexIndex);
            Vector3d vecPQ = edgeVectors[0];
            Vector3d vecPR = edgeVectors[1];

            //Find vertex indices of opposite angles to edges
            int[] faceVertices = pMesh.Faces.GetFaceVertices(faceIndex);
            int indexV = Array.IndexOf(faceVertices, vertexIndex);

            //Shifted list of vertices
            int[] faceVerticesShift = new int[faceVertices.Length];

            for (int i = 0; i < faceVertices.Length; i++)
            {
                int indexShift = (i + indexV) % faceVertices.Length;
                faceVerticesShift[i] = faceVertices[indexShift];
            }

            double angleQ = calcVertexAngle(pMesh, faceIndex, faceVerticesShift[1]);
            double angleR = calcVertexAngle(pMesh, faceIndex, faceVerticesShift[2]);

            //Area definition
            double areaV = (1 / 8.0) * ((Math.Pow(vecPR.Length, 2) * (1 / Math.Tan(angleQ))) + (Math.Pow(vecPQ.Length, 2) * (1 / Math.Tan(angleR))));

            return areaV;
        }


        //--------------------------------------------------------------------OTHER---------------------------------------------------------//
        //Create list of verticesXYZ
        List<Point3d> extractVerticesXYZ(PlanktonMesh pMesh)
        {
            List<Point3d> verticesXYZ = new List<Point3d>();

            for (int i = 0; i < pMesh.Vertices.Count; i++)
            {
                verticesXYZ.Add(new Point3d(pMesh.Vertices[i].X, pMesh.Vertices[i].Y, pMesh.Vertices[i].Z));
            }

            return verticesXYZ;
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
            get { return new Guid("{4f5d8b78-cd64-42e5-9e42-9bdb884ccd3e}"); }
        }
    }
}