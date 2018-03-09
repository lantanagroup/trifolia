using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trifolia.DB
{
    [Table("vValueSetMemberWhiteSpace")]
    public class ViewValueSetMemberWhiteSpace
    {
        [Column("valueSetId", Order = 0), Key]
        public int ValueSetId { get; set; }

        [Column("valueSetName", Order = 1)]
        public string ValueSetName { get; set; }

        [Column("code", Order = 2), Key]
        public string Code { get; set; }

        [Column("displayName", Order = 3)]
        public string DisplayName { get; set; }
    }
}
