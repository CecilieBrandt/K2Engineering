using System;
using System.Collections.Generic;
using KangarooSolver;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace BarElement
{
    public class BarElementComponent : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public BarElementComponent()
            : base("BarElement", "Bar",
                "A goal that represents a bar element with axial stiffness only. It outputs the extended/shortened line geometry and stress value (- compression, + tension)",
                "K2Struct", "Elements")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddLineParameter("Line", "Ln", "Line representing the bar element. Model has to be in mm", GH_ParamAccess.item);
            pManager.AddNumberParameter("E-Modulus", "E", "E-Modulus of the material in MPa", GH_ParamAccess.item);
            pManager.AddNumberParameter("Area", "A", "Cross-section area in mm2", GH_ParamAccess.item);
            pManager.AddNumberParameter("PreStress", "F", "Optional prestress in kN", GH_ParamAccess.item);
            pManager[3].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("B", "Bar", "Bar element with stress output", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //Input
            Line line = new Line();
            DA.GetData(0, ref line);

            double eModulus = 0.0;
            DA.GetData(1, ref eModulus);

            double area = 0.0;
            DA.GetData(2, ref area);

            double preStress = 0.0;
            if (this.Params.Input[3].SourceCount != 0)
            {
                DA.GetData(3, ref preStress);
            }


            //Create instance of bar
            GoalObject barElement = new Bar(line, eModulus, area, preStress);


            //Output
            DA.SetData(0, barElement);
        }


        public class Bar : GoalObject
        {
            double restLenght;
            bool isCompressionMember;
            double area;

            public Bar(Line L, double E, double A, double F)
            {
                restLenght = L.From.DistanceTo(L.To);
                isCompressionMember = true;
                area = A;

                PPos = new Point3d[2] {L.From, L.To};     // PPos must contain an array of the points this goal acts on
                Move = new Vector3d[2];       // Move is an array of vectors, one for each PPos
                Weighting = new double[2] { (2 * E * A) / restLenght, (2 * E * A) / restLenght }; // Weighting is an array of doubles for how strongly the goal affects each point 

                //Adjust restlenght if prestressed bar
                restLenght -= (F * 1000 * restLenght) / (E * A);
            }

            public override void Calculate(List<KangarooSolver.Particle> p)
            {
                Point3d ptStart = p[PIndex[0]].Position;             //get the current position of the particle at the start of the line
                Point3d ptEnd = p[PIndex[1]].Position;             //get the current position of the particle at the end of the line
                
                //Calculate force direction
                Vector3d forceDir = new Vector3d(ptEnd - ptStart);  //force direction pointing from start of line to end
                double currentLength = forceDir.Length;
                forceDir.Unitize();

                //Calculate extension
                double extension = currentLength - restLenght;

                if (extension > 0.0)
                {
                    isCompressionMember = false;
                }
                else if (extension < 0.0)
                {
                    isCompressionMember = true;
                }

                //Set vector direction and magnitude
                Move[0] = forceDir * (extension/2);                 //has to point to exact point according to Hooke's Law. Divide by 2 as the bar is extended in both directions with the same amount
                Move[1] = -forceDir * (extension/2);
            }

            //Stress in bar (ONE VALUE PER LINE ELEMENT)
            public override object Output(List<KangarooSolver.Particle> p)
            {
                double factor = 1.0;
                if (isCompressionMember)
                {
                    factor = -1.0;
                }

                var Data = new object[4] {PIndex[0], PIndex[1], new Line(p[PIndex[0]].Position, p[PIndex[1]].Position), ( factor * Weighting[0] * Move[0].Length) / area};     //Extended/contracted line element and its stress in [MPa]
                return Data;
            }

        }

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                // You can add image files to your project resources and access them like this:
                //return Resources.IconForThisComponent;
                return null;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("{aa5b569b-241f-4fe4-a45d-a137099fd485}"); }
        }
    }
}
