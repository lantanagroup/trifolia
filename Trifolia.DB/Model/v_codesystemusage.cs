using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trifolia.DB
{
    [Table("v_codeSystemUsage")]
    public class ViewCodeSystemUsage
    {
        [Key, Column(Order = 0)]
        public int TemplateId { get; set; }

        [Column(Order = 1)]
        public string TemplateIdentifier { get; set; }

        [Column(Order = 2)]
        public string TemplateName { get; set; }

        [Column(Order = 3)]
        public string TemplateBookmark { get; set; }

        [Key, Column(Order = 4)]
        public int CodeSystemId { get; set; }
        
        [Column(Order = 5)]
        public string CodeSystemIdentifier { get; set; }

        [Column(Order = 6)]
        public string CodeSystemName { get; set; }
    }
}
