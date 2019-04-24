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
            pManager.AddVectorParameter("Snow", "S", "The snow load as a vector indicating direction and magnitude [kN/m2]", GH_ParamAccess.item);
            pManager.AddVectorParameter("VertexNormals", "nA", "The vertex normals scaled according to the associated voronoi area [m2]", GH_ParamAccess.list);
            pManager.AddBooleanParameter("Option", "opt", "If true, the nodal load is set to zero if the normal is pointing downwards", GH_ParamAccess.item, true);
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
            Vector3d snow = new Vector3d();
            DA.GetData(0, ref snow);

            List<Vector3d> vertexNormals = new List<Vector3d>();
            DA.GetDataList(1, vertexNormals);

            bool opt = true;
            DA.GetData(2, ref opt);


            //Calculate
            List<Vector3d> nodalLoads = calcNodalSnowLoads(vertexNormals, snow, opt);


            //Output
            DA.SetDataList(0, nodalLoads);
        }


        //Methods

        //Calculate nodal snow loads
        List<Vector3d> calcNodalSnowLoads(List<Vector3d> vertexNormals, Vector3d snow, bool opt)
        {
            List<Vector3d> nodalLoads = new List<Vector3d>();

            for (int i = 0; i < vertexNormals.Count; i++)
            {
                Vector3d vN = vertexNormals[i];
                double vertexArea = vN.Length;                //m2
                vN.Unitize();

                double dot = Vector3d.Multiply(vN, Vector3d.ZAxis);

                Vector3d force = new Vector3d(0, 0, 0);
                if(dot >= 0.0)
                {
                    force = snow * vertexArea * 1e3;                   //N
                }

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