using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trifolia.Export.FHIR.STU3.Models
{
    public class Control
    {
        public Control(string version)
        {
            this.Tool = "jekyll";
            this.License = "CC0-1.0";
            this.Version = version;
            this.SctEdition = "http://snomed.info/sct/731000124108";
            this.Paths = new Path();

            this.Defaults = new Dictionary<string, TemplateReference>();
            this.Defaults.Add("Any", new TemplateReference("instance-template-format.html", "instance-template-base.html"));
            this.Defaults.Add("StructureDefinition", new TemplateReference(null, "instance-template-sd.html"));

            this.Resources = new Dictionary<string, ResourceReference>();
            this.SpecialUrls = new List<string>();
            this.DependencyList = new List<Dependency>();
        }

        [JsonProperty(PropertyName = "tool", NullValueHandling = NullValueHandling.Ignore)]
        public string Tool { get; set; }

        [JsonProperty(PropertyName = "paths", NullValueHandling = NullValueHandling.Ignore)]
        public Path Paths { get; set; }

        [JsonProperty(PropertyName = "defaults", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, TemplateReference> Defaults { get; set; }

        [JsonProperty(PropertyName = "canonicalBase", NullValueHandling = NullValueHandling.Ignore)]
        public string CanonicalBase { get; set; }

        [JsonProperty(PropertyName = "dependencyList", NullValueHandling = NullValueHandling.Ignore)]
        public List<Dependency> DependencyList { get; set; }

        [JsonProperty(PropertyName = "source", NullValueHandling = NullValueHandling.Ignore)]
        public string Source { get; set; }

        [JsonProperty(PropertyName = "npm-name", NullValueHandling = NullValueHandling.Ignore)]
        public string NpmName { get; set; }

        [JsonProperty(PropertyName = "version", NullValueHandling = NullValueHandling.Ignore)]
        public string Version { get; set; }

        [JsonProperty(PropertyName = "license", NullValueHandling = NullValueHandling.Ignore)]
        public string License { get; set; }

        [JsonProperty(PropertyName = "sct-edition", NullValueHandling = NullValueHandling.Ignore)]
        public string SctEdition { get; set; }

        [JsonProperty(PropertyName = "special-urls", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> SpecialUrls { get; set; }

        [JsonProperty(PropertyName = "resources", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, ResourceReference> Resources { get; set; }

        public class Dependency
        {
            [JsonProperty(PropertyName = "name", NullValueHandling = NullValueHandling.Ignore)]
            public string Name { get; set; }

            [JsonProperty(PropertyName = "location", NullValueHandling = NullValueHandling.Ignore)]
            public string Location { get; set; }

            [JsonProperty(PropertyName = "source", NullValueHandling = NullValueHandling.Ignore)]
            public string Source { get; set; }
        }

        public class Path
        {
            public Path()
            {
                this.Resources = new List<string> { "resources" };
                this.Pages = "pages";
                this.Temp = "temp";
                this.Output = "output";
                this.QA = "qa";
                this.Specification = "http://hl7-fhir.github.io/";
            }

            [JsonProperty(PropertyName = "resources", NullValueHandling = NullValueHandling.Ignore)]
            public List<string> Resources { get; set; }

            [JsonProperty(PropertyName = "pages", NullValueHandling = NullValueHandling.Ignore)]
            public string Pages { get; set; }

            [JsonProperty(PropertyName = "temp", NullValueHandling = NullValueHandling.Ignore)]
            public string Temp { get; set; }

            [JsonProperty(PropertyName = "output", NullValueHandling = NullValueHandling.Ignore)]
            public string Output { get; set; }

            [JsonProperty(PropertyName = "qa", NullValueHandling = NullValueHandling.Ignore)]
            public string QA { get; set; }

            [JsonProperty(PropertyName = "specification", NullValueHandling = NullValueHandling.Ignore)]
            public string Specification { get; set; }
        }

        public class ResourceReference
        {
            public ResourceReference() { }

            public ResourceReference(string reference_base)
            {
                this.ReferenceBase = reference_base;
            }

            public ResourceReference(string template_base, string reference_base)
            {
                this.TemplateBase = template_base;
                this.ReferenceBase = reference_base;
            }

            [JsonProperty(PropertyName = "template-base", NullValueHandling = NullValueHandling.Ignore)]
            public string TemplateBase { get; set; }

            [JsonProperty(PropertyName = "base", NullValueHandling = NullValueHandling.Ignore)]
            public string ReferenceBase { get; set; }
        }

        public class TemplateReference
        {
            public TemplateReference() { }

            public TemplateReference(string template_format, string template_base)
            {
                this.TemplateFormat = template_format;
                this.TemplateBase = template_base;
            }

            [JsonProperty(PropertyName = "template-defns", NullValueHandling = NullValueHandling.Ignore)]
            public string TemplateDefinitions { get; set; }

            [JsonProperty(PropertyName = "template-format", NullValueHandling = NullValueHandling.Ignore)]
            public string TemplateFormat { get; set; }

            [JsonProperty(PropertyName = "template-base", NullValueHandling = NullValueHandling.Ignore)]
            public string TemplateBase { get; set; }
        }
    }
}
