using System;
using System.Collections.Generic;
using Plankton;
using System.Linq;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace K2Engineering
{
    public class MeshVertexArea : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MeshVertexArea class.
        /// </summary>
        public MeshVertexArea()
          : base("MeshVertexArea", "MVArea",
              "Calculate the voronoi area associated with each vertex of a mesh",
              "K2Eng", "2 Load")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("PlanktonMesh", "pMesh", "PlanktonMesh in [m]", GH_ParamAccess.item);
            pManager.AddBooleanParameter("ProjectToXY", "proj", "Project the mesh faces to the XY plane (useful for e.g. snow load calculation)", GH_ParamAccess.item, false);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Vertices", "V", "The mesh vertices", GH_ParamAccess.list);
            pManager.AddVectorParameter("VertexNormals", "n", "The vertex normals scaled according to the associated voronoi area [m2]", GH_ParamAccess.list);
            pManager.AddVectorParameter("area", "a", "Areas", GH_ParamAccess.list);
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

            bool projXY = false;
            DA.GetData(1, ref projXY);


            //Calculate
            PMeshExt pMeshE = new PMeshExt(pMesh);
            Point3d[] verticesXYZ = pMeshE.convertVerticesToXYZ();
            Vector3d[] vertexNormals = pMeshE.calcVertexNormals();

            Vector3d[] faceNormals = new Vector3d[pMeshE.Faces.Count];
            for(int i=0; i<pMeshE.Faces.Count; i++)
            {
                faceNormals[i] = pMeshE.calcFaceNormal(i);
            }



            //Output
            DA.SetDataList(0, verticesXYZ);
            DA.SetDataList(1, vertexNormals);
            DA.SetDataList(2, faceNormals);

        }

        //Methods
















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
            get { return new Guid("{9881eebc-de15-4d78-a45f-3375efbeb5a9}"); }
        }
    }
}