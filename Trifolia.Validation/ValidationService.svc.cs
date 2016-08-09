using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Xml.Schema;
using System.IO;
using System.Reflection;
using System.Web.Hosting;

using LantanaGroup.ValidationUtility.Model;
using LantanaGroup.Schematron.ISO;
using LantanaGroup.Schematron;

using Trifolia.Shared;
using Trifolia.Generation.Schematron;
using Trifolia.DB;
using LantanaGroup.ValidationUtility;

namespace Trifolia.ValidationService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "ValidationService" in code, svc and config file together.
    [ServiceBehavior(InstanceContextMode=InstanceContextMode.Single, ConcurrencyMode=ConcurrencyMode.Multiple)]
    public class ValidationService : IValidationService
    {
        private static List<ValidationProfile> loadedProfiles = new List<ValidationProfile>();
        private CustomXmlResolver xmlResolver = new CustomXmlResolver();
        private object loadedProfilesLock = new object();

        /// <summary>
        /// Validates each of the contents and returns the validation results
        /// </summary>
        /// <param name="profile">The profile to validate the content against</param>
        /// <param name="contents">The content to validate against the profile</param>
        /// <returns></returns>
        public List<ValidationResult> ValidateDocument(int profileId, List<ValidationContent> contents)
        {
            List<ValidationResult> results = new List<ValidationResult>();

            using (IObjectRepository tdb = DBContext.Create())
            {
                // Get the requested profile
                ValidationProfile profile = null;

                try
                {
                   profile = GetValidationProfile(tdb, profileId);
                }
                catch (Exception ex)
                {
                    results.Add(new ValidationResult()
                    {
                        ErrorMessage = "Preparing validation package failed with the following error: " + ex.Message,
                        Severity = "Critical"
                    });

                    return results;
                }

                foreach (ValidationContent cContent in contents)
                {
                    try
                    {
                        // Validate the document with the profile
                        results.AddRange(ValidateInputXml(cContent, profile));
                    }
                    catch (Exception ex)
                    {
                        results.Add(new ValidationResult()
                        {
                            ErrorMessage = "Validating document resulted in a critical system error: " + ex.Message,
                            FileName = cContent.FileName,
                            Severity = "Critical"
                        });
                    }
                }
            }

            return results;
        }

        public List<ValidationProfile> GetValidationProfiles()
        {
            using (IObjectRepository tdb = DBContext.Create())
            {
                return GetValidationProfiles(tdb);
            }
        }

        public List<ValidationProfile> GetValidationProfiles(IObjectRepository tdb)
        {
            List<ValidationProfile> existingProfiles = (from igf in tdb.ImplementationGuideFiles
                                                        where igf.ContentType == ImplementationGuideFile.ContentTypeSchematron &&
                                                          igf.ImplementationGuide.PublishDate != null
                                                        select new ValidationProfile()
                                                        {
                                                            Id = igf.Id,
                                                            Name = igf.ImplementationGuide.Name + " (" + igf.FileName + ")",
                                                            Type = igf.ImplementationGuide.ImplementationGuideType.Name,
                                                            SchemaLocation = igf.ImplementationGuide.ImplementationGuideType.SchemaLocation,
                                                            SchemaPrefix = igf.ImplementationGuide.ImplementationGuideType.SchemaPrefix
                                                        }).ToList();

            return existingProfiles;
        }

        #region Validation Package

        public List<ValidationImplementationGuide> GetValidationImplementationGuides()
        {
            using (IObjectRepository tdb = DBContext.Create())
            {
                return (from ig in tdb.ImplementationGuides
                        select new ValidationImplementationGuide()
                        {
                            ImplementationGuideId = ig.Id,
                            Name = ig.Name
                        }).ToList();
            }
        }

        public List<ValidationDocument> GetValidationPackage(IObjectRepository tdb, int implementationGuideId, GenerationOptions options, DateTime? lastRetrieveDate)
        {
            List<ValidationDocument> documents = GetValidationPackageHelperFiles(tdb, implementationGuideId);

            if (options == GenerationOptions.RetrieveExisting)
            {
                ValidationDocument existingValidationDoc = GetStoredValidationDocument(tdb, implementationGuideId, null);

                if (existingValidationDoc == null)
                    return null;

                documents.Add(existingValidationDoc);
            }
            else if (options == GenerationOptions.RegenerateChanges)
            {
                ValidationDocument existingValidationDoc = GetStoredValidationDocument(tdb, implementationGuideId, lastRetrieveDate);

                if (existingValidationDoc == null)
                {
                    ValidationDocument generatedValidationDoc = GetGeneratedValidationDocument(tdb, implementationGuideId);
                    documents.Add(generatedValidationDoc);
                }
            }
            else if (options == GenerationOptions.Generate)
            {
                ValidationDocument generatedValidationDoc = GetGeneratedValidationDocument(tdb, implementationGuideId);
                documents.Add(generatedValidationDoc);
            }

            return documents;
        }

        public List<ValidationDocument> GetValidationPackage(int implementationGuideId, GenerationOptions options, DateTime? lastRetrieveDate)
        {
            using (IObjectRepository tdb = DBContext.Create())
            {
                return GetValidationPackage(tdb, implementationGuideId, options, lastRetrieveDate);
            }
        }

        private ValidationDocument GetStoredValidationDocument(IObjectRepository tdb, int implementationGuideId, DateTime? lastRetrieveDate)
        {
            ViewImplementationGuideFile igFile = tdb.ViewImplementationGuideFiles.FirstOrDefault(y => y.ImplementationGuideId == implementationGuideId && y.ContentType == ImplementationGuideFile.ContentTypeSchematron);

            if (igFile == null)
                return null;

            if (lastRetrieveDate != null)
            {
                var igConstraintIds = (from tc in tdb.TemplateConstraints
                                       join t in tdb.Templates on tc.TemplateId equals t.Id
                                       where t.OwningImplementationGuideId == implementationGuideId
                                       select tc.Id);
                var newerConstraints = (from audit in tdb.AuditEntries
                                        join tcId in igConstraintIds on audit.TemplateConstraintId equals tcId
                                        where audit.AuditDate > lastRetrieveDate
                                        select tcId);

                if (newerConstraints.Count() > 0)
                    return null;
            }

            ValidationDocument schDoc = new ValidationDocument()
            {
                Name = igFile.FileName,
                Content = igFile.Data,
                ContentType = igFile.ContentType,
                MimeType = igFile.MimeType
            };

            return schDoc;
        }

        private ValidationDocument GetGeneratedValidationDocument(IObjectRepository tdb, int implementationGuideId)
        {
            ImplementationGuide ig = tdb.ImplementationGuides.Single(y => y.Id == implementationGuideId);
            ImplementationGuideFile vocFile = tdb.ImplementationGuideFiles.FirstOrDefault(y => y.ContentType == ImplementationGuideFile.ContentTypeVocabulary && y.ImplementationGuideId == ig.Id);
            string vocFilename = "voc.xml";

            if (vocFile != null)
                vocFilename = vocFile.FileName;

            List<Template> templates = ig.GetRecursiveTemplates(tdb);

            SchematronGenerator schGenerator = new SchematronGenerator(tdb, ig, templates, true, vocFileName:vocFilename);
            string schContent = schGenerator.Generate();
            string schName = Trifolia.Shared.Helper.NormalizeName(ig.Name) + ".sch";

            ValidationDocument schDoc = new ValidationDocument()
            {
                Content = ASCIIEncoding.UTF8.GetBytes(schContent),
                ContentType = ImplementationGuideFile.ContentTypeSchematron,
                MimeType = "text/xml",
                Name = schName
            };

            return schDoc;
        }

        private List<ValidationDocument> GetValidationPackageHelperFiles(IObjectRepository tdb, int implementationGuideId)
        {
            List<ValidationDocument> documents = new List<ValidationDocument>();

            // Get the list of sch helper files, and vocabulary files that we need to return
            var helperFiles = (from vigf in tdb.ViewImplementationGuideFiles
                               where vigf.ImplementationGuideId == implementationGuideId &&
                               (vigf.ContentType == ImplementationGuideFile.ContentTypeSchematronHelper || vigf.ContentType == ImplementationGuideFile.ContentTypeVocabulary)
                               select vigf);

            foreach (ViewImplementationGuideFile currentFile in helperFiles)
            {
                ValidationDocument newValidationDoc = new ValidationDocument()
                {
                    Name = currentFile.FileName,
                    ContentType = currentFile.ContentType,
                    MimeType = currentFile.MimeType,
                    Content = currentFile.Data
                };

                documents.Add(newValidationDoc);
            }

            return documents;
        }

        #endregion

        #region Private Validation Functionality

        private List<ValidationResult> ValidateInputXml(ValidationContent content, ValidationProfile profile)
        {
            string dataContent = System.Text.ASCIIEncoding.UTF8.GetString(content.Data);
            List<ValidationResult> results = new List<ValidationResult>();

            using (StringReader ms = new StringReader(dataContent))
            {
                XmlReader sourceReader = XmlReader.Create(ms);
                XmlDocument sourceDoc = new XmlDocument();

                try
                {
                    sourceDoc.LoadXml(dataContent);
                }
                catch (XmlException ex)
                {
                    results.Add(new ValidationResult()
                    {
                        FileName = content.FileName,
                        ErrorMessage = "Could not load XML document: " + ex.Message
                    });
                    return results;
                }

                XmlNamespaceManager nsManager = new XmlNamespaceManager(sourceDoc.NameTable);

                results.AddRange(
                    ValidateSchema(content.FileName, profile, content.Data, nsManager));

                results.AddRange(
                    ValidateSchematron(profile.SchematronValidator, content));
            }

            return results;
        }

        private List<ValidationResult> ValidateSchema(string fileName, ValidationProfile profile, byte[] sourceData, XmlNamespaceManager nsManager)
        {
            Dictionary<string, string> namespaces = null;
            XmlSchemaCollection schemas = new XmlSchemaCollection();
            schemas.Add(Helper.GetSchema(profile.SchemaLocation, out namespaces));

            XmlParserContext context = new XmlParserContext(null, nsManager, null, XmlSpace.None);

            using (MemoryStream sourceStream = new MemoryStream(sourceData))
            {
                XmlValidatingReader validatingReader = new XmlValidatingReader(sourceStream, XmlNodeType.Document, context);
                validatingReader.ValidationType = ValidationType.Schema;
                validatingReader.Schemas.Add(schemas);

                List<ValidationResult> results = new List<ValidationResult>();
                ValidationResult newResult = null;

                validatingReader.ValidationEventHandler += new ValidationEventHandler(delegate(object sender, ValidationEventArgs e)
                    {
                        newResult = new ValidationResult()
                        {
                            FileName = fileName,
                            ErrorMessage = e.Message,
                            Severity = e.Severity.ToString(),
                            TestContext = "Schema",
                            Test = "N/A",
                            Location = "N/A",
                            LineNumber = e.Exception.LineNumber
                        };

                        results.Add(newResult);
                    });

                while (validatingReader.Read())
                {
                    if (newResult != null)
                    {
                        newResult.Test = validatingReader.Name;
                        results.Add(newResult);
                        newResult = null;
                    }
                }

                return results;
            }
        }

        private List<ValidationResult> ValidateSchematron(IValidator validator, ValidationContent content)
        {
            List<ValidationResult> results = new List<ValidationResult>();

            try
            {
                string contentText = ASCIIEncoding.UTF8.GetString(content.Data);
                ValidationResults validationResults = validator.Validate(contentText);

                results = (from schvr in validationResults.Messages
                           select new ValidationResult()
                           {
                               ErrorMessage = schvr.Message,
                               FileName = content.FileName,
                               LineNumber = schvr.LineNumber,
                               Test = schvr.Test,
                               TestContext = schvr.Context,
                               Severity = schvr.Severity.ToString(),
                               XmlChunk = schvr.ContextContent
                           }).ToList();
            }
            catch { }

            return results;
        }

        #endregion

        #region Profile Retrieval

        private ValidationProfile GetValidationProfile(IObjectRepository tdb, int profileId)
        {
            if (profileId >= 0)
                return GetStoredValidationProfile(tdb, profileId);

            return GetGeneratedValidationProfile(tdb, profileId * -1);
        }

        private ValidationProfile GetGeneratedValidationProfile(IObjectRepository tdb, int implementationGuideId)
        {
            return null;
        }

        private ValidationProfile GetStoredValidationProfile(IObjectRepository tdb, int implementationGuideFileId)
        {
            ValidationProfile foundProfile = loadedProfiles.SingleOrDefault(y => y.Id == implementationGuideFileId);
            ImplementationGuideFileData latestSchematron = (from igf in tdb.ImplementationGuideFiles
                                                            join igfd in tdb.ImplementationGuideFileDatas on igf.Id equals igfd.ImplementationGuideFileId
                                                            where igf.Id == implementationGuideFileId
                                                            select igfd).OrderByDescending(y => y.UpdatedDate).FirstOrDefault();
            ImplementationGuideFile igFile = tdb.ImplementationGuideFiles.Single(y => y.Id == implementationGuideFileId);
            int implementationGuideId = igFile.ImplementationGuideId;

            if (foundProfile != null && latestSchematron != null && foundProfile.LastUpdated != latestSchematron.UpdatedDate)
            {
                lock (loadedProfilesLock)
                {
                    loadedProfiles.Remove(foundProfile);
                }

                foundProfile = null;
            }

            // The profile is not already loaded
            if (foundProfile == null)
            {
                lock (loadedProfilesLock)
                {
                    string type = igFile.ImplementationGuide.ImplementationGuideType.Name;
                    string schemaLocation = Path.Combine(
                        Trifolia.Shared.Helper.GetSchemasDirectory(igFile.ImplementationGuide.ImplementationGuideType.Name),
                        igFile.ImplementationGuide.ImplementationGuideType.SchemaLocation);

                    foundProfile = new ValidationProfile()
                    {
                        Id = implementationGuideFileId
                    };

                    List<ImplementationGuideFile> schHelperFiles = (from pigf in tdb.ImplementationGuideFiles
                                                                    join igf in tdb.ImplementationGuideFiles on pigf.ImplementationGuideId equals igf.ImplementationGuideId
                                                                    where pigf.ImplementationGuideId == implementationGuideId && igf.ContentType == ImplementationGuideFile.ContentTypeSchematronHelper
                                                                    select igf).ToList();

                    IValidator schValidator = SchematronValidationFactory.NewValidator(ASCIIEncoding.UTF8.GetString(latestSchematron.Data));

                    List<ImplementationGuideFile> additionalFiles = (from pigf in tdb.ImplementationGuideFiles
                                                                     join igf in tdb.ImplementationGuideFiles on pigf.ImplementationGuideId equals igf.ImplementationGuideId
                                                                     where pigf.Id == implementationGuideFileId && igf.ContentType == ImplementationGuideFile.ContentTypeVocabulary
                                                                     select igf).ToList();

                    foreach (ImplementationGuideFile cAdditionalFile in additionalFiles)
                    {
                        ImplementationGuideFileData latestVersion = cAdditionalFile.Versions
                            .OrderByDescending(y => y.UpdatedDate)
                            .FirstOrDefault();

                        schValidator.AddInclude(cAdditionalFile.FileName, latestVersion.Data);
                    }

                    foundProfile.SchematronValidator = schValidator;
                    foundProfile.LastUpdated = latestSchematron.UpdatedDate;
                    foundProfile.Type = type;
                    foundProfile.SchemaPrefix = igFile.ImplementationGuide.ImplementationGuideType.SchemaPrefix;
                    foundProfile.SchemaUri = igFile.ImplementationGuide.ImplementationGuideType.SchemaURI;
                    foundProfile.SchemaLocation = schemaLocation;

                    loadedProfiles.Add(foundProfile);
                }
            }

            return foundProfile;
        }

        #endregion

        #region Utility

        private static CustomXmlResolver CreateSchematronResolver(List<ImplementationGuideFile> schHelperFiles)
        {
            CustomXmlResolver resolver = new CustomXmlResolver();

            resolver.AddStoredFile("skeleton1-5.xsl", GetResource("Trifolia.ValidationService.Schematron.skeleton1-5.xsl"));
            resolver.AddStoredFile("RNG2Schtrn.xsl", GetResource("Trifolia.ValidationService.Schematron.RNG2Schtrn.xsl"));
            resolver.AddStoredFile("XSD2Schtrn.xsl", GetResource("Trifolia.ValidationService.Schematron.XSD2Schtrn.xsl"));
            resolver.AddStoredFile("iso_schematron_skeleton.xsl", GetResource("Trifolia.ValidationService.Schematron.iso_schematron_skeleton.xsl"));

            // Add schematron helper files to resolver
            schHelperFiles.ForEach(y => resolver.AddStoredFile(y.FileName, y.GetLatestData().Data));

            return resolver;
        }

        private static byte[] GetResource(string resource)
        {
            Stream resourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resource);
            byte[] data = new byte[resourceStream.Length];
            resourceStream.Read(data, 0, data.Length);
            return data;
        }

        #endregion
    }
}
