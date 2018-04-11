using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trifolia.Shared;

namespace Trifolia.Plugins
{
    public abstract class BasePlugin
    {
        /// <summary>
        /// This method is responsible for identifying an ITypeExporter that has an ImplementationGuideTypePlugin
        /// attribute that matches the IGType of this (derived) class's ImplementationGuideTypePlugin attribute.
        /// </summary>
        /// <returns>An ITypeExporter that can be used to export the ImplementationGuideType to various formats</returns>
        public ITypeExporter GetExporter()
        {
            var thisIgTypePluginAttributes = this.GetType().GetCustomAttributes(typeof(ImplementationGuideTypePluginAttribute), true);

            if (thisIgTypePluginAttributes.Count() != 1)
                throw new Exception("A single ImplementationGuideTypePlugin attribute is required for the plugin " + this.GetType().Name);

            var thisIgTypePluginAttribute = thisIgTypePluginAttributes.Cast<ImplementationGuideTypePluginAttribute>().First();

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var exports = assemblies.SingleOrDefault(e => e.FullName.Contains("Trifolia.Export"));
            if(exports != null)
            {
                //Find all IG Exporters
                var IGExports = exports.GetTypes()
                .Where(t => t.IsClass && t.GetCustomAttributes(typeof(ImplementationGuideTypePluginAttribute)).Any())
                .ToArray();

                foreach(var IGExport in IGExports)
                {
                    var attributes = IGExport.GetCustomAttributes();

                    //Check to see if the plugin is of the correct type by examining its attributes
                    var exportAtt = attributes.SingleOrDefault(a => a.GetType() == typeof(ImplementationGuideTypePluginAttribute) && ((ImplementationGuideTypePluginAttribute)a).IGType == thisIgTypePluginAttribute.IGType);

                    if (exportAtt != null)
                    {
                        //Create an instance of the plugin and return it
                        var igTypeExporter = (ITypeExporter)Activator.CreateInstance(IGExport);
                        return igTypeExporter;
                    }
                }

            }

            throw new Exception("Did not find a single ITypeExporter with an ImplementationGuideType of " + thisIgTypePluginAttribute.IGType);

        }
    }
}
