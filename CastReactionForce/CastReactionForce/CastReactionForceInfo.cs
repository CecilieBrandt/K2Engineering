using System;
using System.Drawing;
using Grasshopper.Kernel;

namespace CastReactionForce
{
    public class CastReactionForceInfo : GH_AssemblyInfo
    {
        public override string Name
        {
            get
            {
                return "CastReactionForce";
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
                return new Guid("e9b85822-5f5b-4ca2-9ad9-f3359261bc9b");
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
