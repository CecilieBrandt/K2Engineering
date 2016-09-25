using System;
using System.Collections.Generic;
using Plankton;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace K2Engineering
{
    public class MeshSnowLoad : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MeshSnowLoad class.
        /// </summary>
        public MeshSnowLoad()
          : base("MeshSnowLoad", "MSnow",
              "Calculate the snow load on a mesh",
              "K2Eng", "2 Load")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddVectorParameter("VertexNormals", "nA", "The vertex normals scaled according to the associated voronoi area (projected) [m2]", GH_ParamAccess.list);
            pManager.AddVectorParameter("Snow", "S", "The snow load as a vector indicating direction and magnitude [kN/m2]", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddVectorParameter("NodalSnowLoad", "FS", "The nodal snow loads in [N]", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //Input
            List<Vector3d> vertexNormals = new List<Vector3d>();
            DA.GetDataList(0, vertexNormals);

            Vector3d snow = new Vector3d();
            DA.GetData(1, ref snow);


            //Calculate
            List<Vector3d> nodalLoads = calcNodalSnowLoads(vertexNormals, snow);


            //Output
            DA.SetDataList(0, nodalLoads);
        }


        //Methods

        //Calculate nodal snow loads
        List<Vector3d> calcNodalSnowLoads(List<Vector3d> vertexNormals, Vector3d snow)
        {
            List<Vector3d> nodalLoads = new List<Vector3d>();

            for (int i = 0; i < vertexNormals.Count; i++)
            {
                double vertexArea = vertexNormals[i].Length;                //m2
                Vector3d force = snow * vertexArea * 1e3;                   //N
                nodalLoads.Add(force);
            }

            return nodalLoads;
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
                return Properties.Resources.Snow;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("{a8003bae-380e-4ddd-aefe-85384e84d79c}"); }
        }
    }
}