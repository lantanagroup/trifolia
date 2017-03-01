namespace Trifolia.DB
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("implementationguide_section")]
    public partial class ImplementationGuideSection
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("implementationGuideId")]
        public int ImplementationGuideId { get; set; }

        [Column("heading")]
        [Required]
        [StringLength(255)]
        public string Heading { get; set; }

        [Column("content", TypeName = "ntext")]
        public string Content { get; set; }

        [Column("order")]
        public int Order { get; set; }

        [Column("level")]
        public int Level { get; set; }

        public virtual ImplementationGuide ImplementationGuide { get; set; }
    }
}
