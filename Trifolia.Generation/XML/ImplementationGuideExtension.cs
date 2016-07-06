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
        public static ExportImplementationGuide Export(this ImplementationGuide ig, IObjectRepository tdb, IGSettingsManager igSettings)
        {
            ExportImplementationGuide exportIg = new ExportImplementationGuide()
            {
                name = ig.Name,
                displayName = ig.DisplayName,
                webDisplayName = ig.WebDisplayName,
                WebDescription = ig.WebDescription,
                WebReadmeOverview = ig.WebReadmeOverview,
                Volume1 = null
            };

            if (ig.PreviousVersion.Count != 0)
            {
                exportIg.PreviousVersion = new Shared.ImportExport.Model.TrifoliaImplementationGuidePreviousVersion()
                {
                    name = ig.PreviousVersion.First().Name,
                    number = ig.PreviousVersion.First().Version.HasValue ? ig.PreviousVersion.First().Version.Value : 1
                };
            }

            exportIg.CardinalityDisplay.Add(new Shared.ImportExport.Model.TrifoliaImplementationGuideCardinalityDisplay()
            {
                cardinality = "0..0",
                display = igSettings.GetSetting(IGSettingsManager.SettingProperty.CardinalityZero)
            });
            exportIg.CardinalityDisplay.Add(new Shared.ImportExport.Model.TrifoliaImplementationGuideCardinalityDisplay()
            {
                cardinality = "0..1",
                display = igSettings.GetSetting(IGSettingsManager.SettingProperty.CardinalityZeroToOne)
            });
            exportIg.CardinalityDisplay.Add(new Shared.ImportExport.Model.TrifoliaImplementationGuideCardinalityDisplay()
            {
                cardinality = "0..*",
                display = igSettings.GetSetting(IGSettingsManager.SettingProperty.CardinalityZeroOrMore)
            });
            exportIg.CardinalityDisplay.Add(new Shared.ImportExport.Model.TrifoliaImplementationGuideCardinalityDisplay()
            {
                cardinality = "1..1",
                display = igSettings.GetSetting(IGSettingsManager.SettingProperty.CardinalityOneToOne)
            });
            exportIg.CardinalityDisplay.Add(new Shared.ImportExport.Model.TrifoliaImplementationGuideCardinalityDisplay()
            {
                cardinality = "1..*",
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
