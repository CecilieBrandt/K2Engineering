using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace K2Engineering.DataTypes
{
    public class PressureData
    {
        public Point3d[] Locations;
        public Vector3d[] Loads;
        public double PresStart;
        public double PresEnd;
        public double VolStart;
        public double VolEnd;
        public int MolStart;
        public int MolEnd;

        public PressureData() { }

        public PressureData(Point3d[] locations, Vector3d[] loads, double presStart, double presEnd, double volStart, double volEnd, int molStart, int molEnd)
        {
            Locations = new Point3d[locations.Length];
            Loads = new Vector3d[loads.Length];

            for(int i=0; i<locations.Length; i++)
            {
                Locations[i] = locations[i];
                Loads[i] = loads[i];
            }

            PresStart = presStart;
            PresEnd = presEnd;
            VolStart = volStart;
            VolEnd = volEnd;
            MolStart = molStart;
            MolEnd = molEnd;
        }
    }
}
