using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;

namespace K2Engineering.DataTypes
{
    class Support6DOFData
    {
        public Plane Pln;
        public Vector3d ReactionForce;
        public Vector3d ReactionMoment;

        public Support6DOFData() { }
        public Support6DOFData(Plane pln, Vector3d rf, Vector3d rm)
        {
            Pln = pln;
            ReactionForce = rf;
            ReactionMoment = rm;
        }
    }
}
