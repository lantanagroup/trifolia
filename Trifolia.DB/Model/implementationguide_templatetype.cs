namespace Trifolia.DB
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("implementationguide_templatetype")]
    public partial class ImplementationGuideTemplateType
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("implementationGuideId")]
        public int ImplementationGuideId { get; set; }

        [Column("templateTypeId")]
        public int TemplateTypeId { get; set; }

        [Column("name")]
        [Required]
        [StringLength(255)]
        public string Name { get; set; }

        [Column("detailsText")]
        public string DetailsText { get; set; }

        public virtual ImplementationGuide ImplementationGuide { get; set; }

        public virtual TemplateType TemplateType { get; set; }
    }
}
