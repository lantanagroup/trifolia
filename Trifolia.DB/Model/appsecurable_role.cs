namespace Trifolia.DB
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("appsecurable_role")]
    public partial class RoleAppSecurable
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("appSecurableId")]
        public int AppSecurableId { get; set; }

        [Column("roleId")]
        public int RoleId { get; set; }

        public virtual AppSecurable AppSecurable { get; set; }

        public virtual Role Role { get; set; }
    }
}
