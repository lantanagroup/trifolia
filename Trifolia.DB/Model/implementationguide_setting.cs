namespace Trifolia.DB
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("implementationguide_setting")]
    public partial class ImplementationGuideSetting
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("implementationGuideId")]
        public int ImplementationGuideId { get; set; }

        [Column("propertyName")]
        [Required]
        [StringLength(255)]
        public string PropertyName { get; set; }

        [Column("propertyValue")]
        [Required]
        public string PropertyValue { get; set; }

        public virtual ImplementationGuide ImplementationGuide { get; set; }
    }
}
