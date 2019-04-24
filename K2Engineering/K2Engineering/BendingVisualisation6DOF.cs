using System;
using System.Collections.Generic;
using System.Drawing;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System.Linq;

namespace K2Engineering
{
    public class BendingVisualisation6DOF : GH_Component
    {

        //Class properties
        public List<Mesh> meshes;
        public Color color;

        /// <summary>
        /// Initializes a new instance of the BeamBendingVisualisation class.
        /// </summary>
        public BendingVisualisation6DOF()
          : base("BendingVisualisation6DOF", "BendingDisplay",
              "Visualise the 6 DOF beam forces/moments",
              "K2Eng", "4 Display")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPlaneParameter("P0", "P0", "Local start plane", GH_ParamAccess.list);
            pManager.AddPlaneParameter("P1", "P1", "Local end plane", GH_ParamAccess.list);
            pManager.AddNumberParameter("Vy", "Vy", "The shear force parallel the local y-axis in [kN]", GH_ParamAccess.list);
            pManager.AddNumberParameter("Vz", "Vz", "The shear force parallel the local z-axis in [kN]", GH_ParamAccess.list);
            pManager.AddNumberParameter("Mt", "Mt", "The torsional moment in [kNm]", GH_ParamAccess.list);
            pManager.AddNumberParameter("My0", "My0", "The bending moment about the local y-axis at the start node in [kNm]", GH_ParamAccess.list);
            pManager.AddNumberParameter("Mz0", "Mz0", "The bending moment about the local z-axis at the start node in [kNm]", GH_ParamAccess.list);
            pManager.AddNumberParameter("My1", "My1", "The bending moment about the local y-axis at the end node in [kNm]", GH_ParamAccess.list);
            pManager.AddNumberParameter("Mz1", "Mz1", "The bending moment about the local z-axis at the end node in [kNm]", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Option", "opt", "0 = Vy, 1 = Vz, 2 = Mt, 3 = My, 4 = Mz", GH_ParamAccess.item, 3);
            pManager.AddNumberParameter("ScaleFactor", "sc", "The scale factor of the moment diagram", GH_ParamAccess.item, 0.5);
            pManager.AddColourParameter("Colour", "c", "Diagram colour", GH_ParamAccess.item, Color.Coral);
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

            List<double> Vy = new List<double>();
            DA.GetDataList(2, Vy);

            List<double> Vz = new List<double>();
            DA.GetDataList(3, Vz);

            List<double> Mt = new List<double>();
            DA.GetDataList(4, Mt);

            List<double> My0 = new List<double>();
            DA.GetDataList(5, My0);

            List<double> Mz0 = new List<double>();
            DA.GetDataList(6, Mz0);

            List<double> My1 = new List<double>();
            DA.GetDataList(7, My1);

            List<double> Mz1 = new List<double>();
            DA.GetDataList(8, Mz1);

            meshes = new List<Mesh>();

            int opt = 3;
            DA.GetData(9, ref opt);
            if (opt < 0)
            {
                opt = 0;
            }
            else if(opt > 4)
            {
                opt = 4;
            }

            double scale = 0.5;
            DA.GetData(10, ref scale);
            if (scale <= 0.0)
            {
                scale = 0.1;
            }

            DA.GetData(11, ref color);


            this.Message = "WIP";


            //Calculate

            //Find max value
            double max = 1.0;

            //Shear
            if (opt == 0 || opt == 1)
            {
                double maxVy = Math.Max(Math.Abs(Vy.Min()), Math.Abs(Vy.Max()));
                double maxVz = Math.Max(Math.Abs(Vz.Min()), Math.Abs(Vz.Max()));
                max = Math.Max(maxVy, maxVz);
            }

            //Moment
            else
            {
                double maxMt = Math.Max(Math.Abs(Mt.Min()), Math.Abs(Mt.Max()));
                double maxMy = Math.Max(Math.Max(Math.Abs(My0.Min()), Math.Abs(My0.Max())), Math.Max(Math.Abs(My1.Min()), Math.Abs(My1.Max())));
                double maxMz = Math.Max(Math.Max(Math.Abs(Mz0.Min()), Math.Abs(Mz0.Max())), Math.Max(Math.Abs(Mz1.Min()), Math.Abs(Mz1.Max())));

                double[] maxValues = new double[3] { maxMt, maxMy, maxMz };
                max = maxValues.Max();
            }


            //Scale values
            List<double> startValues = new List<double>();
            List<double> endValues = new List<double>();

            //Shear Vy
            if (opt == 0)
            {
                startValues = endValues = scaleValues(Vy, max, scale);
            }
            
            //Shear Vz
            else if(opt == 1)
            {
                startValues = endValues = scaleValues(Vz, max, scale);
            }

            //Torsion Mt
            else if (opt == 2)
            {
                startValues = endValues = scaleValues(Mt, max, scale);
            }

            //Moment My
            else if (opt == 3)
            {
                startValues = scaleValues(My0, max, scale);
                endValues = scaleValues(My1, max, scale);
            }

            //Moment Mz
            else
            {
                startValues = scaleValues(Mz0, max, scale);
                endValues = scaleValues(Mz1, max, scale);
            }

            //Create meshes
            for (int i = 0; i < P0.Count; i++)
            {
                //General
                Point3d p0 = P0[i].Origin;
                Point3d p1 = new Point3d();
                Point3d p2 = new Point3d();
                Point3d p3 = P1[i].Origin;

                Vector3d dir0 = new Vector3d();
                Vector3d dir1 = new Vector3d();

                //Shear Vy
                if (opt == 0)
                {
                    dir0 = P0[i].XAxis;
                    dir1 = P1[i].XAxis;
                    p1 = p0 + dir0 * startValues[i];
                    p2 = p3 + dir1 * endValues[i];
                }
                
                //Shear Vz
                else if (opt == 1)
                {
                    dir0 = P0[i].YAxis;
                    dir1 = P1[i].YAxis;
                    p1 = p0 + dir0 * startValues[i];
                    p2 = p3 + dir1 * endValues[i];
                }

                //Torsion Mt
                else if (opt == 2)
                {
                    dir0 = P0[i].YAxis;
                    dir1 = P1[i].YAxis;
                    p1 = p0 + dir0 * startValues[i];
                    p2 = p3 + dir1 * endValues[i];
                }

                //Moment My
                else if (opt == 3)
                {
                    dir0 = -P0[i].YAxis;
                    dir1 = -P1[i].YAxis;
                    p1 = p0 + dir0 * startValues[i];
                    p2 = p3 + dir1 * endValues[i] * (-1);
                }

                //Moment Mz
                else
                {
                    dir0 = -P0[i].XAxis;
                    dir1 = -P1[i].XAxis;
                    p1 = p0 + dir0 * startValues[i];
                    p2 = p3 + dir1 * endValues[i] * (-1);
                }

                //Create diagram mesh
                Mesh m = new Mesh();
                m.Vertices.Add(p0);
                m.Vertices.Add(p1);
                m.Vertices.Add(p2);
                m.Vertices.Add(p3);
                m.Faces.AddFace(0, 1, 2, 3);

                meshes.Add(m);
            }

        }

        //Scale values in list
        List<double> scaleValues(List<double> values, double max, double tmap)
        {
            List<double> valuesScaled = new List<double>();

            for(int i=0; i<values.Count; i++)
            {
                double val = (values[i] / max) * tmap;
                valuesScaled.Add(val);
            }

            return valuesScaled;
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
                        args.Display.DrawMeshWires(mesh, Color.White);
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
                return Properties.Resources.BeamDisplay;
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