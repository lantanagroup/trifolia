namespace Trifolia.DB
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("v_latestimplementationguidefiledata")]
    public partial class ViewLatestImplementationGuideFileData
    {
        [Key]
        [Column("id", Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        [Key]
        [Column("implementationGuideFileId", Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ImplementationGuideFileId { get; set; }

        [Key]
        [Column("data", Order = 2, TypeName = "image")]
        public byte[] Data { get; set; }

        [Key]
        [Column("updatedDate", Order = 3)]
        public DateTime UpdatedDate { get; set; }

        [Key]
        [Column("updatedBy", Order = 4)]
        [StringLength(255)]
        public string UpdatedBy { get; set; }

        [Column("note", Order = 5)]
        public string Note { get; set; }
    }
}
