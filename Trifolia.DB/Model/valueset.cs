namespace Trifolia.DB
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("valueset")]
    public partial class ValueSet
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public ValueSet()
        {
            Constraints = new HashSet<TemplateConstraint>();
            Members = new HashSet<ValueSetMember>();
            Identifiers = new HashSet<ValueSetIdentifier>();
        }

        [Column("id")]
        public int Id { get; set; }

        [Column("name")]
        [Required]
        [StringLength(255)]
        public string Name { get; set; }

        [Column("code")]
        [StringLength(255)]
        public string Code { get; set; }

        [Column("description")]
        public string Description { get; set; }

        [Column("intensional")]
        public bool? Intensional { get; set; }

        [Column("intensionalDefinition")]
        public string IntensionalDefinition { get; set; }

        [Column("lastUpdate")]
        public DateTime? LastUpdate { get; set; }

        [Column("source")]
        [StringLength(1024)]
        public string Source { get; set; }

        /// <summary>
        /// Where the value set was imported from, if at all.
        /// </summary>
        [Column("importSource")]
        public ValueSetImportSources? ImportSource { get; set; }

        /// <summary>
        /// The id that the value is represented by on the source of the import.
        /// </summary>
        /// <remarks>For VSAC, for example, this is the "id" of the value set (aka, the "oid") without any prefixes.</remarks>
        [Column("importSourceId")]
        public string ImportSourceId { get; set; }

        [Column("isIncomplete")]
        public bool IsIncomplete { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TemplateConstraint> Constraints { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ValueSetMember> Members { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ValueSetIdentifier> Identifiers { get; set; }
    }

    public enum ValueSetImportSources
    {
        VSAC = 1,
        PHINVADS = 2,
        ROSETREE = 3
    }
}
