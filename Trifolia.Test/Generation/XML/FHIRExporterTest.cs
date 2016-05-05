using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Trifolia.Generation.XML.FHIR.DSTU1;
using Trifolia.DB;

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

            ImplementationGuide ig = tdb.FindOrAddImplementationGuide(MockObjectRepository.DEFAULT_FHIR_DSTU1_IG_TYPE_NAME, "Test FHIR IG");
            Template t = tdb.GenerateTemplate("FHIR_1.2.3.4", "Composition", "Test FHIR Composition", ig, "Composition", "Composition");

            var tc1 = tdb.GenerateConstraint(t, null, null, "subject", "SHALL", "1..1");
            var tc2 = tdb.GenerateConstraint(t, null, null, "author", "SHALL", "1..1");

            List<Template> templates = new List<Template>();
            templates.Add(t);

            FHIRExporter exporter = new FHIRExporter(tdb, templates, new Trifolia.Shared.IGSettingsManager(tdb));
            string export = exporter.GenerateExport();

            Assert.IsNotNull(export, "Expected generated export not to be null");
            Assert.AreNotEqual(0, export.Length, "Expected generated export not to be empty");
        }
    }
}
