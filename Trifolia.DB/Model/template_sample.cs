namespace Trifolia.DB
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("template_sample")]
    public partial class TemplateSample
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("templateId")]
        public int TemplateId { get; set; }

        [Column("lastUpdated", TypeName = "date")]
        public DateTime LastUpdated { get; set; }

        [Column("xmlSample", TypeName = "text")]
        [Required]
        public string XmlSample { get; set; }

        [Column("name")]
        [Required]
        [StringLength(255)]
        public string Name { get; set; }

        public virtual Template Template { get; set; }
    }
}
