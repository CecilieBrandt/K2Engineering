using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;

namespace K2Engineering.DataTypes
{
    public class BeamData
    {
        public Plane P0;
        public Plane P1;
        public double N;
        public double Mx;
        public double My0;
        public double Mz0;
        public double My1;
        public double Mz1;

        public BeamData()
        {
        }

        public BeamData(Plane p0, Plane p1, double n, double mx, double my0, double mz0, double my1, double mz1)
        {
            P0 = p0;
            P1 = p1;
            N = n;
            Mx = mx;
            My0 = my0;
            Mz0 = mz0;
            My1 = my1;
            Mz1 = mz1;
        }
    }
}
