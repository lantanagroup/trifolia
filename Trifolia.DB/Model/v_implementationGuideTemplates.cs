namespace Trifolia.DB
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("v_implementationGuideTemplates")]
    public partial class ViewImplementationGuideTemplate
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Column("implementationGuideId", Order = 0)]
        public int ImplementationGuideId { get; set; }

        [Column("templateId", Order = 1)]
        public int? TemplateId { get; set; }
    }
}
