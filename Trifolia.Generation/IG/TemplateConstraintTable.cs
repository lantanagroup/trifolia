using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml;

using Trifolia.DB;
using Trifolia.Generation.IG.Models;
using Trifolia.Shared;
using Trifolia.Shared.Plugins;

namespace Trifolia.Generation.IG
{
    /// <summary>
    /// Constraint overview table in MS Word exports
    /// </summary>
    public class TemplateConstraintTable
    {
        private const string CATEGORY = "Category";
        private const string NAME = "Name";
        private const string XPATH = "XPath";
        private const string CARD = "Card.";
        private const string VERB = "Verb";
        private const string DATA_TYPE = "Data Type";
        private const string CONF = "CONF#";
        private const string FIXED_VALUE = "Value";

        private IGSettingsManager igSettings;
        private IIGTypePlugin igTypePlugin;
        private List<Template> templates;
        private TableCollection tables;
        private List<string> selectedCategories = null;
        private IObjectRepository tdb;
        private List<ConstraintReference> references = null;

        #region Ctor

        public TemplateConstraintTable(IObjectRepository tdb, List<ConstraintReference> references, IGSettingsManager igSettings, IIGTypePlugin igTypePlugin, List<Template> templates, TableCollection tables, List<string> selectedCategories)
        {
            this.tdb = tdb;
            this.references = references;
            this.igSettings = igSettings;
            this.igTypePlugin = igTypePlugin;
            this.templates = templates;
            this.tables = tables;
            this.selectedCategories = selectedCategories;
        }

        #endregion

        #region Private Methods

        private bool HasSelectedCategories
        {
            get
            {
                return this.selectedCategories != null && this.selectedCategories.Count > 0;
            }
        }

        /// <summary>
        /// Adds a table for the template in question which lists all constraints in separate rows.
        /// </summary>
        public void AddTemplateConstraintTable(SimpleSchema schema, Template template, Body documentBody, string templateXpath)
        {
            var constraintCategories = template.ChildConstraints.Where(y => !string.IsNullOrEmpty(y.Category)).Select(y => y.Category).Distinct();
            var includeCategoryHeader = false;

            if (!this.HasSelectedCategories && constraintCategories.Count() > 0)
                includeCategoryHeader = true;
            else if (this.HasSelectedCategories && this.selectedCategories.Count > 1)
                includeCategoryHeader = true;

            var rootConstraints = template.ChildConstraints.Where(y => y.ParentConstraintId == null);
            List<HeaderDescriptor> lHeaders = new List<HeaderDescriptor>();

            if (includeCategoryHeader)
                lHeaders.Add(new HeaderDescriptor() { HeaderName = CATEGORY, AutoResize = true, AutoWrap = false, ColumnWidth = "720" });

            lHeaders.Add(new HeaderDescriptor() { HeaderName = XPATH, AutoResize = true, AutoWrap = false, ColumnWidth = "3445" });
            lHeaders.Add(new HeaderDescriptor() { HeaderName = CARD, AutoResize = false, CellWidth = .5, ColumnWidth = "720" });
            lHeaders.Add(new HeaderDescriptor() { HeaderName = VERB, AutoResize = false, CellWidth = .8, ColumnWidth = "1152" });
            lHeaders.Add(new HeaderDescriptor() { HeaderName = DATA_TYPE, AutoResize = false, CellWidth = .6, ColumnWidth = "864" });
            lHeaders.Add(new HeaderDescriptor() { HeaderName = CONF, AutoResize = false, CellWidth = .6, ColumnWidth = "864" });
            lHeaders.Add(new HeaderDescriptor() { HeaderName = FIXED_VALUE, AutoResize = false, CellWidth = .6, ColumnWidth = "3171" });

            Table t = this.tables.AddTable(string.Format("{0} Constraints Overview", template.Name), lHeaders.ToArray());

            TableRow templateRow = new TableRow(
                new TableCell(
                    new TableCellProperties(
                        new GridSpan()
                        {
                            Val = new Int32Value(includeCategoryHeader ? 7 : 6)
                        }),
                    new Paragraph(
                        new ParagraphProperties(
                            new ParagraphStyleId()
                            {
                                Val = Properties.Settings.Default.TableContentStyle
                            }),
                        DocHelper.CreateRun(templateXpath))));
            t.Append(templateRow);

            var templateSchema = schema.GetSchemaFromContext(template.PrimaryContextType);
            var constraintIds = template.ChildConstraints.Select(y => y.Id).ToList();
            var templateReferences = (from r in this.references
                                      join tc in constraintIds on r.TemplateConstraintId equals tc
                                      select r);

            // Start out creating the first set of rows with root (top-level) constraints (constraints that don't have children)
            foreach (TemplateConstraint cConstraint in rootConstraints.OrderBy(y => y.Order))
            {
                if (this.HasSelectedCategories && !string.IsNullOrEmpty(cConstraint.Category) && !this.selectedCategories.Contains(cConstraint.Category))
                    continue;

                var schemaObject = templateSchema != null ? templateSchema.Children.SingleOrDefault(y => y.Name == cConstraint.Context) : null;

                this.AddTemplateTableConstraint(template, templateReferences, t, cConstraint, 1, includeCategoryHeader, schemaObject);
            }
        }

        private void AddTemplateTableConstraint(Template template, IEnumerable<ConstraintReference> templateReferences, Table table, TemplateConstraint constraint, int level, bool includeCategoryHeader, SimpleSchema.SchemaObject schemaObject)
        {
            // Skip the child constraints if this is a choice there is only one child constraint. This constraint
            // adopts the constraint narrative of the child constraint when there is only one option.
            if (constraint.IsChoice && constraint.ChildConstraints.Count == 1)
                constraint = constraint.ChildConstraints.First();

            if (constraint.IsPrimitive != true && !string.IsNullOrEmpty(constraint.Context))
            {
                string xpath = constraint.Context;
                string cardinality = constraint.Cardinality;
                string conformance = constraint.Conformance;
                string dataType = constraint.DataType;
                string fixedValue = string.Empty;
                string fixedValueLink = string.Empty;
                string levelSpacing = string.Empty;
                string confNumber = constraint.GetFormattedNumber(this.igSettings.PublishDate);
                var isFhir = constraint.Template.ImplementationGuideType.SchemaURI == ImplementationGuideType.FHIR_NS;

                // Check if we're dealing with a FHIR constraint
                if (isFhir && schemaObject != null)
                    dataType = schemaObject.DataType;

                if (constraint.ValueSet != null)
                {
                    string valueSetIdentifier = constraint.ValueSet.GetIdentifier(igTypePlugin);
                    fixedValue = string.Format("{0} ({1})", valueSetIdentifier, constraint.ValueSet.Name);
                }
                else if (constraint.CodeSystem != null)
                {
                    fixedValue = string.Format("{0} ({1})", constraint.CodeSystem.Oid, constraint.CodeSystem.Name);
                }

                var constraintReferences = templateReferences.Where(y => y.TemplateConstraintId == constraint.Id);

                if (!string.IsNullOrEmpty(constraint.Value))
                {
                    if (!string.IsNullOrEmpty(fixedValue))
                        fixedValue += " = " + constraint.Value;
                    else
                        fixedValue = constraint.Value;
                }
                else if (constraintReferences.Count() > 0)
                {
                    var containedTemplateValues = constraintReferences.Select(y => string.Format("{0} (identifier: {1}", y.Name, y.Identifier));
                    fixedValue = string.Join(" or ", containedTemplateValues);

                    var firstConstraintReference = constraintReferences.First();

                    if (constraintReferences.Count() == 1 && firstConstraintReference.IncludedInIG)
                        fixedValueLink = firstConstraintReference.Bookmark;
                }

                for (int i = 1; i <= (level); i++)      // One tab for each level
                    levelSpacing += "\t";

                TableRow entryRow = new TableRow();

                if (includeCategoryHeader)
                    AppendTextCell(entryRow, constraint.Category);

                AppendTextCell(entryRow, levelSpacing + xpath);
                AppendTextCell(entryRow, cardinality);
                AppendTextCell(entryRow, conformance);
                AppendTextCell(entryRow, dataType);
                AppendHyperlinkCell(entryRow, confNumber, "C_" + confNumber);

                if (!string.IsNullOrEmpty(fixedValueLink))
                    AppendHyperlinkCell(entryRow, fixedValue, fixedValueLink);
                else
                    AppendTextCell(entryRow, fixedValue);

                table.AppendChild(entryRow);
            }

            // Recursively handle child constraints
            var childConstraints = template.ChildConstraints
                .Where(y => y.ParentConstraintId == constraint.Id)
                .OrderBy(y => y.Order);

            foreach (TemplateConstraint cConstraint in childConstraints)
            {
                if (this.HasSelectedCategories && !string.IsNullOrEmpty(cConstraint.Category) && !this.selectedCategories.Contains(cConstraint.Category))
                    continue;

                var nextSchemaObject = schemaObject != null ?
                    schemaObject.Children.SingleOrDefault(y => y.Name == cConstraint.Context) :
                    null;

                this.AddTemplateTableConstraint(template, templateReferences, table, cConstraint, level + 1, includeCategoryHeader, nextSchemaObject);
            }
        }

        private void AppendTextCell(TableRow row, string text)
        {
            TableCell cell = new TableCell(
                    new Paragraph(
                        new ParagraphProperties(
                            new ParagraphStyleId()
                            {
                                Val = Properties.Settings.Default.TableContentStyle
                            }),
                        new Run(
                            new Text(text) { Space = SpaceProcessingModeValues.Preserve })));

            row.AppendChild(cell);
        }

        private void AppendHyperlinkCell(TableRow row, string text, string hyperlink)
        {
            TableCell cell = new TableCell(
                    new Paragraph(
                        new ParagraphProperties(
                            new ParagraphStyleId()
                            {
                                Val = Properties.Settings.Default.TableContentStyle
                            }),
                        DocHelper.CreateAnchorHyperlink(text, hyperlink, Properties.Settings.Default.TableLinkStyle)));

            row.AppendChild(cell);
        }

        #endregion
    }
}
