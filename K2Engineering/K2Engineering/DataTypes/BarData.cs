using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;

namespace K2Engineering.DataTypes
{
    public class BarData
    {
        public Line BarLine;
        public double Force;
        public double Stress;

        public BarData()
        {
        }

        public BarData(Line barLine, double force, double stress)
        {
            BarLine = barLine;
            Force = force;
            Stress = stress;
        }
    }
}
