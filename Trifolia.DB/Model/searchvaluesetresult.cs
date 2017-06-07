using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Core.Objects.DataClasses;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Trifolia.DB
{
    public partial class SearchValueSetResult
    {
        [Column("totalItems")]
        public int TotalItems { get; set; }
        
        [Column("id")]
        public int Id { get; set; }

        [Column("identifiers")]
        public string Identifiers { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("code")]
        public string Code { get; set; }

        [Column("description")]
        public string Description { get; set; }

        [Column("intensional")]
        public bool? Intensional { get; set; }

        [Column("intensionalDefinition")]
        public string IntensionalDefinition { get; set; }

        [Column("source")]
        public string SourceUrl { get; set; }

        [Column("isComplete")]
        public bool IsComplete { get; set; }

        [Column("hasPublishedIg")]
        public bool HasPublishedIg { get; set; }

        [Column("canEditPublishedIg")]
        public bool CanEditPublishedIg { get; set; }
        
        [Column("importSource")]
        public ValueSetImportSources? ImportSource { get; set; }

        [Column("importSourceId")]
        public string ImportSourceId { get; set; }
    }
}
