using System;
using System.Drawing;
using Grasshopper.Kernel;

namespace K2Engineering
{
    public class K2EngineeringInfo : GH_AssemblyInfo
    {
        public override string Name
        {
            get
            {
                return "K2Engineering";
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
                return new Guid("0fd0c246-fadb-4ac7-9bed-b6ae0bfa2993");
            }
        }

        public override string AuthorName
        {
            get
            {
                //Return a string identifying you or your company.
                return "Cecilie Brandt-Olsen";
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
