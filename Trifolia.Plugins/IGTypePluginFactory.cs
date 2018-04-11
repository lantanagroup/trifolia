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
            //Collect all methods in script
            Assembly assembly = Assembly.GetExecutingAssembly();

            //Find all IGPlugins
            var IGPlugins = assembly.GetTypes()
                .Where(t => t.IsClass && t.GetCustomAttributes(typeof(ImplementationGuideTypePluginAttribute)).Any())
                .ToArray();

            
            foreach(var IGPlugin in IGPlugins)
            {
                //Get attributes of each plugin being examined
                var attributes = IGPlugin.GetCustomAttributes();
                
                //Check to see if the plugin is of the correct type by examining its attributes
                var pluginAtt = attributes.SingleOrDefault(a => a.GetType() == typeof(ImplementationGuideTypePluginAttribute) && ((ImplementationGuideTypePluginAttribute)a).IGType == igType.Name);

                //If pluginAtt isn't null, means the examined IGPlugin is of the right type
                if(pluginAtt != null)
                {
                    //Create an instance of the plugin and return it
                    var plugin = (IIGTypePlugin)Activator.CreateInstance(IGPlugin);
                    return plugin;
                }
            }

            //IG type plugin we're searching for doesn't exist (should never be the case but seems like a reasonable thing to have a check for)
            throw new NotSupportedException();
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
