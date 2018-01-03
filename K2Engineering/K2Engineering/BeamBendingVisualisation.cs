using System;
using System.Collections.Generic;
using System.Drawing;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System.Linq;

namespace K2Engineering
{
    public class BeamBendingVisualisation : GH_Component
    {

        //Class properties
        public List<Mesh> meshes;
        public Color color;

        /// <summary>
        /// Initializes a new instance of the BeamBendingVisualisation class.
        /// </summary>
        public BeamBendingVisualisation()
          : base("BeamBendingVisualisation", "BendingDisplay",
              "Visualise the bending stress with colour",
              "K2Eng", "5 Display")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPlaneParameter("P0", "P0", "Local start plane", GH_ParamAccess.list);
            pManager.AddPlaneParameter("P1", "P1", "Local end plane", GH_ParamAccess.list);
            pManager.AddNumberParameter("Mt", "Mt", "The torsional moment in [kNm]", GH_ParamAccess.list);
            pManager.AddNumberParameter("My0", "My0", "The bending moment about the local y-axis at the start node in [kNm]", GH_ParamAccess.list);
            pManager.AddNumberParameter("Mz0", "Mz0", "The bending moment about the local z-axis at the start node in [kNm]", GH_ParamAccess.list);
            pManager.AddNumberParameter("My1", "My1", "The bending moment about the local y-axis at the end node in [kNm]", GH_ParamAccess.list);
            pManager.AddNumberParameter("Mz1", "Mz1", "The bending moment about the local z-axis at the end node in [kNm]", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Option", "opt", "0: Mt, 1: My, 2: Mz", GH_ParamAccess.item, 1);
            pManager.AddNumberParameter("ScaleFactor", "sc", "The scale factor of the moment diagram", GH_ParamAccess.item, 0.5);
            pManager.AddColourParameter("Colour", "c", "Bending moment diagram colour", GH_ParamAccess.item, Color.Coral);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //Input
            List<Plane> P0 = new List<Plane>();
            DA.GetDataList(0, P0);

            List<Plane> P1 = new List<Plane>();
            DA.GetDataList(1, P1);

            List<double> Mt = new List<double>();
            DA.GetDataList(2, Mt);

            List<double> My0 = new List<double>();
            DA.GetDataList(3, My0);

            List<double> Mz0 = new List<double>();
            DA.GetDataList(4, Mz0);

            List<double> My1 = new List<double>();
            DA.GetDataList(5, My1);

            List<double> Mz1 = new List<double>();
            DA.GetDataList(6, Mz1);

            int opt = 1;
            DA.GetData(7, ref opt);
            if (opt < 0)
            {
                opt = 0;
            }
            else if(opt > 2)
            {
                opt = 2;
            }

            double scale = 0.5;
            DA.GetData(8, ref scale);
            if (scale <= 0.0)
            {
                scale = 0.1;
            }

            //Global properties
            meshes = new List<Mesh>();
            DA.GetData(9, ref color);


            //Calculate
            
            //Find max value in lists and scale accordingly
            double Mmax = 0.0;
            if (opt == 0)
            {
                Mmax = Math.Max(Math.Abs(Mt.Min()), Math.Abs(Mt.Max()));
            }
            else if(opt == 1)
            {
                Mmax = Math.Max(Math.Max(Math.Abs(My0.Min()), Math.Abs(My0.Max())), Math.Max(Math.Abs(My1.Min()), Math.Abs(My1.Max())));
            }
            else
            {
                Mmax = Math.Max(Math.Max(Math.Abs(Mz0.Min()), Math.Abs(Mz0.Max())), Math.Max(Math.Abs(Mz1.Min()), Math.Abs(Mz1.Max())));
            }

            List<double> M0Scaled = new List<double>();
            List<double> M1Scaled = new List<double>();
            for (int i = 0; i < P0.Count; i++)
            {
                if (opt == 0)
                {
                    M0Scaled.Add((Mt[i] / Mmax) * scale);
                    M1Scaled.Add((Mt[i] / Mmax) * scale);
                }
                else if (opt == 1)
                {
                    M0Scaled.Add((My0[i] / Mmax) * scale);
                    M1Scaled.Add((My1[i] / Mmax) * scale);
                }
                else
                {
                    M0Scaled.Add((Mz0[i] / Mmax) * scale);
                    M1Scaled.Add((Mz1[i] / Mmax) * scale);
                }

            }

            //Run through lists
            for (int i = 0; i < P0.Count; i++)
            {
                //start/end points
                Point3d p0 = P0[i].Origin;
                Point3d p3 = P1[i].Origin;

                //Diagram direction
                Vector3d dir0 = new Vector3d(0,0,0);
                Vector3d dir1 = new Vector3d(0,0,0);
                if (opt == 0)
                {
                    dir0 = P0[i].YAxis;
                    dir1 = P1[i].YAxis;  
                }
                else if (opt == 1)
                {
                    dir0 = P0[i].YAxis;
                    dir1 = P1[i].YAxis;
                }
                else
                {
                    dir0 = P0[i].XAxis;
                    dir1 = P1[i].XAxis;
                }

                //Middle points
                Point3d p1 = p0 + dir0 * M0Scaled[i];
                Point3d p2 = p3 + dir1 * M1Scaled[i] * (-1);

                Mesh m = new Mesh();
                m.Vertices.Add(p0);
                m.Vertices.Add(p1);
                m.Vertices.Add(p2);
                m.Vertices.Add(p3);
                m.Faces.AddFace(0, 1, 2, 3);

                meshes.Add(m);
            }

        }

        //Preview meshes in Rhino
        public override void DrawViewportMeshes(IGH_PreviewArgs args)
        {
            if (Hidden) { return; }             //if the component is hidden
            if (Locked) { return; }              //if the component is locked

            base.DrawViewportMeshes(args);
            base.DrawViewportWires(args);

            Rhino.Display.DisplayMaterial mat = new Rhino.Display.DisplayMaterial(color, 0);

            if (meshes != null)
            {
                foreach (Mesh mesh in meshes)
                {
                    if (mesh != null && mesh.IsValid)
                    {
                        args.Display.DrawMeshShaded(mesh, mat);
                        args.Display.DrawMeshWires(mesh, color);
                    }
                }
            }
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
            get { return new Guid("ed49e8dd-2c0f-444c-947a-c582a1676b1e"); }
        }
    }
}