namespace Trifolia.DB
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("templatetype")]
    public partial class TemplateType
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TemplateType()
        {
            ImplementationGuideTemplateTypes = new HashSet<ImplementationGuideTemplateType>();
            Templates = new HashSet<Template>();
        }

        [Column("id")]
        public int Id { get; set; }

        [Column("implementationGuideTypeId")]
        public int ImplementationGuideTypeId { get; set; }

        [Column("name")]
        [Required]
        [StringLength(255)]
        public string Name { get; set; }

        [Column("outputOrder")]
        public int OutputOrder { get; set; }

        [Column("rootContext")]
        [StringLength(255)]
        public string RootContext { get; set; }

        [Column("rootContextType")]
        [StringLength(255)]
        public string RootContextType { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ImplementationGuideTemplateType> ImplementationGuideTemplateTypes { get; set; }

        public virtual ImplementationGuideType ImplementationGuideType { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Template> Templates { get; set; }
    }
}
