using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml;

using Trifolia.DB;

namespace Trifolia.Generation.IG
{
    public class TemplateContextTable
    {
        private const string TEMPLATE_CONTEXT_TABLE_USED_BY = "Contained By:";
        private const string TEMPLATE_CONTEXT_TABLE_CONTAINS = "Contains:";

        private IObjectRepository tdb = null;
        private TableCollection tables = null;
        private Template template = null;
        private Body documentBody = null;
        private List<Template> exportedTemplates = null;
        private IEnumerable<TemplateConstraint> allConstraints = null;

        #region Public Static Methods

        public static void AddTable(IObjectRepository tdb, TableCollection tables, Body documentBody, Template template, List<Template> exportedTemplates)
        {
            TemplateContextTable cot = new TemplateContextTable(tdb, tables, documentBody, template, exportedTemplates);
            cot.AddTemplateContextTable();
        }

        #endregion

        #region Ctor

        private TemplateContextTable(IObjectRepository tdb, TableCollection tables, Body documentBody, Template template, List<Template> exportedTemplates)
        {
            this.tdb = tdb;
            this.tables = tables;
            this.template = template;
            this.documentBody = documentBody;
            this.exportedTemplates = exportedTemplates;

            this.allConstraints = template.ChildConstraints;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Adds a context table for the template. The context table lists all templates that are used by the current template, as 
        /// well as all templates that this current template uses. Templates that use this template also indicate whether they are
        /// required or optional, depending on the conformance of the constraint that uses this template.
        /// </summary>
        /// <param name="template">The template in question</param>
        /// <param name="allConstraints">All constraints for the current template</param>
        private void AddTemplateContextTable()
        {
            TableRow newRow = new TableRow();
            TableCell usedByCell = new TableCell();
            TableCell containedCell = new TableCell();

            var usedByTemplates = (from tc in this.tdb.TemplateConstraints
                                              join te in this.tdb.Templates on tc.TemplateId equals te.Id
                                              where tc.ContainedTemplateId == template.Id && tc.TemplateId != template.Id
                                              orderby tc.Conformance, te.Name
                                              select te)
                                              .Distinct().ToList();
            var containedTemplates = (from ac in allConstraints
                                                 join ct in this.tdb.Templates on ac.ContainedTemplateId equals ct.Id
                                                 where this.exportedTemplates.Exists(y => y.Id == ct.Id) && ac.ContainedTemplateId != null
                                                 orderby ct.Name
                                                 select ct)
                                                 .Distinct().ToList();

            var usedByTemplatesSelectedForExport = usedByTemplates.Where(e => this.exportedTemplates.Exists(y => y.Id == e.Id)).ToList();
            var containedTemplatesSelectedForExport = containedTemplates.Where(e => this.exportedTemplates.Exists(y => y.Id == e.Id)).ToList();

            int maxRows = containedTemplatesSelectedForExport.Count > usedByTemplatesSelectedForExport.Count ? containedTemplatesSelectedForExport.Count : usedByTemplatesSelectedForExport.Count;

            for (int i = 0; i < maxRows; i++)
            {
                Paragraph usedByPara = new Paragraph(
                            new ParagraphProperties(
                                new ParagraphStyleId()
                                {
                                    Val = Properties.Settings.Default.TableContentStyle
                                }));
                Paragraph containedPara = new Paragraph(
                            new ParagraphProperties(
                                new ParagraphStyleId()
                                {
                                    Val = Properties.Settings.Default.TableContentStyle
                                }));
                
                Template usedByTemplate = i < usedByTemplatesSelectedForExport.Count ? usedByTemplatesSelectedForExport[i] : null;
                Template containedTemplate = i < containedTemplatesSelectedForExport.Count ? containedTemplatesSelectedForExport[i] : null;

                // Output the used by template
                if (usedByTemplate != null)
                {
                    List<TemplateConstraint> usedByConstraints = this.tdb.TemplateConstraints.Where(y =>
                        y.TemplateId == usedByTemplate.Id &&
                        y.ContainedTemplateId == template.Id).ToList();
                    bool isRequired = AreConstraintsRequiredByParents(usedByConstraints);

                    // Output a hyperlink if it is included in this doc, otherwise plain text
                    if (this.exportedTemplates.Exists(y => y.Id == usedByTemplate.Id))
                        usedByPara.Append(
                            DocHelper.CreateAnchorHyperlink(usedByTemplate.Name, usedByTemplate.Bookmark, Properties.Settings.Default.TableLinkStyle),
                            DocHelper.CreateRun(isRequired ? " (required)" : " (optional)"));
                    else
                        usedByPara.Append(
                            DocHelper.CreateRun(usedByTemplate.Name),
                            DocHelper.CreateRun(isRequired ? " (required)" : " (optional)"));

                    usedByCell.Append(usedByPara); 
                }

                // Output the contained template
                if (containedTemplate != null)
                {
                    // Output a hyperlink if it is included in this doc, otherwise plain text
                    if (this.exportedTemplates.Exists(y => y.Id == containedTemplate.Id))
                        containedPara.Append(
                            DocHelper.CreateAnchorHyperlink(containedTemplate.Name, containedTemplate.Bookmark, Properties.Settings.Default.TableLinkStyle));
                    else
                        containedPara.Append(
                            DocHelper.CreateRun(containedTemplate.Name));

                    containedCell.Append(containedPara);
                }
            }

            // Make sure the cells have at least one paragraph in them
            if (containedCell.ChildElements.Count == 0)
                containedCell.AppendChild(new Paragraph());

            if (usedByCell.ChildElements.Count == 0)
                usedByCell.AppendChild(new Paragraph());

            // Only add the table to the document if there are conatined or used-by relationships
            if (maxRows > 0)
            {
                string[] headers = new string[] { TEMPLATE_CONTEXT_TABLE_USED_BY, TEMPLATE_CONTEXT_TABLE_CONTAINS };
                Table t = this.tables.AddTable(string.Format("{0} Contexts", template.Name), headers);

                t.Append(
                    new TableRow(usedByCell, containedCell));
            }
        }

        #endregion

        #region Private Static Methods

        private static bool AreConstraintsRequiredByParents(List<TemplateConstraint> constraints)
        {
            foreach (TemplateConstraint cConstraint in constraints)
            {
                if (IsConstraintRequiredByParent(cConstraint))
                    return true;
            }

            return false;
        }

        private static bool IsConstraintRequiredByParent(TemplateConstraint constraint)
        {
            if (constraint != null)
            {
                if (!string.IsNullOrEmpty(constraint.Conformance) && constraint.Conformance != "SHALL")
                    return false;

                if (!IsConstraintRequiredByParent(constraint.ParentConstraint))
                    return false;
            }

            return true;
        }

        #endregion
    }
}
