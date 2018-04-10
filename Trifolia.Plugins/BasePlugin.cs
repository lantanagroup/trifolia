using System;
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
            var igTypeExporterTypes =
                from a in AppDomain.CurrentDomain.GetAssemblies()
                from t in a.GetTypes()
                let attributes = t.GetCustomAttributes(typeof(ImplementationGuideTypePluginAttribute), true)
                let attribute = attributes != null && attributes.Length == 1 ? attributes.Cast<ImplementationGuideTypePluginAttribute>().First() : null
                where
                  t.IsAssignableFrom(typeof(ITypeExporter)) &&              // Type must implement ITypeExporter
                  attribute != null &&                                      // Must have ImplementationGuideType attribute
                  attribute.IGType == thisIgTypePluginAttribute.IGType      // IGType on attribute must match this class/type's IGType
                select t;

            if (igTypeExporterTypes.Count() != 1)
                throw new Exception("Did not find a single ITypeExporter with an ImplementationGuideType of " + thisIgTypePluginAttribute.IGType);

            var igTypeExporterType = igTypeExporterTypes.First();
            var igTypeExporter = Activator.CreateInstance(igTypeExporterType) as ITypeExporter;
            return igTypeExporter;
        }
    }
}
