using System;
using System.Drawing;
using Grasshopper.Kernel;

namespace BendingVisualisation
{
    public class BendingVisualisationInfo : GH_AssemblyInfo
    {
        public override string Name
        {
            get
            {
                return "BendingVisualisation";
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
                return new Guid("a1c1f457-6410-4c81-a71e-0f095d3a61ca");
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
