using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;

namespace K2Engineering.DataTypes
{
    class SupportData
    {
        public Point3d Location;
        public Vector3d Reaction;

        public SupportData() { }
        public SupportData(Point3d location, Vector3d reaction)
        {
            Location = location;
            Reaction = reaction;
        }
    }
}
