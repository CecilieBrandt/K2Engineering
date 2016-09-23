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
            pManager.AddGenericParameter("PlanktonMesh", "pMesh", "A PlanktonMesh", GH_ParamAccess.item);
            pManager.AddBooleanParameter("ProjectToXY", "proj", "Project the mesh faces to the XY plane (useful for e.g. snow load calculation)", GH_ParamAccess.item, false);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("Vertices", "v", "The mesh vertices", GH_ParamAccess.list);
            pManager.AddVectorParameter("VertexNormals", "nA", "The vertex normals scaled according to the associated voronoi area", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //------------------INPUT--------------------//
            PlanktonMesh pMesh = new PlanktonMesh();
            DA.GetData(0, ref pMesh);

            bool projXY = false;
            DA.GetData(1, ref projXY);



            //------------------CALCULATE--------------------//
            PMeshExt pMeshE = new PMeshExt(pMesh);

            if (!pMeshE.isMeshTriangulated())
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "The mesh has to be triangulated");
            }

            //Extract vertices from initial 3d pMesh
            Point3d[] verticesXYZ = pMeshE.convertVerticesToXYZ();

            //If projected then create new pMesh
            if (projXY)
            {
                pMeshE = pMeshE.projectMeshToXY();
            }

            Vector3d[] vertexAreas = pMeshE.calcVertexVoronoiAreas(projXY);



            //------------------OUTPUT--------------------//
            DA.SetDataList(0, verticesXYZ);
            DA.SetDataList(1, vertexAreas);
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
            get { return new Guid("{9881eebc-de15-4d78-a45f-3375efbeb5a9}"); }
        }
    }
}