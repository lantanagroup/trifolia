using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trifolia.Plugins
{
    //Needs a version parameter to easily link the plugin to its corresponding IGController
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class ImplementationGuideTypePlugin : Attribute
    {
        private String igType;

        public ImplementationGuideTypePlugin(String igType)
        {
            this.igType = igType;
        }

        public String IGType
        {
            get { return this.igType; }
        }
    }
}
