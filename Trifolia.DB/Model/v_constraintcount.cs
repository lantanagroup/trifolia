namespace Trifolia.DB
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("v_constraintcount")]
    public partial class ViewConstraintCount
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Column("templateId", Order = 0)]
        public int TemplateId { get; set; }

        [Column("total", Order = 1)]
        public int? Total { get; set; }
    }
}
