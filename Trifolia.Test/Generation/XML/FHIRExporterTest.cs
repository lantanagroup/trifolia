using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Trifolia.DB;
using Trifolia.Export.FHIR.DSTU1;

namespace Trifolia.Test.Generation.XML
{
    [TestClass]
    public class FHIRExporterTest
    {
        [TestMethod]
        [DeploymentItem("Schemas\\", "Schemas\\")]
        public void ExportFHIRComposition()
        {
            MockObjectRepository tdb = new MockObjectRepository();
            tdb.InitializeFHIRRepository();

            ImplementationGuide ig = tdb.FindOrCreateImplementationGuide(MockObjectRepository.DEFAULT_FHIR_DSTU1_IG_TYPE_NAME, "Test FHIR IG");
            Template t = tdb.CreateTemplate("FHIR_1.2.3.4", "Composition", "Test FHIR Composition", ig, "Composition", "Composition");

            var tc1 = tdb.AddConstraintToTemplate(t, null, null, "subject", "SHALL", "1..1");
            var tc2 = tdb.AddConstraintToTemplate(t, null, null, "author", "SHALL", "1..1");

            List<Template> templates = new List<Template>();
            templates.Add(t);

            string export = FHIRExporter.GenerateExport(tdb, templates, new Trifolia.Shared.IGSettingsManager(tdb));

            Assert.IsNotNull(export, "Expected generated export not to be null");
            Assert.AreNotEqual(0, export.Length, "Expected generated export not to be empty");
        }
    }
}
