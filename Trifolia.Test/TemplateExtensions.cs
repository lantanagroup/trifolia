using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trifolia.DB
{
    public static class TemplateExtensions
    {
        public static void SetPreviousVersion(this Template template, Template previousVersion)
        {
            template.PreviousVersion = previousVersion;
            template.PreviousVersionTemplateId = previousVersion.Id;
        }
    }
}
