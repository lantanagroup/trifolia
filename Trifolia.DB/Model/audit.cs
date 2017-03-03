namespace Trifolia.DB
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("audit")]
    public partial class AuditEntry
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("username")]
        [Required]
        [StringLength(255)]
        public string Username { get; set; }

        [Column("auditDate")]
        public DateTime AuditDate { get; set; }

        [Column("ip")]
        [Required]
        [StringLength(50)]
        public string IP { get; set; }

        [Column("type")]
        [Required]
        [StringLength(128)]
        public string Type { get; set; }

        [Column("implementationGuideId")]
        public int? ImplementationGuideId { get; set; }

        [Column("templateId")]
        public int? TemplateId { get; set; }

        [Column("templateConstraintId")]
        public int? TemplateConstraintId { get; set; }

        [Column("note")]
        public string Note { get; set; }
    }
}
