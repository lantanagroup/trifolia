using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trifolia.DB
{
    [Table("codesystem_identifier")]
    public partial class CodeSystemIdentifier
    {
        public CodeSystemIdentifier() { }

        [Column("id")]
        public int Id { get; set; }

        [Column("codeSystemId")]
        public int CodeSystemId { get; set; }

        [Column("identifier")]
        [StringLength(255)]
        public string Identifier { get; set; }

        [Column("isDefault")]
        public bool IsDefault { get; set; }

        [Column("type")]
        public IdentifierTypes Type { get; set; }

        public virtual CodeSystem CodeSystem { get; set; }
    }
}
