namespace Trifolia.DB
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("template_constraint_reference")]
    public partial class TemplateConstraintReference : IConstraintReference
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("templateConstraintId")]
        public int TemplateConstraintId { get; set; }

        [Column("referenceIdentifier")]
        public string ReferenceIdentifier { get; set; }

        [Column("referenceType")]
        public ConstraintReferenceTypes ReferenceType { get; set; }
        public virtual TemplateConstraint Constraint { get; set; }
    }

    public enum ConstraintReferenceTypes
    {
        Template = 0
    }
}
