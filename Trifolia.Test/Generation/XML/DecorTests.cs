using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Trifolia.DB;
using Trifolia.Shared;
using Trifolia.Export.DECOR;

namespace Trifolia.Test.Shared
{
    [TestClass]
    public class DecorTests
    {
        [TestMethod]
        public void GenerateModelTest()
        {
        }

        [TestMethod]
        public void GenerateXMLTest()
        {
            MockObjectRepository mockTdb = new MockObjectRepository();
            mockTdb.InitializeCDARepository();

            ImplementationGuide ig = mockTdb.FindOrCreateImplementationGuide("CDA", "Test IG", null, DateTime.Now);
            Template template = mockTdb.CreateTemplate("urn:oid:2.16.22.22.11", "Document", "Test Doc Type", ig, "ClinicalDocument", "ClinicalDocument", "Test Description");
            var tca1 = mockTdb.AddConstraintToTemplate(template, null, null, "@classCode", "SHALL", "1..1", value: "test1");
            var tca2 = mockTdb.AddConstraintToTemplate(template, null, null, "@moodCode", "SHALL", "1..1", value: "test2");
            var tc1 = mockTdb.AddConstraintToTemplate(template, null, null, "entryRelationship", "SHALL", "1..1");
            var tc2 = mockTdb.AddConstraintToTemplate(template, tc1, null, "observation", "SHOULD", "0..1");
            var tc3 = mockTdb.AddConstraintToTemplate(template, tc2, null, "value", "SHALL", "1..1", "CD", value: "4321", displayName: "Test");

            template.TemplateSamples = new System.Data.Entity.Core.Objects.DataClasses.EntityCollection<TemplateSample>();
            template.TemplateSamples.Add(new TemplateSample()
            {
                XmlSample = "<test><example>VALUE</example></test>"
            });

            List<Template> templates = new List<Template>();
            templates.Add(template);

            IGSettingsManager igSettings = new IGSettingsManager(mockTdb, ig.Id);
            TemplateExporter exporter = new TemplateExporter(templates, mockTdb, ig.Id);
            string export = exporter.GenerateXML();

            Assert.IsFalse(string.IsNullOrEmpty(export));
        }
    }
}
