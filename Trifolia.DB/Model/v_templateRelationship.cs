namespace Trifolia.DB
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("v_templateRelationship")]
    public partial class ViewTemplateRelationship
    {
        [Key, Column(Order = 0)]
        public int ParentTemplateId { get; set; }

        [Column(Order = 1)]
        public string ParentTemplateName { get; set; }

        [Column(Order = 2)]
        public string ParentTemplateIdentifier { get; set; }

        [Column(Order = 3)]
        public string ParentTemplateBookmark { get; set; }

        [Key, Column(Order = 4)]
        public int ChildTemplateId { get; set; }

        [Column(Order = 5)]
        public string ChildTemplateName { get; set; }

        [Column(Order = 6)]
        public string ChildTemplateIdentifier { get; set; }

        [Column(Order = 7)]
        public string ChildTemplateBookmark { get; set; }

        [Key, Column(Order = 8)]
        public bool Required { get; set; }
    }
}
