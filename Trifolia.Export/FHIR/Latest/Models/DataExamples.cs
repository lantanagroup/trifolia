using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trifolia.Export.FHIR.Latest.Models
{
    public class DataExamples
    {
        public DataExamples()
        {
            this.StructureDefinitions = new Dictionary<string, StructureDefinition>();
        }

        /// <summary>
        /// Key = id of StructureDefinition
        /// Value = list of examples (their ids) for each structure definition
        /// </summary>
        public Dictionary<string, StructureDefinition> StructureDefinitions { get; set; }

        public class StructureDefinition
        {
            public StructureDefinition()
            {
                this.Examples = new List<Example>();
            }

            [JsonProperty(PropertyName = "examples")]
            public List<Example> Examples { get; set; }
        }

        public class Example
        {
            [JsonProperty(PropertyName = "type")]
            public string Type { get; set; }

            [JsonProperty(PropertyName = "id")]
            public string Id { get; set; }
        }
    }
}
