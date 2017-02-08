namespace Trifolia.DB
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("v_template")]
    public partial class ViewTemplate
    {
        [Key]
        [Column("id", Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        [Key]
        [Column("oid", Order = 1)]
        [StringLength(255)]
        public string Oid { get; set; }

        [Key]
        [Column("name", Order = 2)]
        [StringLength(255)]
        public string Name { get; set; }

        [Key]
        [Column("isOpen", Order = 3)]
        public bool IsOpen { get; set; }
        
        [Column("owningImplementationGuideId", Order = 4)]
        public int? OwningImplementationGuideId { get; set; }

        [Column(Order = 5)]
        [StringLength(255)]
        public string OwningImplementationGuideTitle { get; set; }

        [Key]
        [Column("implementationGuideTypeName", Order = 6)]
        [StringLength(255)]
        public string ImplementationGuideTypeName { get; set; }

        [Key]
        [Column("templateTypeId", Order = 7)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int TemplateTypeId { get; set; }

        [Key]
        [Column("templateTypeName", Order = 8)]
        [StringLength(255)]
        public string TemplateTypeName { get; set; }

        [Column("primaryContext", Order = 9)]
        [StringLength(255)]
        public string PrimaryContext { get; set; }

        [Key]
        [Column("templateTypeDisplay", Order = 10)]
        [StringLength(513)]
        public string TemplateTypeDisplay { get; set; }

        [Column("organizationName", Order = 11)]
        [StringLength(255)]
        public string OrganizationName { get; set; }

        [Column("publishDate", Order = 12)]
        public DateTime? PublishDate { get; set; }

        [Column("impliedTemplateOid", Order = 13)]
        [StringLength(255)]
        public string ImpliedTemplateOid { get; set; }

        [Column("impliedTemplateTitle", Order = 14)]
        [StringLength(255)]
        public string ImpliedTemplateTitle { get; set; }

        [Column("constraintCount", Order = 15)]
        public int? ConstraintCount { get; set; }

        [Column("containedTemplateCount", Order = 16)]
        public int? ContainedTemplateCount { get; set; }

        [Column("impliedTemplateCount", Order = 17)]
        public int? ImpliedTemplateCount { get; set; }
    }
}
