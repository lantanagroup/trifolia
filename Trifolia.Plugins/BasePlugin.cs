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
            var igTypeNames = thisIgTypePluginAttributes.Cast<ImplementationGuideTypePluginAttribute>().Select(y => y.IGType);
            var exportAssembly = Assembly.Load("Trifolia.Export");
            var typesWithAttribute = (from t in exportAssembly.GetTypes()
                                      from p in t.GetCustomAttributes(typeof(ImplementationGuideTypePluginAttribute)).Cast<ImplementationGuideTypePluginAttribute>()
                                      join i in igTypeNames on p.IGType equals i
                                      select t)
                                      .Distinct();
            var types = typesWithAttribute.Where(y => y.IsClass && y.GetInterfaces().Contains(typeof(ITypeExporter)));

            if (types.Count() != 1)
                throw new Exception("Did not find one type that immplements ITypeExporter and has an ImplementationGuideTypePlugin attribute with a matching IGName");

            return (ITypeExporter)Activator.CreateInstance(types.First());

        }
    }
}
