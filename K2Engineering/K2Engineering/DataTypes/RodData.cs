using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;

namespace K2Engineering.DataTypes
{
    class RodData
    {
        public int SharedPointIndex;
        public Plane BendingPlane;
        public double Moment;
        public double BendingStress;

        public RodData() { }

        public RodData(int sharedPointIndex, Plane bendingPlane, double moment, double bendingStress)
        {
            SharedPointIndex = sharedPointIndex;
            BendingPlane = bendingPlane;
            Moment = moment;
            BendingStress = bendingStress;
        }
    }
}
