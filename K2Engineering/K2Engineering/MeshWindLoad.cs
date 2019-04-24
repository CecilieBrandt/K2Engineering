using System;
using System.Collections.Generic;
using Plankton;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace K2Engineering
{
    public class MeshWindLoad : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MeshWindLoad class.
        /// </summary>
        public MeshWindLoad()
          : base("MeshWindLoad", "MWind",
              "Calculate wind load on a mesh",
              "K2Eng", "2 Load")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddVectorParameter("Wind", "W", "The wind load as a vector indicating direction and magnitude [kN/m2]", GH_ParamAccess.item);
            pManager.AddVectorParameter("VertexNormals", "nA", "The vertex normals scaled according to the associated voronoi area [m2]", GH_ParamAccess.list);
            pManager.AddBooleanParameter("Option", "opt", "True: normal direction (pressure/suction). False: constant direction parallel to load vector", GH_ParamAccess.item, true);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddVectorParameter("NodalWindLoad", "FW", "The nodal wind loads in [N]", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //Input
            Vector3d wind = new Vector3d();
            DA.GetData(0, ref wind);

            List<Vector3d> vertexNormals = new List<Vector3d>();
            DA.GetDataList(1, vertexNormals);

            bool opt = true;
            DA.GetData(2, ref opt);


            //Calculate
            List<Vector3d> nodalLoads = calcNodalWindLoads(vertexNormals, wind, opt);


            //Output
            DA.SetDataList(0, nodalLoads);
        }


        //Methods

        List<Vector3d> calcNodalWindLoads(List<Vector3d> vertexNormals, Vector3d wind, bool opt)
        {
            List<Vector3d> windload = new List<Vector3d>();

            double w = wind.Length;                                 //kN/m2
            Vector3d wDir = new Vector3d(wind);
            wDir.Unitize();

            for(int i=0; i<vertexNormals.Count; i++)
            {
                Vector3d vN = vertexNormals[i];
                double vertexArea = vN.Length;                      //m2
                vN.Unitize();

                //Calculate wind force
                double forceMagnitude = vertexArea * w * 1e3;         //N
                Vector3d force = new Vector3d();

                if (opt == true)
                {
                    force = vN * forceMagnitude;

                    double dotProduct = Vector3d.Multiply(vN, wDir);

                    if (dotProduct < 0.0)
                    {
                        force.Reverse();
                    }
                }

                else
                {
                    Vector3d forceDir = new Vector3d(wind);
                    forceDir.Unitize();
                    force = forceDir * forceMagnitude;
                }

                windload.Add(force);
            }

            return windload;
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
                return Properties.Resources.Wind;
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