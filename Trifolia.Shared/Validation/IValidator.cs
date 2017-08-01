using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trifolia.DB;

namespace Trifolia.Shared.Validation
{
    public interface IValidator
    {
        ValidationResults ValidateImplementationGuide(int implementationGuideId);
        List<ValidationResult> ValidateTemplate(int templateId);
        List<ValidationResult> ValidateTemplate(Template template, SimpleSchema igSchema);
    }
}
