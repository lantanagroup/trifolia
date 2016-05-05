using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

using Trifolia.Shared;
using Trifolia.DB;

namespace Trifolia.Generation.IG
{
    public class TemplateSchematronGenerator
    {
        private const string SchNamespacePrefix = "sch";
        private const string SchNamespace = "http://www.ascc.net/xml/schematron";
        private const string Hl7NamespacePrefix = "hl7";
        private const string Hl7Namespace = "urn:hl7-org:v3";
        private const string VocNamespacePrefix = "voc";
        private const string VocNamespace = "http://www.lantanagroup.com/voc";
        private const string XsiNamespacePrefix = "xsi";
        private const string XsiNamespace = "http://www.w3.org/2001/XMLSchema-instance";
        private const string CdaNamespacePrefix = "cda";
        private const string CdaNamespace = "urn:hl7-org:v3";

        private IObjectRepository tdb;
        private ImplementationGuide implementationGuide;
        private XmlDocument schematronDoc;
        private XmlNamespaceManager nsManager;

        private XmlElement errorsPhase;
        private XmlElement warningsPhase;

        #region Initialization

        public TemplateSchematronGenerator(IObjectRepository tdb, long implementationGuideId)
        {
            this.tdb = tdb;
            this.implementationGuide = tdb.ImplementationGuides.Single(y => y.Id == implementationGuideId);

            this.Initialize();
        }

        private void Initialize()
        {
            this.schematronDoc = new XmlDocument();

            this.nsManager = new XmlNamespaceManager(schematronDoc.NameTable);

            this.nsManager.AddNamespace(SchNamespacePrefix, SchNamespace);
            this.nsManager.AddNamespace(Hl7NamespacePrefix, Hl7Namespace);
            this.nsManager.AddNamespace(VocNamespacePrefix, VocNamespace);
            this.nsManager.AddNamespace(XsiNamespacePrefix, XsiNamespace);
            this.nsManager.AddNamespace(CdaNamespacePrefix, CdaNamespace);

            this.schematronDoc.AppendChild(this.CreateSchElement("schema"));

            XmlElement nsVocEle = this.CreateSchElement("ns",
                this.CreateAttribute("prefix", VocNamespacePrefix),
                this.CreateAttribute("uri", VocNamespace));
            XmlElement nsSchEle = this.CreateSchElement("ns",
                this.CreateAttribute("prefix", SchNamespacePrefix),
                this.CreateAttribute("uri", SchNamespace));
            XmlElement nsXsiEle = this.CreateSchElement("ns",
                this.CreateAttribute("prefix", XsiNamespacePrefix),
                this.CreateAttribute("uri", XsiNamespace));
            XmlElement nsCdaEle = this.CreateSchElement("ns",           // TODO: This should be captured by IG
                this.CreateAttribute("prefix", CdaNamespacePrefix),
                this.CreateAttribute("uri", CdaNamespace));

            this.schematronDoc.DocumentElement.AppendChild(nsVocEle);
            this.schematronDoc.DocumentElement.AppendChild(nsSchEle);
            this.schematronDoc.DocumentElement.AppendChild(nsXsiEle);
            this.schematronDoc.DocumentElement.AppendChild(nsCdaEle);

            this.errorsPhase = this.CreateSchElement("phase",
                this.CreateAttribute("id", "errors"));
            this.schematronDoc.DocumentElement.AppendChild(this.errorsPhase);

            this.warningsPhase = this.CreateSchElement("phase",
                this.CreateAttribute("id", "warnings"));
            this.schematronDoc.DocumentElement.AppendChild(this.warningsPhase);
        }

        #endregion

        public void GenerateSchematron()
        {
            List<Template> templates = (from at in this.implementationGuide.ChildTemplates
                                        select at).ToList();

            foreach (Template cTemplate in templates)
            {
                AddTemplate(cTemplate);
            }
        }

        private void AddTemplate(Template template)
        {
            string schName = string.Format("p-{0}-errors", template.Oid);

            XmlElement templatePatternEle = this.CreateSchElement("pattern",
                this.CreateAttribute("id", schName),
                this.CreateAttribute("name", schName));

            string ruleContext = CreateTemplateContext(template);
            XmlElement ruleEle = this.CreateSchElement("rule",
                this.CreateAttribute("context", ruleContext));
            templatePatternEle.AppendChild(ruleEle);

            this.schematronDoc.DocumentElement.AppendChild(templatePatternEle);
        }

        private string CreateTemplateContext(Template template)
        {
            string templateContext = template.TemplateType.RootContext;

            if (string.IsNullOrEmpty(templateContext) && template.PrimaryContext != null)
                templateContext = template.PrimaryContext;

            // TODO: Add a way to define what constraints should be used to define the context of the template, rather than using the CDA-specific templateId element
            string ruleContext = string.Format("{0}:{1}[{0}:templateId/@root='{2}']",
                CdaNamespacePrefix,
                templateContext.ToLower(),
                template.Oid);

            return ruleContext;
        }

        public string GetSchematron()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                this.schematronDoc.Save(ms);

                ms.Seek(0, SeekOrigin.Begin);

                StreamReader reader = new StreamReader(ms);
                return reader.ReadToEnd();
            }
        }

        #region XML Utility Methods

        private XmlAttribute CreateAttribute(string name, string value)
        {
            XmlAttribute att = this.schematronDoc.CreateAttribute(name);
            att.Value = value;
            return att;
        }

        private XmlElement CreateSchElement(string name, params XmlAttribute[] attributes)
        {
            XmlElement ele = this.schematronDoc.CreateElement(SchNamespacePrefix, name, SchNamespace);

            if (attributes != null)
            {
                foreach (XmlAttribute cAtt in attributes)
                {
                    ele.Attributes.Append(cAtt);
                }
            }

            return ele;
        }

        #endregion
    }
}
