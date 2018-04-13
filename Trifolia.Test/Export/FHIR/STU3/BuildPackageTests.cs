using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using Ionic.Zip;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Trifolia.DB;
using Trifolia.Export.FHIR.STU3;
using Trifolia.Shared;

namespace Trifolia.Test.Export.FHIR.STU3
{
    [TestClass, DeploymentItem("Schemas\\", "Schemas\\")]
    public class BuildPackageTests
    {
        public static MockObjectRepository tdb;
        public static ImplementationGuideType igType;
        public static ImplementationGuide ig;
        public static TemplateType compositionType;
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
            "resources/structuredefinition/profile1.xml",

            "_runant.bat",
            "clean.bat",
            "RunIGPublisher.bat",
            "build.xml",
            "README.txt",
            "instance-template-base.html",
            "instance-template-format.html",
            "instance-template-sd.html",
            "Test_IG_Publisher_Build_Package.json",
            "RunIGPublisherCmd.bat" };

        [ClassInitialize]
        public static void SetupData(TestContext context)
        {
            tdb = new MockObjectRepository();
            tdb.InitializeFHIR3Repository();
            igType = tdb.ImplementationGuideTypes.Single(y => y.Name == Constants.IGType.FHIR_STU3_IG_TYPE);
            compositionType = tdb.TemplateTypes.Single(y => y.ImplementationGuideType == igType && y.Name == "Composition");

            ig = tdb.FindOrCreateImplementationGuide(igType, "Test IG Publisher Build Package");
            ig.Identifier = "http://test.com/ig";

            var compositionProfile = tdb.CreateTemplate(
                "http://test.com/ig/StructureDefinition/profile1", 
                compositionType, 
                "Test Composition 1", 
                ig, 
                "Composition", 
                "Composition", 
                "This is a test description for composition");
            compositionProfile.Bookmark = "profile1";           // This translates to the StructureDefinition.id, whic is important

            tdb.AddConstraintToTemplate(compositionProfile, null, null, "identifier", "SHALL", "1..1");

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
            string jsonFileName = "Test_IG_Publisher_Build_Package.json";

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

            // TODO: Test results of the IG Publisher?

            // TODO: Check the QA.html file generated by the IG Publisher in the output directory?
        }
    }
}
