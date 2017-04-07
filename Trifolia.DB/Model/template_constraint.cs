namespace Trifolia.DB
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("template_constraint")]
    public partial class TemplateConstraint
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TemplateConstraint()
        {
            GreenConstraints = new HashSet<GreenConstraint>();
            Samples = new HashSet<TemplateConstraintSample>();
            ChildConstraints = new HashSet<TemplateConstraint>();
            References = new HashSet<TemplateConstraintReference>();
        }

        [Column("id")]
        public int Id { get; set; }

        [Column("number")]
        public int? Number { get; set; }

        [Column("templateId")]
        public int TemplateId { get; set; }

        [Column("parentConstraintId")]
        public int? ParentConstraintId { get; set; }

        [Column("codeSystemId")]
        public int? CodeSystemId { get; set; }

        [Column("valueSetId")]
        public int? ValueSetId { get; set; }

        [Column("order")]
        public int Order { get; set; }

        [Column("isBranch")]
        public bool IsBranch { get; set; }

        [Column("isPrimitive")]
        public bool IsPrimitive { get; set; }

        [Column("conformance")]
        [StringLength(128)]
        public string Conformance { get; set; }

        [Column("cardinality")]
        [StringLength(50)]
        public string Cardinality { get; set; }

        [Column("context")]
        [StringLength(255)]
        public string Context { get; set; }

        [Column("dataType")]
        [StringLength(255)]
        public string DataType { get; set; }

        [Column("valueConformance")]
        [StringLength(50)]
        public string ValueConformance { get; set; }

        [Column("isStatic")]
        public bool? IsStatic { get; set; }

        [Column("value")]
        [StringLength(255)]
        public string Value { get; set; }

        [Column("displayName")]
        [StringLength(255)]
        public string DisplayName { get; set; }

        [Column("valueSetDate")]
        public DateTime? ValueSetDate { get; set; }

        [Column("schematron")]
        public string Schematron { get; set; }

        [Column("description")]
        public string Description { get; set; }

        [Column("notes")]
        public string Notes { get; set; }

        [Column("primitiveText")]
        public string PrimitiveText { get; set; }

        [Column("isInheritable")]
        public bool IsInheritable { get; set; }
        
        [Column("label", TypeName = "ntext")]
        public string Label { get; set; }

        [Column("isBranchIdentifier")]
        public bool IsBranchIdentifier { get; set; }

        [Column("isSchRooted")]
        public bool IsSchRooted { get; set; }

        [Column("isHeading")]
        public bool IsHeading { get; set; }

        [Column("headingDescription", TypeName = "ntext")]
        public string HeadingDescription { get; set; }

        [Column("category")]
        [StringLength(255)]
        public string Category { get; set; }

        [Column("displayNumber")]
        [StringLength(128)]
        public string DisplayNumber { get; set; }

        [Column("isModifier")]
        public bool IsModifier { get; set; }

        [Column("mustSupport")]
        public bool MustSupport { get; set; }

        [Column("isChoice")]
        public bool IsChoice { get; set; }

        [Column("isFixed")]
        public bool IsFixed { get; set; }

        public virtual CodeSystem CodeSystem { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<GreenConstraint> GreenConstraints { get; set; }

        public virtual Template Template { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TemplateConstraintSample> Samples { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TemplateConstraintReference> References { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TemplateConstraint> ChildConstraints { get; set; }

        public virtual TemplateConstraint ParentConstraint { get; set; }

        public virtual ValueSet ValueSet { get; set; }
    }
}
