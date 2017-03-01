namespace Trifolia.DB
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("v_implementationguidefile")]
    public partial class ViewImplementationGuideFile
    {
        [Key]
        [Column("id", Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        [Key]
        [Column("implementationGuideId", Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ImplementationGuideId { get; set; }

        [Key]
        [Column("fileName", Order = 2)]
        [StringLength(255)]
        public string FileName { get; set; }

        [Key]
        [Column("mimeType", Order = 3)]
        [StringLength(255)]
        public string MimeType { get; set; }

        [Key]
        [Column("contentType", Order = 4)]
        [StringLength(255)]
        public string ContentType { get; set; }

        [Column("expectedErrorCount", Order = 5)]
        public int? ExpectedErrorCount { get; set; }

        [Key]
        [Column("data", Order = 6, TypeName = "image")]
        public byte[] Data { get; set; }

        [Key]
        [Column("updatedDate", Order = 7)]
        public DateTime UpdatedDate { get; set; }

        [Key]
        [Column("updatedBy", Order = 8)]
        [StringLength(255)]
        public string UpdatedBy { get; set; }

        [Column("note", Order = 9)]
        public string Note { get; set; }
    }
}
