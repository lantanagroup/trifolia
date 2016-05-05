using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

using Trifolia.Shared;
using Trifolia.DB;

namespace Trifolia.Web
{
    public class SimplifiedSchemaContext
    {
        public static SimpleSchema GetSimplifiedSchema(HttpContext context, ImplementationGuideType igType)
        {
            if (context != null)
                return GetSimplifiedSchema(context.Application, igType);

            return GetSimplifiedSchema((HttpApplicationState)null, igType);
        }

        public static SimpleSchema GetSimplifiedSchema(HttpApplicationState application, ImplementationGuideType igType)
        {
            if (application != null && application[igType.Name] != null)
                return (SimpleSchema)application[igType.Name];

            SimpleSchema newSimplifiedSchema =
                SimpleSchema.CreateSimpleSchema(Trifolia.Shared.Helper.GetIGSimplifiedSchemaLocation(igType));

            if (application != null)
                application.Add(igType.Name, newSimplifiedSchema);

            return newSimplifiedSchema;
        }

        public static void UpdateSimplifiedSchema(HttpApplicationState application, ImplementationGuideType igType, SimpleSchema schema)
        {
            if (application[igType.Name] != null)
                application.Remove(igType.Name);

            application.Add(igType.Name, schema);
        }

        public static SimpleSchema GetTemplateSimpleSchema(HttpApplicationState application, Template template)
        {
            string context = !string.IsNullOrEmpty(template.PrimaryContext) ? template.PrimaryContext : string.Empty;

            if (string.IsNullOrEmpty(context))
                context = template.TemplateType.RootContext;

            if (string.IsNullOrEmpty(context))
                return null;

            SimpleSchema schema = GetSimplifiedSchema(application, template.ImplementationGuideType);
            schema = schema.GetSchemaFromContext(context);

            return schema;
        }
    }
}
