using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace K2Engineering
{
    public class TransformMoment : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the TransformMoment class.
        /// </summary>
        public TransformMoment()
          : base("TransformMoment", "TransformMoment",
              "Transform a moment from one horizontal plane (XY) to another",
              "K2Eng", "5 Utility")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddVectorParameter("Moment", "M1", "The moment as a vector i.e. angle/axis representation", GH_ParamAccess.item);
            pManager.AddPlaneParameter("Plane1", "pln1", "Plane 1 (XY)", GH_ParamAccess.item);
            pManager.AddPlaneParameter("Plane2", "pln2", "Plane 2 (XY)", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddVectorParameter("TransformedMoment", "M2", "The transformed moment", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //Input
            Vector3d M1 = new Vector3d();
            DA.GetData(0, ref M1);

            Plane pln1 = new Plane();
            DA.GetData(1, ref pln1);

            Plane pln2 = new Plane();
            DA.GetData(2, ref pln2);


            //Calculate
            Vector3d M2 = transformMoment(M1.X, M1.Y, pln1, pln2);

            //Output
            DA.SetData(0, M2);
        }


        Vector3d transformMoment(double m1x, double m1y, Plane pln1, Plane pln2)
        {
            //Moment = force * distance. Keep force magnitude equal to moment and use unit distance
            //Mx moment (plane 1) is equivalent to
            Vector3d fx = new Vector3d(0, 0, m1x);
            Vector3d dx = pln1.YAxis;

            //My moment (plane 1) is equivalent to
            Vector3d fy = new Vector3d(0, 0, -m1y);
            Vector3d dy = pln1.XAxis;

            //Add to arrays
            Vector3d[] forces = new Vector3d[2] { fx, fy };
            Vector3d[] distances = new Vector3d[2] { dx, dy };

            //Equivalent moment (plane2)
            Vector3d M2 = new Vector3d(0, 0, 0);

            //run through Mx and My (plane1) contribution to moment (plane2)
            for (int i = 0; i < 2; i++)
            {

                //Contribution about x-axis (plane2)
                double dotx = Vector3d.Multiply(distances[i], pln2.YAxis);
                Vector3d dx2 = Vector3d.Multiply(pln2.YAxis, dotx);

                Vector3d crossx = Vector3d.CrossProduct(dx2, forces[i]);    //vector parallel to x-axis
                double m2x = forces[i].Length * dx2.Length;                 //moment magnitude

                if (Vector3d.Multiply(crossx, pln2.XAxis) < 0)
                {
                    m2x *= -1;                                                //reverse sign of moment
                }

                M2 += new Vector3d(m2x, 0, 0);


                //Contribution about y-axis (plane2)
                double doty = Vector3d.Multiply(distances[i], pln2.XAxis);
                Vector3d dy2 = Vector3d.Multiply(pln2.XAxis, doty);

                Vector3d crossy = Vector3d.CrossProduct(dy2, forces[i]);    //vector parallel to x-axis
                double m2y = forces[i].Length * dy2.Length;                 //moment magnitude

                if (Vector3d.Multiply(crossy, pln2.YAxis) < 0)
                {
                    m2y *= -1;                                                //reverse sign of moment
                }

                M2 += new Vector3d(0, m2y, 0);
            }

            return M2;
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
                return Properties.Resources.TransformMoment;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("058fb304-139f-4a9c-ac05-d8a7506fb3b6"); }
        }
    }
}