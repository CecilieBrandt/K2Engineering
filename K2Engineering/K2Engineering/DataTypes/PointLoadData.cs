using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;

namespace K2Engineering.DataTypes
{
    public class PointLoadData
    {
        public Point3d Location;
        public Vector3d Load;

        public PointLoadData() {}

        public PointLoadData(Point3d location, Vector3d load)
        {
            Location = location;
            Load = load;
        }
    }
}
