using System;
using System.Drawing;
using Grasshopper.Kernel;

namespace GH.MiscToolbox
{
    public class MiscToolboxInfo : GH_AssemblyInfo
    {
        public override string Name
        {
            get
            {
                return "MiscToolbox";
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
                return new Guid("3a8926a9-dd15-45e0-b209-a9a546b21e1e");
            }
        }

        public override string AuthorName
        {
            get
            {
                //Return a string identifying you or your company.
                return "Stamatios Psarras";
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

        public override string Version => "0.0.0.0";
        public override string AssemblyVersion => "0.0.*";
    }
}
