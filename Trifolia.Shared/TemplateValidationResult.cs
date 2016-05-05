using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Trifolia.Shared
{
    public enum ValidationLevels
    {
        Warning,
        Error
    }

    public class TemplateValidationResult
    {
        public int? ConstraintNumber { get; set; }
        public ValidationLevels Level { get; set; }
        public string Message { get; set; }

        public static TemplateValidationResult CreateResult(ValidationLevels level, string message)
        {
            return CreateResult(null, level, message);
        }

        public static TemplateValidationResult CreateResult(int? number, ValidationLevels level, string messageFormat, params object[] args)
        {
            TemplateValidationResult newResult = new TemplateValidationResult()
            {
                ConstraintNumber = number,
                Level = level,
                Message = string.Format(messageFormat, args)
            };

            return newResult;
        }
    }
}
