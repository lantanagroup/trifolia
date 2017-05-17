namespace Trifolia.DB
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("v_implementationGuideCodeSystems")]
    public partial class ViewImplementationGuideCodeSystem
    {
        [Key]
        [Column("implementationGuideId", Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ImplementationGuideId { get; set; }

        [Key]
        [Column("codeSystemId", Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int CodeSystemId { get; set; }

        [Column("codeSystemName", Order = 2)]
        public string Name { get; set; }

        [Column("codeSystemIdentifier", Order = 3)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Identifier { get; set; }

        [Column("codeSystemDescription", Order = 4)]
        public string Description { get; set; }
    }
}
