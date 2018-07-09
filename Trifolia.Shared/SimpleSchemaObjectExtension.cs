using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Trifolia.DB;

namespace Trifolia.Shared
{
    public static class SimpleSchemaObjectExtension
    {
        public static TemplateConstraint CreateComputable(this SimpleSchema.SchemaObject schemaObject, bool isOpen)
        {
            var newConstraint = new TemplateConstraint();

            newConstraint.Context = schemaObject.IsAttribute ? "@" + schemaObject.Name : schemaObject.Name;

            if (isOpen)
            {
                newConstraint.Conformance = schemaObject.Conformance;
                newConstraint.Cardinality = schemaObject.Cardinality;
            }
            else
            {
                newConstraint.Conformance = "SHALL";
                newConstraint.Cardinality = "0..0";
            }

            return newConstraint;
        }
    }
}
