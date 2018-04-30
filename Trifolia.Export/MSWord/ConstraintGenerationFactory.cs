using System.Collections.Generic;
using DocumentFormat.OpenXml.Wordprocessing;
using Trifolia.Shared;
using Trifolia.DB;
using Trifolia.Export.MSWord.ConstraintGeneration;
using DocumentFormat.OpenXml.Packaging;

namespace Trifolia.Export.MSWord
{
    public class ConstraintGenerationFactory
    {
        public static IConstraintGenerator NewConstraintGenerator(
            IGSettingsManager igSettings,
            MainDocumentPart mainPart,
            CommentManager cmtMgr,
            FigureCollection figures,
            bool includeSamples,
            IObjectRepository dataSource, 
            List<TemplateConstraint> rootConstraints, 
            List<TemplateConstraint> allConstraints, 
            Template currentTemplate,
            List<Template> allTemplates,
            string constraintHeadingStyle,
            List<string> selectedCategories,
            HyperlinkTracker hyperlinkTracker)
        {
            IConstraintGenerator constraintGenerator = null;

            if (igSettings.GetBoolSetting(IGSettingsManager.SettingProperty.UseConsolidatedConstraintFormat))
                constraintGenerator = new ConsolidatedGeneration();
            else
                constraintGenerator = new LegacyGeneration();

            constraintGenerator.IGSettings = igSettings;
            constraintGenerator.Figures = figures;
            constraintGenerator.IncludeSamples = includeSamples;
            constraintGenerator.DataSource = dataSource;
            constraintGenerator.RootConstraints = rootConstraints;
            constraintGenerator.AllConstraints = allConstraints;
            constraintGenerator.CurrentTemplate = currentTemplate;
            constraintGenerator.AllTemplates = allTemplates;
            constraintGenerator.ConstraintHeadingStyle = constraintHeadingStyle;
            constraintGenerator.CommentManager = cmtMgr;
            constraintGenerator.IncludeCategory = !string.IsNullOrEmpty(igSettings.GetSetting(IGSettingsManager.SettingProperty.Categories));
            constraintGenerator.SelectedCategories = selectedCategories;
            constraintGenerator.HyperlinkTracker = hyperlinkTracker;
            constraintGenerator.MainPart = mainPart;
            constraintGenerator.DocumentBody = mainPart.Document.Body;

            return constraintGenerator;
        }
    }
}
