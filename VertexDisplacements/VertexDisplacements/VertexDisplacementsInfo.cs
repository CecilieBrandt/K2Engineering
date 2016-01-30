using System;
using System.Drawing;
using Grasshopper.Kernel;

namespace VertexDisplacements
{
    public class VertexDisplacementsInfo : GH_AssemblyInfo
    {
        public override string Name
        {
            get
            {
                return "VertexDisplacements";
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
                return new Guid("da3e192f-1c0d-448e-8f04-89072ce29e54");
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
