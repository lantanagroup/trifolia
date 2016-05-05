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

namespace Trifolia.Generation.IG
{
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
        private List<Template> templates;
        private TableCollection tables;
        private List<string> selectedCategories = null;
        private bool includeCategory = false;

        #region Ctor

        public TemplateConstraintTable(IGSettingsManager igSettings, List<Template> templates, TableCollection tables, List<string> selectedCategories)
        {
            this.igSettings = igSettings;
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
        public void AddTemplateConstraintTable(Template template, Body documentBody, string templateXpath)
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
            lHeaders.Add(new HeaderDescriptor() { HeaderName = VERB, AutoResize = false, CellWidth = .6, ColumnWidth = "864" });
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

            // Start out creating the first set of rows with root (top-level) constraints (constraints that don't have children)
            foreach (TemplateConstraint cConstraint in rootConstraints.OrderBy(y => y.Order))
            {
                if (this.HasSelectedCategories && !string.IsNullOrEmpty(cConstraint.Category) && !this.selectedCategories.Contains(cConstraint.Category))
                    continue;

                this.AddTemplateTableConstraint(template, t, cConstraint, 1, includeCategoryHeader);
            }
        }

        private void AddTemplateTableConstraint(Template template, Table table, TemplateConstraint constraint, int level, bool includeCategoryHeader)
        {
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

                if (constraint.ValueSet != null)
                    fixedValue = string.Format("{0} ({1})", constraint.ValueSet.Oid, constraint.ValueSet.Name);
                else if (constraint.CodeSystem != null)
                    fixedValue = string.Format("{0} ({1})", constraint.CodeSystem.Oid, constraint.CodeSystem.Name);

                if (!string.IsNullOrEmpty(constraint.Value))
                {
                    if (!string.IsNullOrEmpty(fixedValue))
                        fixedValue += " = " + constraint.Value;
                    else
                        fixedValue = constraint.Value;
                }
                else if (constraint.ContainedTemplate != null)
                {
                    fixedValue = string.Format("{0} (identifier: {1}", constraint.ContainedTemplate.Name, constraint.ContainedTemplate.Oid);
                    if (this.templates.Contains(constraint.ContainedTemplate))
                        fixedValueLink = constraint.ContainedTemplate.Bookmark;
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


                this.AddTemplateTableConstraint(template, table, cConstraint, level + 1, includeCategoryHeader);
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
