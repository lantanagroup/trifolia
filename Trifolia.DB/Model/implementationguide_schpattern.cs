namespace Trifolia.DB
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("implementationguide_schpattern")]
    public partial class ImplementationGuideSchematronPattern
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("implementationGuideId")]
        public int ImplementationGuideId { get; set; }

        [Column("phase")]
        [Required]
        [StringLength(128)]
        public string Phase { get; set; }

        [Column("patternId")]
        [Required]
        [StringLength(255)]
        public string PatternId { get; set; }

        [Column("patternContent")]
        [Required]
        public string PatternContent { get; set; }

        public virtual ImplementationGuide ImplementationGuide { get; set; }
    }
}
