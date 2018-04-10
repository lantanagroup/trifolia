using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Trifolia.DB;
using Trifolia.Config;

namespace Trifolia.Plugins
{
    public class IGTypePluginFactory
    {
        public static IIGTypePlugin GetPlugin(ImplementationGuideType igType)
        {
            /*
            IGTypeSection config = IGTypeSection.GetSection();
            IGTypePluginElement configElement = config.Plugins[igType.Name];

            if (configElement == null)
                throw new Exception("Plugin not configured for this type of schema!");

            Type type = Type.GetType(configElement.PluginAssembly);

            if (type == null)
                throw new Exception("Plugin not loaded for IG Type: " + configElement.PluginAssembly);

            var pluginInterface = (IIGTypePlugin)Activator.CreateInstance(type);
            return pluginInterface;
            */

            // TODO: Fill in logic to find an IIGTypePlugin that matches the igType.Name based on ImplementationGuideTypeAttribute.IGType
            return null;
        }
    }

    public static class ImplementationGuideTypeExtension
    {
        public static IIGTypePlugin GetPlugin(this ImplementationGuideType igType)
        {
            return IGTypePluginFactory.GetPlugin(igType);
        }
    }
}
