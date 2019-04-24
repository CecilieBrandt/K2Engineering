using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace K2Engineering
{
    public class Shear : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the Shear class.
        /// </summary>
        public Shear()
          : base("Shear", "Shear",
              "Calculate the shear values per line segment as the difference in moments at the endpoints [kN]",
              "K2Eng", "3 Results")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddLineParameter("Line", "ln", "Line segment in [m]", GH_ParamAccess.list);
            pManager.AddNumberParameter("BendingMoment", "M", "The bending moments in [kNm]", GH_ParamAccess.list);
            pManager.AddPlaneParameter("BendingPlane", "pl", "The bending planes", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddVectorParameter("ShearVectors", "V", "The shear vector per line segment in [kN]", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //Input
            List<Line> lines = new List<Line>();
            DA.GetDataList(0, lines);

            List<double> moments = new List<double>();
            DA.GetDataList(1, moments);

            List<Plane> planes = new List<Plane>();
            DA.GetDataList(2, planes);


            //Calculate
            List<Vector3d> shearVectors = calcShearVectors(lines, moments, planes);


            //Output
            DA.SetDataList(0, shearVectors);
        }


        //Methods

        //Calculate shear vectors
        public static List<Vector3d> calcShearVectors(List<Line> lines, List<double> moments, List<Plane> planes)
        {
            List<Point3d> planeOrigins = extractPlaneOrigins(planes);

            List<Vector3d> shearVectors = new List<Vector3d>();
            for (int i = 0; i < lines.Count; i++)
            {
                int[] indexes = findEndIndexes(lines[i], planeOrigins);

                Vector3d shear = new Vector3d();
                if (indexes[0] != -1 && indexes[1] != -1)
                {
                    shear = calcShearVector(planes[indexes[0]], moments[indexes[0]], planes[indexes[1]], moments[indexes[1]], lines[i]);
                }
                else if (indexes[0] != -1)
                {
                    shear = calcShearVector(planes[indexes[0]], moments[indexes[0]], Plane.WorldXY, 0.0, lines[i]);     //Arbitrary other plane because it is multiplied by 0.0 kNm anyway
                }
                else
                {
                    shear = calcShearVector(Plane.WorldXY, 0.0, planes[indexes[1]], moments[indexes[1]], lines[i]);     //Arbitrary other plane because it is multiplied by 0.0 kNm anyway
                }

                shearVectors.Add(shear);
            }

            return shearVectors;
        }


        //Extract origin of planes
        public static List<Point3d> extractPlaneOrigins(List<Plane> planes)
        {
            List<Point3d> origins = new List<Point3d>();

            foreach(Plane pl in planes)
            {
                Point3d o = new Point3d(Math.Round(pl.OriginX, 3), Math.Round(pl.OriginY, 3), Math.Round(pl.OriginZ, 3));
                origins.Add(o);
            }

            return origins;
        }


        //Find the index of the endpoints of a line in the list of bending planes
        public static int[] findEndIndexes(Line ln, List<Point3d> planeOrigins)
        {
            Point3d lnStart = new Point3d(Math.Round(ln.FromX, 3), Math.Round(ln.FromY, 3), Math.Round(ln.FromZ, 3));
            Point3d lnEnd = new Point3d(Math.Round(ln.ToX, 3), Math.Round(ln.ToY, 3), Math.Round(ln.ToZ, 3));

            int indexStart = -1;
            int indexEnd = -1;

            for (int i=0; i<planeOrigins.Count; i++)
            {
                Point3d o = planeOrigins[i];

                if (o.Equals(lnStart))
                {
                    indexStart = i;
                }

                if (o.Equals(lnEnd))
                {
                    indexEnd = i;
                }
            }

            int[] indexes = new int[2] { indexStart, indexEnd };

            if(indexStart == -1 && indexEnd == -1)
            {
                throw new System.InvalidOperationException("Not able to find both start and end index for one or more lines");
            }

            return indexes;
        }


        //Calculate the shear vector from the difference in moments at the start and end of line segment
        public static Vector3d calcShearVector(Plane plStart, double momentStart, Plane plEnd, double momentEnd, Line ln)
        {
            //find the direction of the shear force
            Vector3d mdirStart = plStart.YAxis * momentStart;
            Vector3d mdirEnd = plEnd.YAxis * momentEnd;
            Vector3d mDiff = mdirEnd + mdirStart;                     
            mDiff.Unitize();

            //Detect if one of the moments is negative
            double fStart = 1.0;
            double fEnd = 1.0;
            if(Vector3d.Multiply(mdirStart, mDiff) < 0.0)
            {
                fStart = -1.0;
            }
            if (Vector3d.Multiply(mdirEnd, mDiff) < 0.0)
            {
                fEnd = -1.0;
            }

            //Get the length of the line
            Vector3d line = ln.From - ln.To;
            double length = line.Length;

            //Calculate the shear force as the difference in moments
            double deltaM = (fEnd * momentEnd) - (fStart * momentStart);                    //Assumption of symmetric cross section, so the difference in moments (possibly in different planes) equals the total shear force
            double shearValue = deltaM / length;

            //Find vector perpendicular to line segment
            Vector3d perp1 = Vector3d.CrossProduct(line, mDiff);
            Vector3d shearDir = Vector3d.CrossProduct(perp1, line);
            shearDir.Unitize();

            Vector3d shearVector = shearDir * shearValue;

            return shearVector;
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
                return Properties.Resources.Shear;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("{92f22337-5926-4e02-9c9d-a853eb62f980}"); }
        }
    }
}