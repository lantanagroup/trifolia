using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Trifolia.DB;
using Trifolia.Shared;
using ExportImplementationGuide = Trifolia.Shared.ImportExport.Model.TrifoliaImplementationGuide;

namespace Trifolia.Generation.XML
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

            foreach (var schematronPattern in ig.SchematronPatterns)
            {
                exportIg.CustomSchematron.Add(new Shared.ImportExport.Model.TrifoliaImplementationGuideCustomSchematron()
                {
                    phase = schematronPattern.Phase,
                    patternId = schematronPattern.PatternId,
                    Rule = schematronPattern.PatternContent
                });
            }

            foreach (var igTemplateType in ig.TemplateTypes)
            {
                exportIg.CustomTemplateType.Add(new Shared.ImportExport.Model.TrifoliaImplementationGuideCustomTemplateType()
                {
                    templateTypeName = igTemplateType.TemplateType.Name,
                    CustomName = igTemplateType.Name,
                    Description = igTemplateType.DetailsText
                });
            }

            var volume1Html = igSettings.GetSetting(IGSettingsManager.SettingProperty.Volume1Html);

            if (!string.IsNullOrEmpty(volume1Html) || ig.Sections.Count > 0)
                exportIg.Volume1 = new Shared.ImportExport.Model.TrifoliaImplementationGuideVolume1();

            if (!string.IsNullOrEmpty(volume1Html))
                exportIg.Volume1.Items.Add(volume1Html);

            if (ig.Sections.Count > 0)
            {
                foreach (var section in ig.Sections.OrderBy(y => y.Order))
                {
                    exportIg.Volume1.Items.Add(new Shared.ImportExport.Model.TrifoliaImplementationGuideVolume1Section()
                    {
                        Heading = new Shared.ImportExport.Model.TrifoliaImplementationGuideVolume1SectionHeading()
                        {
                            Title = section.Heading,
                            Level = section.Level
                        },
                        Content = section.Content
                    });
                }
            }

            return exportIg;
        }
    }
}
