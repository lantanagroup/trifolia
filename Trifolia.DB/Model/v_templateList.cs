namespace Trifolia.DB
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("v_templateList")]
    public partial class ViewTemplateList
    {
        [Key]
        [Column("id", Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        [Key]
        [Column("name", Order = 1)]
        [StringLength(255)]
        public string Name { get; set; }

        [Key]
        [Column("oid", Order = 2)]
        [StringLength(255)]
        public string Oid { get; set; }

        [Key]
        [Column("open", Order = 3)]
        [StringLength(3)]
        public string Open { get; set; }

        [Key]
        [Column("templateType", Order = 4)]
        [StringLength(513)]
        public string TemplateType { get; set; }

        [Key]
        [Column("templateTypeId", Order = 5)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int TemplateTypeId { get; set; }

        [Column("implementationGuide", Order = 6)]
        [StringLength(267)]
        public string ImplementationGuide { get; set; }

        [Column("implementationGuideId", Order = 7)]
        public int? ImplementationGuideId { get; set; }

        [Column("organization", Order = 8)]
        [StringLength(255)]
        public string Organization { get; set; }

        [Column("organizationId", Order = 9)]
        public int? OrganizationId { get; set; }

        [Column("publishDate", Order = 10)]
        public DateTime? PublishDate { get; set; }

        [Column("impliedTemplateName", Order = 11)]
        [StringLength(255)]
        public string ImpliedTemplateName { get; set; }

        [Column("impliedTemplateOid", Order = 12)]
        [StringLength(255)]
        public string ImpliedTemplateOid { get; set; }

        [Column("impliedTemplateId", Order = 13)]
        public int? ImpliedTemplateId { get; set; }

        [Column("description", Order = 14)]
        public string Description { get; set; }

        [Column("primaryContextType", Order = 15)]
        [StringLength(255)]
        public string PrimaryContextType { get; set; }
    }
}
