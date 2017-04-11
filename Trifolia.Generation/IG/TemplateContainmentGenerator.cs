using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml;

using Trifolia.Logging;
using Trifolia.Shared;
using Trifolia.DB;

namespace Trifolia.Generation.IG
{
    public class TemplateContainmentGenerator
    {
        private WordprocessingDocument document = null;
        private List<Template> allTemplates = null;
        private List<Template> parentTemplates = new List<Template>();
        private IObjectRepository tdb = null;
        private TableCollection tables = null;

        public static void AddTable(IObjectRepository tdb, WordprocessingDocument document, List<Template> allTemplates, TableCollection tables)
        {
            TemplateContainmentGenerator tcg = new TemplateContainmentGenerator(tdb, document, allTemplates, tables);
            tcg.GenerateTable();
        }

        private TemplateContainmentGenerator(IObjectRepository tdb, WordprocessingDocument document, List<Template> allTemplates, TableCollection tables)
        {
            this.tdb = tdb;
            this.document = document;
            this.allTemplates = allTemplates;
            this.tables = tables;
        }

        private void GenerateTable()
        {
            string[] headers = new string[] { GenerationConstants.USED_TEMPLATE_TABLE_TITLE, GenerationConstants.USED_TEMPLATE_TABLE_TYPE, GenerationConstants.USED_TEMPLATE_TABLE_ID };
            Table t = this.tables.AddTable("Template Containments", headers);

            var referencedTemplates = (from at in this.allTemplates
                                       join tcr in this.tdb.TemplateConstraintReferences on at.Oid equals tcr.ReferenceIdentifier
                                       where tcr.ReferenceType == ConstraintReferenceTypes.Template
                                       select at)
                                       .Distinct();

            // Root templates are not referenced elsewhere
            var rootTemplates = this.allTemplates.Where(y => !referencedTemplates.Contains(y));

            foreach (Template cTemplate in rootTemplates)
            {
                parentTemplates.Add(cTemplate);

                AddTemplateContainmentTableEntry(this.tdb, t, cTemplate, 1);

                parentTemplates.Remove(cTemplate);
            }
        }

        private void AddTemplateContainmentTableEntry(IObjectRepository tdb, Table table, Template template, int level)
        {
            int spacing = 0;

            if (level > 1)
            {
                for (int i = 1; i < level; i++)
                    spacing += 144;
            }

            TableRow newRow = new TableRow(
                new TableCell(
                    new Paragraph(
                        new ParagraphProperties(
                            new ParagraphStyleId()
                            {
                                Val = Properties.Settings.Default.TableContentStyle
                            },
                            new Indentation()
                            {
                                Left = new StringValue(spacing.ToString())
                            }),
                        DocHelper.CreateAnchorHyperlink(template.Name, template.Bookmark, Properties.Settings.Default.TableLinkStyle))),
                new TableCell(
                    new Paragraph(
                        new ParagraphProperties(
                            new ParagraphStyleId()
                            {
                                Val = Properties.Settings.Default.TableContentStyle
                            }),
                        DocHelper.CreateRun(template.TemplateType.Name))),
                new TableCell(
                    new Paragraph(
                        new ParagraphProperties(
                            new ParagraphStyleId()
                            {
                                Val = Properties.Settings.Default.TableContentStyle
                            }),
                        DocHelper.CreateRun(template.Oid))));

            table.Append(newRow);

            List<Template> childTemplates = (from tc in template.ChildConstraints
                                             join tcr in this.tdb.TemplateConstraintReferences on tc.Id equals tcr.TemplateConstraintId
                                             join t in this.allTemplates on tcr.ReferenceIdentifier equals t.Oid
                                             where t.Id != template.Id && tcr.ReferenceType == ConstraintReferenceTypes.Template
                                             orderby t.Name
                                             select t)
                                                .Distinct()
                                                .ToList();

            if (childTemplates != null)
            {
                foreach (Template cTemplate in childTemplates)
                {
                    if (this.parentTemplates.Exists(y => y.Id == cTemplate.Id))
                    {
                        Log.For(this).Warn("Circular reference found when generating containment tables for '{0}' ({1})", cTemplate.Name, cTemplate.Oid);
                        continue;
                    }

                    parentTemplates.Add(cTemplate);

                    AddTemplateContainmentTableEntry(tdb, table, cTemplate, level + 1);

                    parentTemplates.Remove(cTemplate);
                }
            }
        }
    }
}
