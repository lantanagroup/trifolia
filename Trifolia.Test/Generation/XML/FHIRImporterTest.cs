using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Trifolia.Generation.XML.FHIR.DSTU1;

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

            string bundleXml = "<?xml version=\"1.0\" encoding=\"utf-16\"?><feed xmlns=\"http://www.w3.org/2005/Atom\"><title type=\"text\">Trifolia Profile Export</title><id>cid:d1e1</id><updated>2014-05-03T10:36:24.698-07:00</updated><entry><title type=\"text\">Test Patient Resource</title><id>cid:d1e5</id><updated>2014-05-03T10:36:24.698-07:00</updated><content type=\"text/xml\"><Profile xmlns=\"http://hl7.org/fhir\"><text><status value=\"generated\" /><xhtml:div xmlns:xhtml=\"http://www.w3.org/1999/xhtml\"><xhtml:p>Test Patient Resource</xhtml:p><xhtml:ol><xhtml:li><p xmlns=\"http://www.w3.org/1999/xhtml\">MAY contain exactly one [1..1] name (CONF:8504).</p><ol xmlns=\"http://www.w3.org/1999/xhtml\"><li><p>This name SHALL contain exactly one [1..1] family (CONF:8505).</p></li></ol><ol xmlns=\"http://www.w3.org/1999/xhtml\"><li><p>This name MAY contain exactly one [1..1] given (CONF:8506).</p></li></ol></xhtml:li><xhtml:li><p xmlns=\"http://www.w3.org/1999/xhtml\">MAY contain zero or more [0..*] communication (CONF:8507).</p></xhtml:li><xhtml:li><p xmlns=\"http://www.w3.org/1999/xhtml\">SHALL contain exactly one [1..1] maritalStatus (CONF:8508).</p></xhtml:li><xhtml:li><p xmlns=\"http://www.w3.org/1999/xhtml\">SHALL contain exactly one [1..1] contact (CONF:8509).</p></xhtml:li></xhtml:ol></xhtml:div></text><identifier value=\"urn:oid:FHIR_1.2.3.41\" /><name value=\"Test Patient Resource\" /><publisher value=\"LCG\" /><status value=\"draft\" /><structure><type value=\"Patient\" /><element><path value=\"Patient.name\" /><definition><min value=\"1\" /><max value=\"1\" /><constraint><key value=\"d10e0\" /><name value=\"d10e0\" /><severity value=\"warning\" /><human value=\"MAY contain exactly one [1..1] name (CONF:8504).\" /><xpath value=\"Patient/name\" /></constraint><isModifier value=\"false\" /></definition></element><element><path value=\"Patient.name.family\" /><definition><min value=\"1\" /><max value=\"1\" /><constraint><key value=\"d13e0\" /><name value=\"d13e0\" /><severity value=\"error\" /><human value=\"This name SHALL contain exactly one [1..1] family (CONF:8505).\" /><xpath value=\"Patient/name/family\" /></constraint><isModifier value=\"false\" /></definition></element><element><path value=\"Patient.name.given\" /><definition><min value=\"1\" /><max value=\"1\" /><constraint><key value=\"d16e0\" /><name value=\"d16e0\" /><severity value=\"warning\" /><human value=\"This name MAY contain exactly one [1..1] given (CONF:8506).\" /><xpath value=\"Patient/name/given\" /></constraint><isModifier value=\"false\" /></definition></element><element><path value=\"Patient.communication\" /><definition><min value=\"0\" /><max value=\"*\" /><constraint><key value=\"d19e0\" /><name value=\"d19e0\" /><severity value=\"warning\" /><human value=\"MAY contain zero or more [0..*] communication (CONF:8507).\" /><xpath value=\"Patient/communication\" /></constraint><isModifier value=\"false\" /></definition></element><element><path value=\"Patient.maritalStatus\" /><definition><min value=\"1\" /><max value=\"1\" /><constraint><key value=\"d22e0\" /><name value=\"d22e0\" /><severity value=\"error\" /><human value=\"SHALL contain exactly one [1..1] maritalStatus (CONF:8508).\" /><xpath value=\"Patient/maritalStatus\" /></constraint><isModifier value=\"false\" /></definition></element><element><path value=\"Patient.contact\" /><definition><min value=\"1\" /><max value=\"1\" /><constraint><key value=\"d25e0\" /><name value=\"d25e0\" /><severity value=\"error\" /><human value=\"SHALL contain exactly one [1..1] contact (CONF:8509).\" /><xpath value=\"Patient/contact\" /></constraint><isModifier value=\"false\" /></definition></element></structure></Profile></content><summary type=\"xhtml\"><xhtml:div xmlns:xhtml=\"http://www.w3.org/1999/xhtml\"><xhtml:p>Test Patient Resource</xhtml:p><xhtml:ol><xhtml:li><p xmlns=\"http://www.w3.org/1999/xhtml\">MAY contain exactly one [1..1] name (CONF:8504).</p><ol xmlns=\"http://www.w3.org/1999/xhtml\"><li><p>This name SHALL contain exactly one [1..1] family (CONF:8505).</p></li></ol><ol xmlns=\"http://www.w3.org/1999/xhtml\"><li><p>This name MAY contain exactly one [1..1] given (CONF:8506).</p></li></ol></xhtml:li><xhtml:li><p xmlns=\"http://www.w3.org/1999/xhtml\">MAY contain zero or more [0..*] communication (CONF:8507).</p></xhtml:li><xhtml:li><p xmlns=\"http://www.w3.org/1999/xhtml\">SHALL contain exactly one [1..1] maritalStatus (CONF:8508).</p></xhtml:li><xhtml:li><p xmlns=\"http://www.w3.org/1999/xhtml\">SHALL contain exactly one [1..1] contact (CONF:8509).</p></xhtml:li></xhtml:ol></xhtml:div></summary></entry></feed>";
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
