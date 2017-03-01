namespace Trifolia.DB
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("green_constraint")]
    public partial class GreenConstraint
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public GreenConstraint()
        {
            ChildGreenConstraints = new HashSet<GreenConstraint>();
        }

        [Column("id")]
        public int Id { get; set; }

        [Column("greenTemplateId")]
        public int GreenTemplateId { get; set; }

        [Column("templateConstraintId")]
        public int TemplateConstraintId { get; set; }

        [Column("parentGreenConstraintId")]
        public int? ParentGreenConstraintId { get; set; }

        [Column("order")]
        public int? Order { get; set; }

        [Column("name")]
        [Required]
        [StringLength(255)]
        public string Name { get; set; }

        [Column("description")]
        public string Description { get; set; }

        [Column("isEditable")]
        public bool IsEditable { get; set; }

        [Column("rootXpath")]
        [StringLength(250)]
        public string RootXpath { get; set; }

        [Column("igtype_datatypeId")]
        public int? ImplementationGuideTypeDataTypeId { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<GreenConstraint> ChildGreenConstraints { get; set; }

        public virtual GreenConstraint ParentGreenConstraint { get; set; }

        public virtual GreenTemplate GreenTemplate { get; set; }

        public virtual ImplementationGuideTypeDataType ImplementationGuideTypeDataType { get; set; }

        public virtual TemplateConstraint TemplateConstraint { get; set; }
    }
}
