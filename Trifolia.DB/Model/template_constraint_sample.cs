namespace Trifolia.DB
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("template_constraint_sample")]
    public partial class TemplateConstraintSample
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("templateConstraintId")]
        public int TemplateConstraintId { get; set; }

        [Column("name")]
        [Required]
        [StringLength(255)]
        public string Name { get; set; }

        [Column("sampleText", TypeName = "ntext")]
        [Required]
        public string SampleText { get; set; }

        public virtual TemplateConstraint Constraint { get; set; }
    }
}
