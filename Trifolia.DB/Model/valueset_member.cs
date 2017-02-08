namespace Trifolia.DB
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("valueset_member")]
    public partial class ValueSetMember
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("valueSetId")]
        public int ValueSetId { get; set; }

        [Column("codeSystemId")]
        public int? CodeSystemId { get; set; }

        [Column("code")]
        [StringLength(255)]
        public string Code { get; set; }

        [Column("displayName")]
        [StringLength(1024)]
        public string DisplayName { get; set; }

        [Column("status")]
        [StringLength(255)]
        public string Status { get; set; }

        [Column("statusDate")]
        public DateTime? StatusDate { get; set; }

        public virtual CodeSystem CodeSystem { get; set; }

        public virtual ValueSet ValueSet { get; set; }
    }
}
