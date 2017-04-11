using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml;

using Trifolia.Shared;
using Trifolia.DB;
using Trifolia.Shared.Plugins;

namespace Trifolia.Generation.IG.ConstraintGeneration
{
    public class ConsolidatedGeneration : IConstraintGenerator
    {
        #region Private Fields

        private int templateConstraintCount = 1;
        
        #endregion

        #region IConstraintGenerator Properties

        private IGSettingsManager igSettings;
        private Body documentBody;
        private FigureCollection figures;
        private WIKIParser wikiParser;
        private bool includeSamples;
        private IObjectRepository dataSource;
        private List<TemplateConstraint> rootConstraints;
        private List<TemplateConstraint> allConstraints;
        private Template currentTemplate;
        private List<Template> allTemplates;
        private string constraintHeadingStyle;

        public IIGTypePlugin IGTypePlugin { get; set; }

        public CommentManager CommentManager { get; set; }

        public string ConstraintHeadingStyle
        {
            get { return constraintHeadingStyle; }
            set { constraintHeadingStyle = value; }
        }

        public IGSettingsManager IGSettings
        {
            get { return igSettings; }
            set { igSettings = value; }
        }

        public Body DocumentBody
        {
            get { return documentBody; }
            set { documentBody = value; }
        }

        public FigureCollection Figures
        {
            get { return figures; }
            set { figures = value; }
        }

        public WIKIParser WikiParser
        {
            get { return wikiParser; }
            set { wikiParser = value; }
        }

        public bool IncludeSamples
        {
            get { return includeSamples; }
            set { includeSamples = value; }
        }

        public IObjectRepository DataSource
        {
            get { return dataSource; }
            set { dataSource = value; }
        }

        public List<TemplateConstraint> RootConstraints
        {
            get { return rootConstraints; }
            set { rootConstraints = value; }
        }

        public List<TemplateConstraint> AllConstraints
        {
            get { return allConstraints; }
            set { allConstraints = value; }
        }

        public Template CurrentTemplate
        {
            get { return currentTemplate; }
            set { currentTemplate = value; }
        }

        public List<Template> AllTemplates
        {
            get { return allTemplates; }
            set { allTemplates = value; }
        }

        public bool IncludeCategory { get; set; }
        public List<string> SelectedCategories { get; set; }

        private bool HasSelectedCategories
        {
            get
            {
                return this.SelectedCategories != null && this.SelectedCategories.Count > 0;
            }
        }

        #endregion

        public void GenerateConstraints(bool aCreateLinksForValueSets = false, bool includeNotes = false)
        {
            // Output the constraints
            foreach (TemplateConstraint cConstraint in rootConstraints)
            {
                if (this.HasSelectedCategories && !cConstraint.CategoryIsMatch(this.SelectedCategories))
                    continue;

                this.AddTemplateConstraint(cConstraint, 1, aCreateLinksForValueSets, includeNotes);
            }
        }

        private void AddTemplateConstraint(TemplateConstraint constraint, int level, bool aCreateLinksForValueSets, bool includeNotes)
        {
            // Skip the child constraints if this is a choice there is only one child constraint. This constraint
            // adopts the constraint narrative of the child constraint when there is only one option.
            if (constraint.IsChoice && constraint.ChildConstraints.Count == 1)
                constraint = constraint.ChildConstraints.First();

            // TODO: May be able to make this more efficient
            List<TemplateConstraint> childConstraints = allConstraints
                .Where(y => y.ParentConstraintId == constraint.Id)
                .OrderBy(y => y.Order)
                .ToList();

            this.templateConstraintCount++;

            // All contained template references must exist in the list of templates being exported to be linked
            // TODO: Improve so that not all references have to be in the export for some to be linked
            var containedTemplateReferences = constraint.References.Where(y => y.ReferenceType == ConstraintReferenceTypes.Template);
            var containedTemplatesFound = (from ct in containedTemplateReferences
                                           join t in this.allTemplates on ct.ReferenceIdentifier equals t.Oid
                                           select t);
            bool containedTemplateLinked = containedTemplateReferences.Count() > 0 && containedTemplateReferences.Count() == containedTemplatesFound.Count();

            bool includeCategory = this.IncludeCategory && (!this.HasSelectedCategories || this.SelectedCategories.Count > 1);
            IFormattedConstraint fConstraint = FormattedConstraintFactory.NewFormattedConstraint(this.dataSource, this.igSettings, this.IGTypePlugin, constraint, linkContainedTemplate: containedTemplateLinked, linkIsBookmark: true, createLinksForValueSets: aCreateLinksForValueSets, includeCategory: includeCategory);
            Paragraph para = fConstraint.AddToDocParagraph(this.wikiParser, this.DocumentBody, level -1, GenerationConstants.BASE_TEMPLATE_INDEX + (int)currentTemplate.Id, this.constraintHeadingStyle);

            if (!string.IsNullOrEmpty(constraint.Notes) && includeNotes)
                this.CommentManager.AddCommentRange(para, constraint.Notes);

            // Add child constraints
            foreach (TemplateConstraint cConstraint in childConstraints)
            {
                if (this.HasSelectedCategories && !cConstraint.CategoryIsMatch(this.SelectedCategories))
                    continue;

                this.AddTemplateConstraint(cConstraint, level + 1, aCreateLinksForValueSets, includeNotes);
            }

            // Add samples for the constraint if it is a heading and the settings indicate to include samples
            if (constraint.IsHeading && this.IncludeSamples)
            {
                foreach (var cSample in constraint.Samples)
                {
                    figures.AddSample(cSample.Name, cSample.SampleText);
                }
            }
        }
    }
}
