namespace Trifolia.DB
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("organization")]
    public partial class Organization
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Organization()
        {
            ImplementationGuides = new HashSet<ImplementationGuide>();
            RoleRestrictions = new HashSet<RoleRestriction>();
        }

        [Column("id")]
        public int Id { get; set; }

        [Column("name")]
        [Required]
        [StringLength(255)]
        public string Name { get; set; }

        [Column("contactName")]
        [StringLength(128)]
        public string ContactName { get; set; }

        [Column("contactEmail")]
        [StringLength(255)]
        public string ContactEmail { get; set; }

        [Column("contactPhone")]
        [StringLength(50)]
        public string ContactPhone { get; set; }

        [Column("authProvider")]
        [StringLength(1024)]
        public string AuthProvider { get; set; }

        [Column("isInternal")]
        public bool IsInternal { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ImplementationGuide> ImplementationGuides { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<RoleRestriction> RoleRestrictions { get; set; }
    }
}
