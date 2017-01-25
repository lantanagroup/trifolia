using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Trifolia.Import.FHIR.DSTU1;

namespace Trifolia.Test.Generation.XML
{
    [TestClass]
    public class FHIRImporterTest
    {
        [TestMethod]
        public void TestImportBundle()
        {
            MockObjectRepository tdb = new MockObjectRepository();
            tdb.InitializeFHIRRepository();

            tdb.FindOrCreateImplementationGuide(MockObjectRepository.DEFAULT_FHIR_DSTU1_IG_TYPE_NAME, "Unowned FHIR DSTU1 Profiles");

            string bundleXml = Helper.GetSampleContents("Trifolia.Test.DocSamples.FHIR.DSTU1.bundle.xml");
            FHIRImporter importer = new FHIRImporter(tdb, false);
            importer.Import(bundleXml);

            try
            {
                importer.Import(bundleXml);
                Assert.Fail("Expected an exception to be thrown when importing FHIR profiles with 'create' option, when profile already exists");
            }
            catch { }

            importer = new FHIRImporter(tdb, true);
            importer.Import(bundleXml);
        }
    }
}
