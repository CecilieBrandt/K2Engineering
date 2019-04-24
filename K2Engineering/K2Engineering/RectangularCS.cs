using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace K2Engineering
{
    public class RectangularCS : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the RectangularCS class.
        /// </summary>
        public RectangularCS()
          : base("RectangularCS", "Rectangle",
              "Calculate the cross section properties of a rectangular shape",
              "K2Eng", "6 Utility")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Width", "w", "The width of the cross section [mm]", GH_ParamAccess.item);
            pManager.AddNumberParameter("Height", "h", "The height of the cross section [mm]", GH_ParamAccess.item);
            pManager.AddNumberParameter("Thickness", "t", "The thickness of the cross section in mm. If nothing specified, the default is a solid", GH_ParamAccess.item);
            pManager[2].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("A", "A", "Cross section area [mm2]", GH_ParamAccess.item);
            pManager.AddNumberParameter("Iy", "Iy", "Moment of inertia about the local y-axis [mm4]", GH_ParamAccess.item);
            pManager.AddNumberParameter("Iz", "Iz", "Moment of inertia about the local z-axis [mm4]", GH_ParamAccess.item);
            pManager.AddNumberParameter("It", "It", "Torsional moment of inertia about the local x-axis [mm4]", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //Input
            double w = 0.0;
            DA.GetData(0, ref w);

            double h = 0.0;
            DA.GetData(1, ref h);

            double t = 0.0;
            DA.GetData(2, ref t);


            //Calculate
            double a = calcArea(w, h, t);
            double iy = calcInertia(w, h, t);
            double iz = calcInertia(h, w, t);
            double it = calcTorsionalInertia(w, h, t);


            //Output
            DA.SetData(0, a);
            DA.SetData(1, iy);
            DA.SetData(2, iz);
            DA.SetData(3, it);
        }

        //Methods

        //Calculate area
        double calcArea(double w, double h, double t)
        {
            double area = w * h;

            if (t != 0.0)
            {
                double areaEmpty = (w - 2.0 * t) * (h - 2.0 * t);
                area -= areaEmpty;
            }

            return area;
        }

        //Calculate moment of inertia
        double calcInertia(double w, double h, double t)
        {
            double inertia = (1 / 12.0) * w * Math.Pow(h, 3);

            if (t != 0.0)
            {
                double inertiaEmpty = (1 / 12.0) * (w - 2.0 * t) * Math.Pow((h - 2.0 * t), 3);
                inertia -= inertiaEmpty;
            }

            return inertia;
        }

        //Calculate torsional moment of inertia
        double calcTorsionalInertia(double w, double h, double t)
        {
            double a = h;
            double b = w;

            if (w > h)
            {
                a = w;
                b = h;
            }

            double inertiaT = 0.0;

            if (t == 0.0)
            {
                inertiaT = a * Math.Pow(b, 3) * (1.0 / 3.0 - 0.21 * b / a * (1 - Math.Pow(b, 4) / (12 * Math.Pow(a, 4))));
            }

            else
            {
                inertiaT = (2 * Math.Pow(t,2) * Math.Pow((a-2),2) * Math.Pow((b-t),2) ) / (a*t + b*t - 2*Math.Pow(t,2));
            }

            return inertiaT;
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
                return Properties.Resources.Rectangular;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("{b2c01d87-2e99-4bdd-82af-247158b8d19a}"); }
        }
    }
}