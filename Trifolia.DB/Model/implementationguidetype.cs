namespace Trifolia.DB
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("implementationguidetype")]
    public partial class ImplementationGuideType
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public ImplementationGuideType()
        {
            ImplementationGuides = new HashSet<ImplementationGuide>();
            DataTypes = new HashSet<ImplementationGuideTypeDataType>();
            Templates = new HashSet<Template>();
            TemplateTypes = new HashSet<TemplateType>();
        }

        [Column("id")]
        public int Id { get; set; }

        [Column("name")]
        [Required]
        [StringLength(255)]
        public string Name { get; set; }

        [Column("schemaLocation")]
        [Required]
        [StringLength(255)]
        public string SchemaLocation { get; set; }

        [Column("schemaPrefix")]
        [Required]
        [StringLength(255)]
        public string SchemaPrefix { get; set; }

        [Column("schemaURI")]
        [Required]
        [StringLength(255)]
        public string SchemaURI { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ImplementationGuide> ImplementationGuides { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ImplementationGuideTypeDataType> DataTypes { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Template> Templates { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TemplateType> TemplateTypes { get; set; }
    }
}
