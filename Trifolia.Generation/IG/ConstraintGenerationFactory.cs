using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml;

using Trifolia.Shared;
using Trifolia.DB;
using Trifolia.Generation.IG.ConstraintGeneration;

namespace Trifolia.Generation.IG
{
    public class ConstraintGenerationFactory
    {
        public static IConstraintGenerator NewConstraintGenerator(
            IGSettingsManager igSettings,
            Body documentBody, 
            CommentManager cmtMgr,
            FigureCollection figures,
            WIKIParser wikiParser,
            bool includeSamples,
            IObjectRepository dataSource, 
            List<TemplateConstraint> rootConstraints, 
            List<TemplateConstraint> allConstraints, 
            Template currentTemplate,
            List<Template> allTemplates,
            string constraintHeadingStyle,
            List<string> selectedCategories)
        {
            IConstraintGenerator constraintGenerator = null;

            if (igSettings.GetBoolSetting(IGSettingsManager.SettingProperty.UseConsolidatedConstraintFormat))
                constraintGenerator = new ConsolidatedGeneration();
            else
                constraintGenerator = new LegacyGeneration();

            constraintGenerator.IGSettings = igSettings;
            constraintGenerator.DocumentBody = documentBody;
            constraintGenerator.Figures = figures;
            constraintGenerator.WikiParser = wikiParser;
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

            return constraintGenerator;
        }
    }
}
