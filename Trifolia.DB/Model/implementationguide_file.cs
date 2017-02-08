namespace Trifolia.DB
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("implementationguide_file")]
    public partial class ImplementationGuideFile
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public ImplementationGuideFile()
        {
            Versions = new HashSet<ImplementationGuideFileData>();
        }

        [Column("id")]
        public int Id { get; set; }

        [Column("implementationGuideId")]
        public int ImplementationGuideId { get; set; }

        [Column("fileName")]
        [Required]
        [StringLength(255)]
        public string FileName { get; set; }

        [Column("mimeType")]
        [Required]
        [StringLength(255)]
        public string MimeType { get; set; }

        [Column("contentType")]
        [Required]
        [StringLength(255)]
        public string ContentType { get; set; }

        [Column("expectedErrorCount")]
        public int? ExpectedErrorCount { get; set; }

        [Column("description", TypeName = "ntext")]
        [Required]
        public string Description { get; set; }

        [Column("url")]
        [StringLength(255)]
        public string Url { get; set; }

        public virtual ImplementationGuide ImplementationGuide { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ImplementationGuideFileData> Versions { get; set; }
    }
}
