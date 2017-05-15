using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trifolia.DB
{
    [Table("implementationguide_accessreq")]
    public class ImplementationGuideAccessRequest
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("implementationGuideId")]
        public int ImplementationGuideId { get; set; }
        
        [Column("permission")]
        public string Permission { get; set; }

        [Column("requestUserId")]
        public int RequestUserId { get; set; }

        [Column("message")]
        public string Message { get; set; }

        [Column("requestDate")]
        public DateTime RequestDate { get; set; }

        public virtual ImplementationGuide ImplementationGuide { get; set; }
        public virtual User RequestUser { get; set; }
    }
}
