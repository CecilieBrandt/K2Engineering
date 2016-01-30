using System;
using System.Drawing;
using Grasshopper.Kernel;

namespace AxialVisualisation
{
    public class AxialVisualisationInfo : GH_AssemblyInfo
    {
        public override string Name
        {
            get
            {
                return "AxialVisualisation";
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
                return new Guid("15fa2a39-eaf3-4d7c-9db8-0d7d8ba7867c");
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
