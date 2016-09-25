using System;
using System.Collections.Generic;
using Plankton;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace K2Engineering
{
    public class MeshSelfweight : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MeshSelfweight class.
        /// </summary>
        public MeshSelfweight()
          : base("MeshSelfweight", "MSelfweight",
              "Calculate the selfweight of a mesh",
              "K2Eng", "2 Load")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddVectorParameter("VertexNormals", "nA", "The vertex normals scaled according to the associated voronoi area [m2]", GH_ParamAccess.list);
            pManager.AddNumberParameter("Thickness", "t", "The thickness in [mm]", GH_ParamAccess.item);
            pManager.AddNumberParameter("MaterialDensity", "rho", "The material density in [kg/m3]", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddVectorParameter("NodalSelfweight", "FG", "The nodal selfweight in [N]", GH_ParamAccess.list);
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

            double thickness = 0.0;
            DA.GetData(1, ref thickness);

            double rho = 0.0;
            DA.GetData(2, ref rho);


            //Calculate
            List<Vector3d> nodalLoads = calcNodalSelfweight(vertexNormals, thickness, rho);


            //Output
            DA.SetDataList(0, nodalLoads);
        }


        //Methods

        //Calculate selfweight
        List<Vector3d> calcNodalSelfweight(List<Vector3d> vertexNormals, double thickness, double rho)
        {
            List<Vector3d> nodalLoads = new List<Vector3d>();

            Vector3d dir = Vector3d.ZAxis * -1;

            for (int i = 0; i < vertexNormals.Count; i++)
            {
                double vertexArea = vertexNormals[i].Length;                                    //m2
                Vector3d force = dir * vertexArea * (thickness * 1e-3) * rho * 9.82;            //N

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
                return Properties.Resources.Gravity;
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