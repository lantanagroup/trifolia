namespace Trifolia.DB
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("implementationguide_filedata")]
    public partial class ImplementationGuideFileData
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("implementationGuideFileId")]
        public int ImplementationGuideFileId { get; set; }
        
        [Column("data", TypeName = "image")]
        [Required]
        public byte[] Data { get; set; }

        [Column("updatedDate")]
        public DateTime UpdatedDate { get; set; }

        [Column("updatedBy")]
        [Required]
        [StringLength(255)]
        public string UpdatedBy { get; set; }

        [Column("note")]
        public string Note { get; set; }

        public virtual ImplementationGuideFile ImplementationGuideFile { get; set; }
    }
}
