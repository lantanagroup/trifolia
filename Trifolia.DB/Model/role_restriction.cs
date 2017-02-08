namespace Trifolia.DB
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("role_restriction")]
    public partial class RoleRestriction
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("roleId")]
        public int RoleId { get; set; }

        [Column("organizationId")]
        public int OrganizationId { get; set; }

        public virtual Organization Organization { get; set; }

        public virtual Role Role { get; set; }
    }
}
