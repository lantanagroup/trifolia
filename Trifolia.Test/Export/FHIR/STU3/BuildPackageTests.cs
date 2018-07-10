using Ionic.Zip;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml;
using Trifolia.Shared;
using Trifolia.DB;
using Trifolia.Export.FHIR.STU3;

namespace Trifolia.Test.Export.FHIR.STU3
{
    [TestClass, DeploymentItem("Schemas\\", "Schemas\\")]
    public class BuildPackageTests
    {
        public static MockObjectRepository tdb;
        public static ImplementationGuideType igType;
        public static ImplementationGuide ig;
        public static TemplateType compositionType;
        public static TemplateType extensionType;
        public static string extractDirectory;
        private const string dstu2ExtractFolder = "dstu2_export_build_package\\";
        private const string igPublisherJarFileName = "org.hl7.fhir.igpublisher.jar";
        private const string igPublisherUrl = "http://hl7.org/fhir/STU3/" + igPublisherJarFileName;      // Not used here, because it is included in the SLN/project
        private string[] expectedFiles = new string[] {
            "pages/_includes/footer.html",
            "pages/_includes/header.html",
            "pages/_includes/navbar.html",
            "pages/assets/css/bootstrap-fhir.css",
            "pages/assets/css/bootstrap-glyphicons.css",
            "pages/assets/css/project.css",
            "pages/assets/css/pygments-manni.css",
            "pages/assets/css/xml.css",
            "pages/assets/fonts/glyphiconshalflings-regular.eot",
            "pages/assets/fonts/glyphiconshalflings-regular.otf",
            "pages/assets/fonts/glyphiconshalflings-regular.svg",
            "pages/assets/fonts/glyphiconshalflings-regular.ttf",
            "pages/assets/fonts/glyphiconshalflings-regular.woff",
            "pages/assets/ico/apple-touch-icon-114-precomposed.png",
            "pages/assets/ico/apple-touch-icon-144-precomposed.png",
            "pages/assets/ico/apple-touch-icon-57-precomposed.png",
            "pages/assets/ico/apple-touch-icon-72-precomposed.png",
            "pages/assets/ico/favicon.ico",
            "pages/assets/ico/favicon.png",
            "pages/assets/images/fhir-logo.png",
            "pages/assets/images/fhir-logo-www.png",
            "pages/assets/images/hl7-logo.png",
            "pages/assets/images/logo_ansinew.jpg",
            "pages/assets/images/search.png",
            "pages/assets/images/stripe.png",
            "pages/assets/images/target.png",
            "pages/assets/js/fhir.js",
            "pages/assets/js/html5shiv.js",
            "pages/assets/js/jquery.js",
            "pages/assets/js/jquery-1.11.1.min.map",
            "pages/assets/js/respond.min.js",
            "pages/assets/js/xml.js",
            "pages/dist/css/bootstrap.css",
            "pages/dist/js/bootstrap.js",
            "pages/dist/js/bootstrap.min.js",
            "pages/index.html",
            "pages/jquery-3.2.1.js",
            "pages/jquery-3.2.1.min.js",
            "pages/jquery-ui.css",
            "pages/jquery-ui.js",
            "pages/jquery-ui.min.css",
            "pages/jquery-ui.min.js",
            "pages/jquery-ui.structure.css",
            "pages/jquery-ui.structure.min.css",
            "pages/jquery-ui.theme.css",
            "pages/jquery-ui.theme.min.css",
            "pages/assets/images/help16.png",
            "pages/_includes/overview.html",
            "pages/_includes/resources.html",
            "pages/_includes/authors.html",
            "pages/_includes/description.html",
            "pages/_includes/codesystems.html",
            "pages/_includes/valuesets.html",
            "pages/_includes/extensions.html",
            "pages/_data/examples.json",

            "resources/implementationguide/2.xml",
            "resources/structuredefinition/CCDA-on-FHIR-US-Realm-Header.xml",
            "resources/structuredefinition/CCDA-on-FHIR-Participant.xml",
            "resources/structuredefinition/CCDA-on-FHIR-Authorization.xml",
            "resources/structuredefinition/us-core-organization.xml",
            "resources/structuredefinition/us-core-patient.xml",
            "resources/structuredefinition/us-core-practitioner.xml",

            "_runant.bat",
            "clean.bat",
            "RunIGPublisher.bat",
            "build.xml",
            "README.txt",
            "instance-template-base.html",
            "instance-template-format.html",
            "instance-template-sd.html",
            "STU3_IG_Publisher_Build_Package.json",
            "RunIGPublisherCmd.bat" };

        [ClassInitialize]
        public static void SetupData(TestContext context)
        {
            tdb = new MockObjectRepository();
            tdb.InitializeFHIR3Repository();
            igType = tdb.ImplementationGuideTypes.Single(y => y.Name == Constants.IGTypeNames.FHIR_STU3);
            compositionType = tdb.TemplateTypes.Single(y => y.ImplementationGuideType == igType && y.Name == "Composition");
            extensionType = tdb.TemplateTypes.Single(y => y.ImplementationGuideType == igType && y.Name == "Extension");

            ig = tdb.FindOrCreateImplementationGuide(igType, "STU3 IG Publisher Build Package");
            ig.Identifier = "http://test.com/ig";
            ig.WebDescription = "This Implementation Guide (IG) defines a series of FHIR profiles on the Composition resource to represent the various document types in C - CDA.This release does not directly map every C - CDA template to FHIR profiles, rather tries to accomplish the C - CDA use case using Composition resource profiles created under this project(the equivalent of Level 2 CDA documents), and begins by linking to the profiles created under the US Core project for any coded entries that would normally be included in C - CDA sections.To have a simpler, more streamlined standard that reuses existing work and focuses on the 80 % that implementers actually need in production systems, the resources of US Core represents a portion of the 80 % needed for coded entries for coded entries of CCD, Care Plan and Discharge Summary).";
            ig.WebReadmeOverview = "We encourage feedback on these Composition profiles, and the general approach to the project as a whole. We also encourage implementers who wish to see more of the coded data from C - CDA mapped to FHIR to comment on the US Core project and make their requests known there.  Once US Core creates new profiles, this project can reference them.";

            tdb.CreateImplementationGuideSection(ig, "Test Heading", "Test Content", 1, level: 3);

            var CCDAonFHIRParticipantProfile = tdb.CreateTemplate(
                "http://test.com/ig/StructureDefinition/CCDA-on-FHIR-Participant",
                 extensionType,
                "C-CDAonFHIRParticipant",
                ig,
                "Extension",
                "Extension",
                "C-CDA on FHIR Participant Extension");
                 CCDAonFHIRParticipantProfile.Bookmark = "CCDA-on-FHIR-Participant";

            var CCDAonFHIRAuthorizationProfile = tdb.CreateTemplate(
                "http://test.com/ig/StructureDefinition/CCDA-on-FHIR-Authorization",
                 extensionType,
                "C-CDAonFHIRAuthorization",
                ig,
                "Extension",
                "Extension",
                "C-CDA on FHIR Authorization Extension");
                CCDAonFHIRAuthorizationProfile.Bookmark = "CCDA-on-FHIR-Authorization";

           var USCorePatientProfile = tdb.CreateTemplate(
             "http://test.com/ig/StructureDefinition/us-core-patient",
              compositionType,
              "U.S.CorePatient",
              ig,
              "Composition",
              "Composition",
              "Defines basic constraints and extensions on the Patient resource for use with other US Core resources.");
              USCorePatientProfile.Bookmark = "us-core-patient";

              USCorePatientProfile.Author = new User();
              USCorePatientProfile.Author.FirstName = "Sarah";
              USCorePatientProfile.Author.LastName = "Gaunt";
              USCorePatientProfile.Author.Email = "sarah.gaunt@lantanagroup.com";

            var USCorePractitionerProfile = tdb.CreateTemplate(
            "http://test.com/ig/StructureDefinition/us-core-practitioner",
             compositionType,
             "U.S.CorePractitioner",
             ig,
             "Composition",
             "Composition",
             "Defines basic constraints and extensions on the Practitioner resource for use with other US Core resources.");
            USCorePractitionerProfile.Bookmark = "us-core-practitioner";

            USCorePractitionerProfile.Author = new User();
            USCorePractitionerProfile.Author.FirstName = "Eric";
            USCorePractitionerProfile.Author.LastName = "Parapini";
            USCorePractitionerProfile.Author.Email = "eric.parapini@lantanagroup.com";

            var USCoreOrganizationProfile = tdb.CreateTemplate(
              "http://test.com/ig/StructureDefinition/us-core-organization",
              compositionType,
              "U.S.CoreOrganization",
              ig,
              "Composition",
              "Composition",
              "Defines basic constraints and extensions on the Organization resource for use with other US Core resources.");
               USCoreOrganizationProfile.Bookmark = "us-core-organization";

               USCoreOrganizationProfile.Author = new User();
               USCoreOrganizationProfile.Author.FirstName = "Meenaxi";
               USCoreOrganizationProfile.Author.LastName = "Gosai";
               USCoreOrganizationProfile.Author.Email = "meenaxi.gosai@lantanagroup.com";

            var USRealmHeaderProfile = tdb.CreateTemplate(
                "http://test.com/ig/StructureDefinition/CCDA-on-FHIR-US-Realm-Header", 
                compositionType, 
                "US.RealmHeader", 
                ig, 
                "Composition", 
                "Composition",
                "This profile defines constraints that represent common administrative and demographic concepts for US Realm clinical documents. Further specification, such as type, are provided in document profiles that conform to this profile.");
                 USRealmHeaderProfile.Bookmark = "CCDA-on-FHIR-US-Realm-Header";           // This translates to the StructureDefinition.id, which is important

                USRealmHeaderProfile.Author = new User();
                USRealmHeaderProfile.Author.FirstName = "Sean";
                USRealmHeaderProfile.Author.LastName = "McIlvenna";
                USRealmHeaderProfile.Author.Email = "sean.mcilvenna@lantanagroup.com";

                // Add basic constraints to the  profile
                tdb.AddConstraintToTemplate(USRealmHeaderProfile, null, null, "identifier", "SHALL", "1..1");
                tdb.AddConstraintToTemplate(USRealmHeaderProfile, null, null, "date", "SHALL", "1..1");
                tdb.AddConstraintToTemplate(USRealmHeaderProfile, null, null, "type", "SHALL", "1..1");
                tdb.AddConstraintToTemplate(USRealmHeaderProfile, null, null, "title", "SHALL", "1..1");
                tdb.AddConstraintToTemplate(USRealmHeaderProfile, null, null, "status", "SHALL", "1..1");
                tdb.AddConstraintToTemplate(USRealmHeaderProfile, null, null, "language", "SHALL", "1..1");


            // Add a reference constraint to a contained profile
            TemplateConstraint t3tc1 = tdb.AddConstraintToTemplate(USRealmHeaderProfile, null, USCorePatientProfile, "subject", "SHALL", "1..1");

            // Add a reference constraint to a contained profile
            TemplateConstraint t3tc2 = tdb.AddConstraintToTemplate(USRealmHeaderProfile, null, USCorePractitionerProfile, "author", "SHALL", "1..1");

            // Add a reference constraint to a contained profile
            TemplateConstraint t3tc3 = tdb.AddConstraintToTemplate(USRealmHeaderProfile, null, USCoreOrganizationProfile, "custodian", "SHALL", "1..1");

            // Constraint with a child (child has code system)
            CodeSystem t1tc4_cs = tdb.FindOrCreateCodeSystem("FHIR CompositionAttestationMode", "http://hl7.org/fhir/composition-attestation-mode");
            TemplateConstraint t1tc4 = tdb.AddConstraintToTemplate(USRealmHeaderProfile, null, null, "attester", "SHOULD", "0..1");
            TemplateConstraint t1tc4_2 = tdb.AddConstraintToTemplate(USRealmHeaderProfile, t1tc4, null, "mode", "SHALL", "1..1", "CE", "SHALL", "legal", null, codeSystem: t1tc4_cs);

            // Add an extension constraint with a contained profile
            TemplateConstraint t5tc1 = tdb.AddConstraintToTemplate(USRealmHeaderProfile, null, CCDAonFHIRAuthorizationProfile, "extension", "SHALL", "1..1", "Extension");
            
            // Add a Constraint with a child (child has valueset)
            ValueSet t1tc6_vs = tdb.FindOrCreateValueSet("v3-ConfidentialityClassification", "http://hl7.org/fhir/ValueSet/v3-ConfidentialityClassification");
            TemplateConstraint t1tc6 = tdb.AddConstraintToTemplate(USRealmHeaderProfile, null, null, "confidentiality", "SHALL", "1..1", "CE", "SHALL", null, null, valueSet: t1tc6_vs);          

            extractDirectory = Path.Combine(context.TestDeploymentDir, dstu2ExtractFolder);
        }

        private byte[] GenerateExport()
        {
            var exporter = new BuildExporter(tdb, ig.Id);
            byte[] exported = exporter.Export(includeVocabulary: true);     // Exports as a ZIP
            return exported;
        }

        /// <summary>
        /// Tests that the build package export produces a correctly-formatted ZIP file
        /// </summary>
        [TestMethod]
        [TestCategory("FHIR")]
        public void STU3_ExportBuildPackageTest()
        {
            var exported = GenerateExport();

            using (MemoryStream ms = new MemoryStream(exported))
            {
                using (var zip = ZipFile.Read(ms))
                {
                    var jsonConfigEntry = zip.Entries.Single(y => y.FileName == "pages/_data/examples.json");

                    using (StreamReader reader = new StreamReader(jsonConfigEntry.OpenReader()))
                    {
                        string contents = reader.ReadToEnd();
                        Assert.AreEqual("{}", contents);
                        Console.WriteLine(contents);
                    }

                    foreach (var expectedFileName in expectedFiles)
                    {
                        var found = zip.Entries.SingleOrDefault(y => y.FileName == expectedFileName);
                        Assert.IsNotNull(found, "Expected to find " + expectedFileName + " in zip package, but did not");
                    }

                    foreach (var entry in zip.Entries)
                    {
                        var found = expectedFiles.Contains(entry.FileName);
                        Assert.IsTrue(found, "Found unexpected file " + entry.FileName + " in zip package");
                    }
                }
            }
        }

        /// <summary>
        /// Tests that the ZIP file executes against the IG Publisher.
        /// This test should be excluded from most automated runs of the unit tests, since it can take a very long time to run.
        /// </summary>
        [TestMethod]
        [TestCategory("FHIR_E2E")]         // This is used to ignore this test in the automated run on AppVeyor
        [DeploymentItem(@"Export\FHIR\STU3\" + igPublisherJarFileName, dstu2ExtractFolder)]
        public void STU3_IGPublisherTest()
        {
            var exported = GenerateExport();
            string jsonFileName = "STU3_IG_Publisher_Build_Package.json";

            using (MemoryStream ms = new MemoryStream(exported))
            {
                using (var zip = ZipFile.Read(ms))
                {
                    var foundJson = zip.Entries.SingleOrDefault(y => y.FileName == jsonFileName);
                    Assert.IsNotNull(foundJson, "Did not find JSON config file in ZIP package. IG publisher won't run.");

                    // Extract the ZIP to the test directory so we can run the IG Publisher against it
                    zip.ExtractAll(extractDirectory);
                    Console.WriteLine("IG Build Publisher export extracted to: " + extractDirectory);
                }
            }

            // Run the IG Publisher
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.FileName = "java";
            startInfo.Arguments = "-jar " + igPublisherJarFileName + " -ig " + jsonFileName;
            startInfo.WorkingDirectory = extractDirectory;
            startInfo.RedirectStandardError = true;
            startInfo.RedirectStandardOutput = true;
            startInfo.UseShellExecute = false;

            Process process = new Process();
            process.StartInfo = startInfo;
            process.OutputDataReceived += (sender, args) =>
            {
                Console.WriteLine("INFO: " + args.Data);
            };
            process.ErrorDataReceived += (sender, args) =>
            {
                Console.WriteLine("ERROR: " + args.Data);
            };
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();
            process.CancelOutputRead();

            if (process.ExitCode != 0)
                Assert.Fail("The IG Publisher exited with code " + process.ExitCode);

            // TODO: Test results of the IG Publisher?
            string outputDirectory = Path.Combine(extractDirectory, "output");          // ...\TestResults\XXXX\Out\dstu2_export_build_package\output
            Assert.IsTrue(File.Exists(Path.Combine(outputDirectory, "StructureDefinition-CCDA-on-FHIR-Authorization.html")), "Expected to find StructureDefinition-CCDA-on-FHIR-Authorization.html");
            // TODO: Check that all formats are created in separate files (ex: .json.html, .json, .xml.html, .xml)

            // TODO: Check the QA.html file generated by the IG Publisher in the output directory?
            try
            {
                string qaFile = Path.Combine(outputDirectory, "qa.html");
                XmlDocument qaDoc = new XmlDocument();
                qaDoc.Load(qaFile);

                XmlNamespaceManager nsManager = new XmlNamespaceManager(qaDoc.NameTable);
                nsManager.AddNamespace("fhir", "http://hl7.org/fhir");

                var errorNodes = qaDoc.SelectNodes("/fhir:Bundle/fhir:entry/fhir:OperationOutcome/fhir:issue[fhir:severity/@value='error']", nsManager);
                Assert.AreEqual(0, errorNodes.Count, "Expected 0 errors, but got " + errorNodes.Count);
            }
            catch (Exception ex)
            {
                Assert.Fail("Error parsing QA XML file: " + ex.Message);
            }
        }
    }
}
