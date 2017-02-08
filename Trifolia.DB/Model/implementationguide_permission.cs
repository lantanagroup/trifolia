namespace Trifolia.DB
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("implementationguide_permission")]
    public partial class ImplementationGuidePermission
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("implementationGuideId")]
        public int ImplementationGuideId { get; set; }

        [Column("permission")]
        [Required]
        [StringLength(50)]
        public string Permission { get; set; }

        [Column("type")]
        [Required]
        [StringLength(50)]
        public string Type { get; set; }

        [Column("groupId")]
        public int? GroupId { get; set; }

        [Column("userId")]
        public int? UserId { get; set; }

        public virtual Group Group { get; set; }

        public virtual ImplementationGuide ImplementationGuide { get; set; }

        public virtual User User { get; set; }
    }
}
