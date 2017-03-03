namespace Trifolia.DB
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("group_manager")]
    public partial class GroupManager
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("groupId")]
        public int GroupId { get; set; }

        [Column("userId")]
        public int UserId { get; set; }

        public virtual Group Group { get; set; }

        public virtual User User { get; set; }
    }
}
