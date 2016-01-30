using System;
using System.Drawing;
using Grasshopper.Kernel;

namespace BarElement
{
    public class BarElementInfo : GH_AssemblyInfo
    {
        public override string Name
        {
            get
            {
                return "BarElement";
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
                return new Guid("fd46a9db-3d20-45b7-b58f-a6e233e939a3");
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
