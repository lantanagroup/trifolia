namespace Trifolia.DB
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("template")]
    public partial class Template
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Template()
        {
            GreenTemplates = new HashSet<GreenTemplate>();
            ChildConstraints = new HashSet<TemplateConstraint>();
            Extensions = new HashSet<TemplateExtension>();
            ImplyingTemplates = new HashSet<Template>();
            NextVersions = new HashSet<Template>();
            TemplateSamples = new HashSet<TemplateSample>();
        }

        [Column("id")]
        public int Id { get; set; }

        [Column("implementationGuideTypeId")]
        public int ImplementationGuideTypeId { get; set; }

        [Column("templateTypeId")]
        public int TemplateTypeId { get; set; }

        [Column("impliedTemplateId")]
        public int? ImpliedTemplateId { get; set; }

        [Column("owningImplementationGuideId")]
        public int OwningImplementationGuideId { get; set; }

        [Column("oid")]
        [Required]
        [StringLength(255)]
        public string Oid { get; set; }

        [Column("isOpen")]
        public bool IsOpen { get; set; }

        [Column("name")]
        [Required]
        [StringLength(255)]
        public string Name { get; set; }

        [Column("bookmark")]
        [Required]
        [StringLength(255)]
        public string Bookmark { get; set; }

        [Column("description")]
        public string Description { get; set; }

        [Column("primaryContext")]
        [StringLength(255)]
        public string PrimaryContext { get; set; }

        [Column("notes")]
        public string Notes { get; set; }

        [Column("lastupdated", TypeName = "date")]
        public DateTime LastUpdated { get; set; }

        [Column("authorId")]
        public int AuthorId { get; set; }

        [Column("previousVersionTemplateId")]
        public int? PreviousVersionTemplateId { get; set; }

        [Column("version")]
        public int Version { get; set; }

        [Column("primaryContextType")]
        [StringLength(255)]
        public string PrimaryContextType { get; set; }

        [Column("statusId")]
        public int? StatusId { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<GreenTemplate> GreenTemplates { get; set; }

        public virtual ImplementationGuide OwningImplementationGuide { get; set; }

        public virtual ImplementationGuideType ImplementationGuideType { get; set; }

        public virtual PublishStatus Status { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TemplateConstraint> ChildConstraints { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TemplateExtension> Extensions { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Template> ImplyingTemplates { get; set; }

        public virtual Template ImpliedTemplate { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Template> NextVersions { get; set; }

        public virtual Template PreviousVersion { get; set; }

        public virtual TemplateType TemplateType { get; set; }

        public virtual User Author { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TemplateSample> TemplateSamples { get; set; }
    }
}
