namespace Trifolia.DB
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("user_role")]
    public partial class UserRole
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("roleId")]
        public int RoleId { get; set; }

        [Column("userId")]
        public int UserId { get; set; }
        
        public virtual Role Role { get; set; }

        public virtual User User { get; set; }
    }
}
