using System;
using System.Drawing;
using Grasshopper.Kernel;

namespace CastBendingStress
{
    public class CastBendingStressInfo : GH_AssemblyInfo
    {
        public override string Name
        {
            get
            {
                return "CastBendingStress";
            }
        }
        public override Bitmap Icon
        {
            get
            {
                //Return a 24x24 pixel bitmap to represent this GHA library.
                return null;
            }
        }
        public override string Description
        {
            get
            {
                //Return a short string describing the purpose of this GHA library.
                return "";
            }
        }
        public override Guid Id
        {
            get
            {
                return new Guid("9c3b33cf-1067-4157-80bf-3d33c58eb4be");
            }
        }

        public override string AuthorName
        {
            get
            {
                //Return a string identifying you or your company.
                return "";
            }
        }
        public override string AuthorContact
        {
            get
            {
                //Return a string representing your preferred contact details.
                return "";
            }
        }
    }
}
