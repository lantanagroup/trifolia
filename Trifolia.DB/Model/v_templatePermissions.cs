namespace Trifolia.DB
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("v_templatePermissions")]
    public partial class ViewTemplatePermission
    {
        [Key]
        [Column("userId", Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int UserId { get; set; }

        [Key]
        [Column("templateId", Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int TemplateId { get; set; }

        [Key]
        [Column("permission", Order = 2)]
        [StringLength(50)]
        public string Permission { get; set; }
    }
}
