namespace Trifolia.DB
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("implementationguidetype_datatype")]
    public partial class ImplementationGuideTypeDataType
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public ImplementationGuideTypeDataType()
        {
            GreenConstraints = new HashSet<GreenConstraint>();
        }

        [Column("id")]
        public int Id { get; set; }

        [Column("implementationGuideTypeId")]
        public int ImplementationGuideTypeId { get; set; }

        [Column("dataTypeName")]
        [Required]
        [StringLength(255)]
        public string DataTypeName { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<GreenConstraint> GreenConstraints { get; set; }

        public virtual ImplementationGuideType ImplementationGuideType { get; set; }
    }
}
