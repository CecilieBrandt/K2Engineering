using System;
using System.Drawing;
using Grasshopper.Kernel;

namespace ElasticRodElement
{
    public class ElasticRodElementInfo : GH_AssemblyInfo
    {
        public override string Name
        {
            get
            {
                return "ElasticRodElement";
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
                return new Guid("354f6967-55a4-4a92-a290-bba9f3a9c884");
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
