using System;
using System.Collections.Generic;
using Plankton;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace K2Structural
{
    public class MeshSelfweight : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MeshSelfweight class.
        /// </summary>
        public MeshSelfweight()
          : base("MeshSelfweight", "MSelfweight",
              "Calculate the selfweight of a mesh",
              "K2Structural", "Load")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("PlanktonMesh", "pMesh", "A plankton mesh in [mm]", GH_ParamAccess.item);
            pManager.AddNumberParameter("Thickness", "t", "The thickness in [mm]", GH_ParamAccess.item);
            pManager.AddNumberParameter("MaterialDensity", "rho", "The material density in [kg/m3]", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("Points", "pts", "A list of points which the forces act on", GH_ParamAccess.list);
            pManager.AddVectorParameter("NodalSelfweight", "Fweight", "The nodal selfweight in [N]", GH_ParamAccess.list);
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

            double thickness = 0.0;
            DA.GetData(1, ref thickness);

            double rho = 0.0;
            DA.GetData(2, ref rho);
            rho *= 1e-8;                // Density in [N/mm3]


            //Calculate
            List<Point3d> verticesXYZ = extractVerticesXYZ(pMesh);
            List<Vector3d> nodalLoads = calcNodalSelfweight(pMesh, thickness, rho);


            //Output
            DA.SetDataList(0, verticesXYZ);
            DA.SetDataList(1, nodalLoads);
        }


        //Methods

        //--------------------------------------------------------------------SELFWEIGHT---------------------------------------------------------//
        //Calculate selfweight
        List<Vector3d> calcNodalSelfweight(PlanktonMesh pMesh, double thickness, double rho)
        {
            List<Vector3d> nodalLoads = new List<Vector3d>();

            for (int i = 0; i < pMesh.Vertices.Count; i++)
            {
                Vector3d dir = new Vector3d(0, 0, -1.0);

                //Selfweight
                double vertexArea = calcVertexVoronoiArea(pMesh, i);
                Vector3d qs = dir * vertexArea * thickness * rho;               // Magnitude in [N]

                nodalLoads.Add(qs);
            }

            return nodalLoads;
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
            get { return new Guid("{6ff077ba-068b-438f-a751-aa26da15d95b}"); }
        }
    }
}