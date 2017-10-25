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
        private List<ViewTemplateRelationship> relationships = null;

        #region Public Static Methods

        public static void AddTable(IObjectRepository tdb, List<ViewTemplateRelationship> relationships, TableCollection tables, Body documentBody, Template template, List<Template> exportedTemplates)
        {
            TemplateContextTable cot = new TemplateContextTable(tdb, relationships, tables, documentBody, template, exportedTemplates);
            cot.AddTemplateContextTable();
        }

        #endregion

        #region Ctor

        private TemplateContextTable(IObjectRepository tdb, List<ViewTemplateRelationship> relationships, TableCollection tables, Body documentBody, Template template, List<Template> exportedTemplates)
        {
            this.tdb = tdb;
            this.relationships = relationships;
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

            List<int> exportedTemplateIds = this.exportedTemplates.Select(y => y.Id).ToList();

            string templateIdentifier = template.Oid;
            int templateId = template.Id;
            
            var usedByTemplates = (from tr in this.relationships
                                   where tr.ChildTemplateId == template.Id
                                   select new
                                   {
                                       Name = tr.ParentTemplateName,
                                       Bookmark = tr.ParentTemplateBookmark,
                                       Required = tr.Required
                                   }).Distinct().ToList();
            var containedTemplates = (from tr in this.relationships
                                      where tr.ParentTemplateId == template.Id
                                      select new
                                      {
                                          Name = tr.ChildTemplateName,
                                          Bookmark = tr.ChildTemplateBookmark,
                                          Required = tr.Required
                                      }).Distinct().ToList();

            int maxRows = containedTemplates.Count > usedByTemplates.Count ? containedTemplates.Count : usedByTemplates.Count;

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
                
                var usedByTemplateReference = i < usedByTemplates.Count ? usedByTemplates[i] : null;
                var containedTemplateReference = i < containedTemplates.Count ? containedTemplates[i] : null;

                // Output the used by template
                if (usedByTemplateReference != null)
                {
                    usedByPara.Append(
                        DocHelper.CreateAnchorHyperlink(usedByTemplateReference.Name, usedByTemplateReference.Bookmark, Properties.Settings.Default.TableLinkStyle),
                        DocHelper.CreateRun(usedByTemplateReference.Required ? " (required)" : " (optional)"));

                    usedByCell.Append(usedByPara); 
                }

                // Output the contained template
                if (containedTemplateReference != null)
                {
                    containedPara.Append(
                        DocHelper.CreateAnchorHyperlink(containedTemplateReference.Name, containedTemplateReference.Bookmark, Properties.Settings.Default.TableLinkStyle),
                        DocHelper.CreateRun(containedTemplateReference.Required ? " (required)" : " (optional)"));

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
