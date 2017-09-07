using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trifolia.Shared.Validation
{
    public class ValidationResult
    {
        public int? TemplateId { get; set; }
        public string TemplateName { get; set; }
        public int? ConstraintNumber { get; set; }
        public ValidationLevels Level { get; set; }
        public string Message { get; set; }

        public static ValidationResult CreateResult(ValidationLevels level, string messageFormat, params object[] args)
        {
            ValidationResult newResult = new ValidationResult()
            {
                Level = level,
                Message = string.Format(messageFormat, args)
            };

            return newResult;
        }

        public static ValidationResult CreateResult(int templateId, string templateName, ValidationLevels level, string messageFormat, params object[] args)
        {
            ValidationResult newResult = new ValidationResult()
            {
                TemplateId = templateId,
                TemplateName = templateName,
                Level = level,
                Message = string.Format(messageFormat, args)
            };

            return newResult;
        }

        public static ValidationResult CreateResult(int templateId, string templateName, int number, ValidationLevels level, string messageFormat, params object[] args)
        {
            ValidationResult newResult = new ValidationResult()
            {
                TemplateId = templateId,
                TemplateName = templateName,
                ConstraintNumber = number,
                Level = level,
                Message = string.Format(messageFormat, args)
            };

            return newResult;
        }
    }
}
