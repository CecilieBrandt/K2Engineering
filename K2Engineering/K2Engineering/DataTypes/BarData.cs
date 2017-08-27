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
        public int Index1;
        public int Index2;
        public Line BarLine;
        public double Force;
        public double Stress;

        public BarData()
        {
        }

        public BarData(int index1, int index2, Line barLine, double force, double stress)
        {
            Index1 = index1;
            Index2 = index2;
            BarLine = barLine;
            Force = force;
            Stress = stress;
        }
    }
}
