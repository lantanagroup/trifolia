using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

using Trifolia.DB;
using Trifolia.Shared;
using Trifolia.Shared.Plugins;

namespace Trifolia.Export.DECOR
{
    public class TemplateExporter
    {
        private XmlDocument dom = new XmlDocument();
        private IEnumerable<Template> templates;
        private IObjectRepository tdb;
        private ImplementationGuide ig;
        private IGSettingsManager igSettings;
        private IIGTypePlugin igTypePlugin;
        private PublishStatus publishedStatus;
        private PublishStatus retiredStatus;

        public TemplateExporter(IEnumerable<Template> templates, IObjectRepository tdb, int implementationGuideId)
        {
            this.templates = templates;
            this.tdb = tdb;
            this.igSettings = new IGSettingsManager(tdb, implementationGuideId);
            this.ig = this.tdb.ImplementationGuides.Single(y => y.Id == implementationGuideId);
            this.igTypePlugin = this.ig.ImplementationGuideType.GetPlugin();
            this.publishedStatus = PublishStatus.GetPublishedStatus(tdb);
            this.retiredStatus = PublishStatus.GetRetiredStatus(tdb);
        }

        private project CreateProject()
        {
            project p = new project();

            p.id = this.ig.Id.ToString();
            p.prefix = this.ig.ImplementationGuideType.SchemaPrefix;
            p.defaultLanguage = "en";
            p.name = new BusinessNameWithLanguage[] { 
                new BusinessNameWithLanguage()
                {
                    Text = new string[] { this.ig.GetDisplayName(false) }
                }};

            copyright cr = new copyright();
            DateTime crDate = this.ig.PublishDate != null ? this.ig.PublishDate.Value : DateTime.Now;
            cr.years = new string[] { crDate.Year.ToString() };
            p.copyright = new copyright[] { cr };

            return p;
        }

        private TemplateDefinition CreateTemplateDefinition(Template template)
        {
            var friendlyName = template.Name.Replace("'", "_").Replace(" ", "_");
            TemplateDefinition templateDef = new TemplateDefinition();
            templateDef.context = new context();
            templateDef.context.path = template.PrimaryContext;
            templateDef.name = friendlyName.Length > 80 ? friendlyName.Substring(0, 80) : friendlyName;
            templateDef.isClosed = !template.IsOpen;
            templateDef.id = template.Oid;

            // Get the actual identifier
            if (template.IsIdentifierOID())
            {
                string oid;
                template.GetIdentifierOID(out oid);
                templateDef.id = oid;
            }
            else if (template.IsIdentifierII())
            {
                string oid, ext;
                DateTime extDate;
                template.GetIdentifierII(out oid, out ext);
                templateDef.id = oid;
                templateDef.versionLabel = ext;

                if (DateTime.TryParse(ext, out extDate))
                {
                    templateDef.effectiveDate = extDate;
                    templateDef.effectiveDateSpecified = true;
                }
            }

            // template type
            TemplateProperties property = new TemplateProperties();
            templateDef.classification = new TemplateProperties[] { property };

            if (template.PrimaryContextType.ToLower() == "clinicaldocument")
                property.type = TemplateTypes.cdaheaderlevel;
            else if (template.PrimaryContextType.ToLower() == "section")
                property.type = TemplateTypes.cdasectionlevel;
            else
                property.type = TemplateTypes.cdaentrylevel;

            // release date based on implementation guide's publish date
            if (template.OwningImplementationGuide.IsPublished())
            {
                if (template.OwningImplementationGuide.PublishDate != null)
                {
                    templateDef.officialReleaseDate = template.OwningImplementationGuide.PublishDate.Value;
                    templateDef.officialReleaseDateSpecified = true;
                }
            }

            // Status
            if (template.Status == this.publishedStatus)
                templateDef.statusCode = TemplateStatusCodeLifeCycle.active;
            else if (template.Status == this.retiredStatus)
                templateDef.statusCode = TemplateStatusCodeLifeCycle.retired;
            else
                templateDef.statusCode = TemplateStatusCodeLifeCycle.draft;

            // description
            if (!string.IsNullOrEmpty(template.Description))
            {
                XmlNode[] anyField = new XmlNode[] { dom.CreateTextNode(template.Description) };

                templateDef.desc = new FreeFormMarkupWithLanguage[] {
                    new FreeFormMarkupWithLanguage()
                    {
                        Any = anyField
                    }};
            }

            // samples/examples
            List<example> examples = new List<example>();
            foreach (var sample in template.TemplateSamples)
            {
                example templateExample = new example();
                XmlNode[] anyField = new XmlNode[] { dom.CreateTextNode(sample.XmlSample) };

                templateExample.Any = anyField;
                examples.Add(templateExample);
            }

            if (examples.Count > 0)
                templateDef.example = examples.ToArray();

            // constraints
            ConstraintExporter constraintExporter = new ConstraintExporter(dom, template, this.igSettings, this.igTypePlugin, this.tdb);
            templateDef.Items = constraintExporter.ExportConstraints();

            return templateDef;
        }

        private ValueSetConcept Convert(ValueSetMember member)
        {
            ValueSetConcept concept = new ValueSetConcept()
            {
                code = member.Code,
                codeSystemName = member.CodeSystem.Name,
                displayName = member.DisplayName,
                type = VocabType.L,
                level = "1"
            };

            CodeSystemIdentifier identifier = member.CodeSystem.GetIdentifier();

            if (identifier.Type == IdentifierTypes.Oid)
                concept.codeSystem = identifier.Identifier.Substring(8);
            else if (identifier.Type == IdentifierTypes.HL7II)
                concept.codeSystem = identifier.Identifier.Substring(10, identifier.Identifier.LastIndexOf(':') - 10);
            else
                concept.codeSystem = identifier.Identifier;

            return concept;
        }

        public decor GenerateModel()
        {
            List<TemplateDefinition> templateDefinitions = new List<TemplateDefinition>();

            foreach (var template in this.templates)
            {
                var templateDef = CreateTemplateDefinition(template);
                templateDefinitions.Add(templateDef);
            }

            var valueSets = (from t in this.templates
                             join c in this.tdb.TemplateConstraints on t.Id equals c.TemplateId
                             where c.ValueSet != null
                             select new { ValueSet = c.ValueSet, BindingDate = c.ValueSetDate })
                             .Distinct();
            List<valueSet> decorValueSets = new List<valueSet>();

            foreach (var valueSetInfo in valueSets)
            {
                var members = valueSetInfo.ValueSet.GetActiveMembers(valueSetInfo.BindingDate);

                valueSet decorValueSet = new valueSet()
                {
                    desc = new FreeFormMarkupWithLanguage[] {
                        new FreeFormMarkupWithLanguage()
                        {
                            Any = new XmlNode[] { dom.CreateTextNode(valueSetInfo.ValueSet.Description) }
                        }},
                    name = valueSetInfo.ValueSet.Name,
                    displayName = valueSetInfo.ValueSet.Name,
                    effectiveDate = valueSetInfo.BindingDate != null ? valueSetInfo.BindingDate.Value : DateTime.MinValue,
                    effectiveDateSpecified = valueSetInfo.BindingDate != null
                };

                decorValueSet.conceptList = new ValueSetConceptList();
                decorValueSet.conceptList.Items = (from m in members
                                                   select Convert(m)).ToArray<object>();
                decorValueSets.Add(decorValueSet);
            }


            decor model = new decor();
            model.project = CreateProject();

            model.rules = templateDefinitions.ToArray<object>();
            model.terminology = new terminology();
            model.terminology.valueSet = decorValueSets.ToArray();

            return model;
        }

        public string GenerateXML()
        {
            decor model = this.GenerateModel();
            XmlSerializer serializer = new XmlSerializer(model.GetType());

            using (StringWriter sw = new StringWriter())
            {
                serializer.Serialize(sw, model);
                return sw.ToString();
            }
        }
    }
}
