using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;

namespace K2Engineering.DataTypes
{
    public class RodData
    {
        public Plane BendingPlane;
        public double Moment;
        public double BendingStress;

        public RodData() { }

        public RodData(Plane bendingPlane, double moment, double bendingStress)
        {
            BendingPlane = bendingPlane;
            Moment = moment;
            BendingStress = bendingStress;
        }
    }
}
