namespace Trifolia.DB
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("implementationguide")]
    public partial class ImplementationGuide
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public ImplementationGuide()
        {
            NextVersions = new HashSet<ImplementationGuide>();
            Files = new HashSet<ImplementationGuideFile>();
            Permissions = new HashSet<ImplementationGuidePermission>();
            SchematronPatterns = new HashSet<ImplementationGuideSchematronPattern>();
            Sections = new HashSet<ImplementationGuideSection>();
            Settings = new HashSet<ImplementationGuideSetting>();
            TemplateTypes = new HashSet<ImplementationGuideTemplateType>();
            ChildTemplates = new HashSet<Template>();
        }

        [Column("id")]
        public int Id { get; set; }

        [Column("")]
        public int ImplementationGuideTypeId { get; set; }

        [Column("organizationId")]
        public int? OrganizationId { get; set; }

        [Column("name")]
        [Required]
        [StringLength(255)]
        public string Name { get; set; }

        [Column("publishDate")]
        public DateTime? PublishDate { get; set; }

        [Column("publishStatusId")]
        public int? PublishStatusId { get; set; }

        [Column("previousVersionImplementationGuideId")]
        public int? PreviousVersionImplementationGuideId { get; set; }

        [Column("version")]
        public int? Version { get; set; }

        [Column("displayName")]
        [StringLength(255)]
        public string DisplayName { get; set; }

        [Column("accessManagerId")]
        public int? AccessManagerId { get; set; }

        [Column("allowAccessRequests")]
        public bool AllowAccessRequests { get; set; }

        [Column("webDisplayName")]
        [StringLength(255)]
        public string WebDisplayName { get; set; }

        [Column("webDescription")]
        public string WebDescription { get; set; }

        [Column("webReadmeOverview")]
        public string WebReadmeOverview { get; set; }

        [Column("identifier")]
        [StringLength(255)]
        public string Identifier { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ImplementationGuide> NextVersions { get; set; }

        public virtual ImplementationGuide PreviousVersion { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ImplementationGuideFile> Files { get; set; }

        public virtual ImplementationGuideType ImplementationGuideType { get; set; }

        public virtual Organization Organization { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ImplementationGuidePermission> Permissions { get; set; }

        public virtual PublishStatus PublishStatus { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ImplementationGuideSchematronPattern> SchematronPatterns { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ImplementationGuideSection> Sections { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ImplementationGuideSetting> Settings { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ImplementationGuideTemplateType> TemplateTypes { get; set; }

        public virtual User AccessManager { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Template> ChildTemplates { get; set; }
    }
}
