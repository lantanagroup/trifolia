using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Trifolia.DB;
using Trifolia.Config;

namespace Trifolia.Shared.IGTypePlugin
{
    public class IGTypePluginFactory
    {
        public static IIGTypePlugin GetPlugin(ImplementationGuideType igType)
        {
            IGTypeSection config = IGTypeSection.GetSection();
            IGTypePluginElement configElement = config.Plugins[igType.Name];

            if (configElement == null)
                throw new Exception("Plugin not configured for this type of schema!");

            Type type = Type.GetType(configElement.PluginAssembly);

            if (type == null)
                throw new Exception("Plugin not loaded for IG Type: " + configElement.PluginAssembly);

            return (IIGTypePlugin)Activator.CreateInstance(type);
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
