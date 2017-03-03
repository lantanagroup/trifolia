namespace Trifolia.DB
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("v_templateusage")]
    public partial class ViewTemplateUsage
    {
        [Key]
        [Column("templateId", Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int TemplateId { get; set; }

        [Key]
        [Column("templateDisplay", Order = 1)]
        [StringLength(513)]
        public string TemplateDisplay { get; set; }

        [Key]
        [Column("implementationGuideId", Order = 2)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ImplementationGuideId { get; set; }

        [Key]
        [Column("implementationGuideDisplay", Order = 3)]
        [StringLength(513)]
        public string ImplementationGuideDisplay { get; set; }
    }
}
