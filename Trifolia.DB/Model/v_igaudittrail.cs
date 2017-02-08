namespace Trifolia.DB
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("v_igaudittrail")]
    public partial class ViewIGAuditTrail
    {
        [Key]
        [Column("username", Order = 0)]
        [StringLength(255)]
        public string UserName { get; set; }

        [Key]
        [Column("auditDate", Order = 1)]
        public DateTime AuditDate { get; set; }

        [Column("ip", Order = 2)]
        [StringLength(50)]
        public string Ip { get; set; }

        [Key]
        [Column("type", Order = 3)]
        public string Type { get; set; }

        [Column("note", Order = 4)]
        public string Note { get; set; }

        [Column("implementationGuideId", Order = 5)]
        public int? ImplementationGuideId { get; set; }

        [Column("templateId", Order = 6)]
        public int? TemplateId { get; set; }

        [Column("templateConstraintId", Order = 7)]
        public int? TemplateConstraintId { get; set; }

        [Column("templateName", Order = 8)]
        [StringLength(255)]
        public string TemplateName { get; set; }

        [Column("conformanceNumber", Order = 9)]
        public int? ConformanceNumber { get; set; }
    }
}
