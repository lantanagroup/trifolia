using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trifolia.Shared.Validation
{
    public class ValidationResults
    {
        public ValidationResults()
        {
            this.Messages = new List<string>();
            this.TemplateResults = new List<TemplateValidationResult>();
        }

        public List<string> Messages { get; set; }
        public List<TemplateValidationResult> TemplateResults { get; set; }
        public bool RestrictDownload { get; set; }

        public List<string> GetAllMessages()
        {
            List<string> messages = new List<string>(this.Messages);

            foreach (var templateResult in this.TemplateResults.SelectMany(y => y.Items))
            {
                string msg = string.Format("{0} ({1}): {2}", templateResult.TemplateName, templateResult.Level, templateResult.Message);
                messages.Add(msg);
            }

            return messages;
        }
    }
}
