using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trifolia.DB
{
    [Table("valueset_identifier")]
    public class ValueSetIdentifier
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("valueSetId")]
        public int ValueSetId { get; set; }

        [Column("identifier")]
        [StringLength(255)]
        public string Identifier { get; set; }

        [Column("isDefault")]
        public bool IsDefault { get; set; }

        [Column("type")]
        public ValueSetIdentifierTypes Type { get; set; }

        public virtual ValueSet ValueSet { get; set; }
    }

    public enum ValueSetIdentifierTypes
    {
        Oid = 0,
        HL7II = 1,
        HTTP = 2
    }
}
