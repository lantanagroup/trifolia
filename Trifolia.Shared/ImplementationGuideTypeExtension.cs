using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Schema;

using Trifolia.DB;

namespace Trifolia.Shared
{
    public static class ImplementationGuideTypeExtension
    {
        public static SimpleSchema GetSimpleSchema(this ImplementationGuideType igType)
        {
            string schemaLocation = Trifolia.Shared.Helper.GetIGSimplifiedSchemaLocation(igType);
            return SimpleSchema.CreateSimpleSchema(schemaLocation);
        }

        public static List<TemplateType> GetRootTemplateTypes(this ImplementationGuideType igType)
        {
            XmlSchema schema = Helper.GetIGSchema(igType);
            List<TemplateType> templateTypes = new List<TemplateType>();
            XmlSchemaElement rootElement = null;

            foreach (var item in schema.Items)
            {
                XmlSchemaElement element = item as XmlSchemaElement;

                if (element != null)
                {
                    rootElement = element;
                    break;
                }
            }

            if (rootElement != null)
            {
                foreach (TemplateType templateType in igType.TemplateTypes)
                {
                    if (templateType.RootContext == rootElement.Name)
                        templateTypes.Add(templateType);
                }
            }

            return templateTypes;
        }
    }
}
