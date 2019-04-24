using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace K2Engineering
{
    public class CircularCS : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the CircularCS class.
        /// </summary>
        public CircularCS()
          : base("CircularCS", "Circle",
              "Calculate the cross section properties of a circular shape",
              "K2Eng", "5 Utility")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Diameter", "d", "The diameter of the circle [mm]", GH_ParamAccess.item);
            pManager.AddNumberParameter("Thickness", "t", "The thickness of the cross section [mm]. If nothing specified, the default is a solid", GH_ParamAccess.item);
            pManager[1].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("A", "A", "Cross section area [mm2]", GH_ParamAccess.item);
            pManager.AddNumberParameter("I", "I", "The moment of inertia [mm4]", GH_ParamAccess.item);
            pManager.AddNumberParameter("z", "z", "The distance to the outer fibre [mm]", GH_ParamAccess.item);
            pManager.AddNumberParameter("It", "It", "The torsional moment of inertia about the local x-axis [mm]", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //Input
            double d = 0.0;
            DA.GetData(0, ref d);

            double t = 0.0;
            DA.GetData(1, ref t);


            //Calculate
            double a = calcArea(d, t);
            double i = calcInertia(d, t);
            double z = d / 2.0;
            double it = calcTorsionalInertia(d, t);


            //Output
            DA.SetData(0, a);
            DA.SetData(1, i);
            DA.SetData(2, z);
            DA.SetData(3, it);
        }

        //Methods

        //Calculate area
        double calcArea(double d, double t)
        {
            double area = (Math.PI / 4.0) * Math.Pow(d, 2);

            if (t != 0.0)
            {
                double areaEmpty = (Math.PI / 4.0) * Math.Pow((d - 2.0 * t), 2);
                area -= areaEmpty;
            }

            return area;
        }

        //Calculate moment of inertia
        double calcInertia(double d, double t)
        {
            double inertia = (Math.PI / 64.0) * (Math.Pow(d, 4));

            if (t != 0.0)
            {
                double inertiaEmpty = (Math.PI / 64.0) * (Math.Pow((d - 2.0 * t), 4));
                inertia -= inertiaEmpty;
            }

            return inertia;
        }

        //Calculate torsional moment of inertia
        double calcTorsionalInertia(double d, double t)
        {
            double inertiaT = (Math.PI / 32.0) * (Math.Pow(d, 4));

            if (t != 0.0)
            {
                double inertiaEmpty = (Math.PI / 32.0) * (Math.Pow((d - 2.0 * t), 4));
                inertiaT -= inertiaEmpty;
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
                return Properties.Resources.Circular;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("{40f42bcb-17c5-463b-bbe7-39aea9f7a2d6}"); }
        }
    }
}