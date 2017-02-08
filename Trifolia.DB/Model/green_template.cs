namespace Trifolia.DB
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("green_template")]
    public partial class GreenTemplate
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public GreenTemplate()
        {
            ChildGreenConstraints = new HashSet<GreenConstraint>();
            ChildGreenTemplates = new HashSet<GreenTemplate>();
        }

        [Column("id")]
        public int Id { get; set; }

        [Column("parentGreenTemplateId")]
        public int? ParentGreenTemplateId { get; set; }

        [Column("templateId")]
        public int TemplateId { get; set; }

        [Column("order")]
        public int? Order { get; set; }

        [Column("name")]
        [Required]
        [StringLength(255)]
        public string Name { get; set; }

        [Column("description")]
        public string Description { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<GreenConstraint> ChildGreenConstraints { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<GreenTemplate> ChildGreenTemplates { get; set; }

        public virtual GreenTemplate ParentGreenTemplate { get; set; }

        public virtual Template Template { get; set; }
    }
}
