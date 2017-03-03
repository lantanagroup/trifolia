namespace Trifolia.DB
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("v_userSecurables")]
    public partial class ViewUserSecurable
    {
        [Key]
        [Column("userId", Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int UserId { get; set; }

        [Key]
        [Column("name", Order = 1)]
        [StringLength(50)]
        public string SecurableName { get; set; }
    }
}
