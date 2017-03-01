namespace Trifolia.DB
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("template_extension")]
    public partial class TemplateExtension
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("templateId")]
        public int TemplateId { get; set; }

        [Column("identifier")]
        [Required]
        [StringLength(255)]
        public string Identifier { get; set; }

        [Column("type")]
        [Required]
        [StringLength(55)]
        public string Type { get; set; }

        [Column("value")]
        [Required]
        [StringLength(255)]
        public string Value { get; set; }

        public virtual Template Template { get; set; }
    }
}
