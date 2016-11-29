using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Trifolia.DB;
using Trifolia.Shared;
using ExportImplementationGuide = Trifolia.Shared.ImportExport.Model.TrifoliaImplementationGuide;
using ExportImplementationGuideCategory = Trifolia.Shared.ImportExport.Model.TrifoliaImplementationGuideCategory;
using ExportImplementationGuideSection = Trifolia.Shared.ImportExport.Model.TrifoliaImplementationGuideVolume1Section;
using ExportImplementationGuideFile = Trifolia.Shared.ImportExport.Model.TrifoliaImplementationGuideFile;
using ExportImplementationGuideFileType = Trifolia.Shared.ImportExport.Model.TrifoliaImplementationGuideFileType;

namespace Trifolia.Export.Native
{
    public static class ImplementationGuideExtension
    {
        public static Shared.ImportExport.Model.ImplementationGuideStatus GetExportStatus(this ImplementationGuide ig)
        {
            if (ig.PublishStatus == null)
                return Shared.ImportExport.Model.ImplementationGuideStatus.Draft;

            switch (ig.PublishStatus.Status)
            {
                case "Ballot":
                    return Shared.ImportExport.Model.ImplementationGuideStatus.Ballot;
                case "Published":
                    return Shared.ImportExport.Model.ImplementationGuideStatus.Published;
                case "Deprecated":
                    return Shared.ImportExport.Model.ImplementationGuideStatus.Deprecated;
                case "Retired":
                    return Shared.ImportExport.Model.ImplementationGuideStatus.Retired;
                default:
                    return Shared.ImportExport.Model.ImplementationGuideStatus.Draft;
            }
        }

        public static ExportImplementationGuide Export(this ImplementationGuide ig, IObjectRepository tdb, IGSettingsManager igSettings)
        {
            ExportImplementationGuide exportIg = new ExportImplementationGuide()
            {
                name = ig.Name,
                type = ig.ImplementationGuideType.Name,
                version = ig.Version != null ? ig.Version.Value : 1,
                status = ig.GetExportStatus(),
                displayName = ig.DisplayName,
                webDisplayName = ig.WebDisplayName,
                organizationName = ig.Organization != null ? ig.Organization.Name : string.Empty,
                WebDescription = ig.WebDescription,
                WebReadmeOverview = ig.WebReadmeOverview,
                Volume1 = null,
                PreviousVersion = null      // Needed so that the generated model doesn't produce an empty <PreviousVersion> element when serialized
            };

            if (ig.PreviousVersionImplementationGuideId != null)
            {
                var previousVersionIg = tdb.ImplementationGuides.Single(y => y.Id == ig.PreviousVersionImplementationGuideId);
                exportIg.PreviousVersion = new Shared.ImportExport.Model.TrifoliaImplementationGuidePreviousVersion()
                {
                    name = previousVersionIg.Name,
                    number = previousVersionIg.Version != null ? previousVersionIg.Version.Value : 1
                };
            }

            exportIg.CardinalityDisplay.Add(new Shared.ImportExport.Model.TrifoliaImplementationGuideCardinalityDisplay()
            {
                cardinality = Shared.ImportExport.Model.CardinalityTypes.Zero,
                display = igSettings.GetSetting(IGSettingsManager.SettingProperty.CardinalityZero)
            });
            exportIg.CardinalityDisplay.Add(new Shared.ImportExport.Model.TrifoliaImplementationGuideCardinalityDisplay()
            {
                cardinality = Shared.ImportExport.Model.CardinalityTypes.ZeroOrOne,
                display = igSettings.GetSetting(IGSettingsManager.SettingProperty.CardinalityZeroToOne)
            });
            exportIg.CardinalityDisplay.Add(new Shared.ImportExport.Model.TrifoliaImplementationGuideCardinalityDisplay()
            {
                cardinality = Shared.ImportExport.Model.CardinalityTypes.ZeroToMany,
                display = igSettings.GetSetting(IGSettingsManager.SettingProperty.CardinalityZeroOrMore)
            });
            exportIg.CardinalityDisplay.Add(new Shared.ImportExport.Model.TrifoliaImplementationGuideCardinalityDisplay()
            {
                cardinality = Shared.ImportExport.Model.CardinalityTypes.One,
                display = igSettings.GetSetting(IGSettingsManager.SettingProperty.CardinalityOneToOne)
            });
            exportIg.CardinalityDisplay.Add(new Shared.ImportExport.Model.TrifoliaImplementationGuideCardinalityDisplay()
            {
                cardinality = Shared.ImportExport.Model.CardinalityTypes.OneToMany,
                display = igSettings.GetSetting(IGSettingsManager.SettingProperty.CardinalityAtLeastOne)
            });

            exportIg.CustomSchematron = (from p in ig.SchematronPatterns
                                         select new Shared.ImportExport.Model.TrifoliaImplementationGuideCustomSchematron()
                                         {
                                             phase = p.Phase,
                                             patternId = p.PatternId,
                                             Rule = p.PatternContent                                             
                                         }).ToList();

            exportIg.CustomTemplateType = (from t in ig.TemplateTypes
                                           select new Shared.ImportExport.Model.TrifoliaImplementationGuideCustomTemplateType()
                                           {
                                               templateTypeName = t.TemplateType.Name,
                                               CustomName = t.Name,
                                               Description = t.DetailsText
                                           }).ToList();

            var volume1Html = igSettings.GetSetting(IGSettingsManager.SettingProperty.Volume1Html);

            if (!string.IsNullOrEmpty(volume1Html) || ig.Sections.Count > 0)
                exportIg.Volume1 = new Shared.ImportExport.Model.TrifoliaImplementationGuideVolume1();

            if (!string.IsNullOrEmpty(volume1Html))
                exportIg.Volume1.Items.Add(volume1Html);

            if (ig.Sections.Count > 0)
            {
                exportIg.Volume1.Items = (from s in ig.Sections.OrderBy(y => y.Order)
                                          select new ExportImplementationGuideSection()
                                          {
                                              Heading = new Shared.ImportExport.Model.TrifoliaImplementationGuideVolume1SectionHeading()
                                              {
                                                  Title = s.Heading,
                                                  Level = s.Level
                                              },
                                              Content = s.Content
                                          }).ToList<object>();
            }

            var categoriesString = igSettings.GetSetting(IGSettingsManager.SettingProperty.Categories);
            var categories = categoriesString.Split(',');

            exportIg.Category = (from c in categories
                                 select new ExportImplementationGuideCategory()
                                 {
                                     name = c
                                 }).ToList();

            foreach (var igFile in ig.Files)
            {
                var latestData = igFile.GetLatestData();

                ExportImplementationGuideFile exportIgFile = new ExportImplementationGuideFile()
                {
                    name = igFile.FileName,
                    mimeType = igFile.MimeType,
                    Description = igFile.Description,
                    Content = Convert.ToBase64String(latestData.Data)
                };

                ExportImplementationGuideFileType type = ExportImplementationGuideFileType.BadSample;
                Enum.TryParse<ExportImplementationGuideFileType>(igFile.ContentType, out type);
                exportIgFile.type = type;

                exportIg.File.Add(exportIgFile);
            }

            return exportIg;
        }
    }
}
