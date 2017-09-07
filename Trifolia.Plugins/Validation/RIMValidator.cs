using System.Collections.Generic;
using Trifolia.DB;
using Trifolia.Shared;
using Trifolia.Shared.Validation;

namespace Trifolia.Plugins.Validation
{
    public class RIMValidator : BaseValidator
    {
        public RIMValidator(IObjectRepository tdb)
         : base(tdb)
        {

        }

        public override List<ValidationResult> ValidateTemplate(Template template, SimpleSchema igSchema, IEnumerable<Template> allContainedTemplates = null)
        {
            return base.ValidateTemplate(template, igSchema, allContainedTemplates);
        }
    }
}
