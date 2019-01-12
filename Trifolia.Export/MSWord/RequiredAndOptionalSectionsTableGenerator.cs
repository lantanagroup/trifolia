using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trifolia.DB;
using Trifolia.Export.Schematron;

namespace Trifolia.Export.MSWord
{
    public class RequiredAndOptionalSectionsTableGenerator
    {
        private TableCollection tables;
        private HyperlinkTracker hyperlinkTracker;
        private List<Template> templates;
        private IObjectRepository tdb;
        private int documentTemplateTypeId;
        private int sectionTemplateTypeId;

        private RequiredAndOptionalSectionsTableGenerator(
            IObjectRepository tdb, 
            TableCollection tables, 
            HyperlinkTracker hyperlinkTracker, 
            List<Template> templates, 
            int documentTemplateTypeId,
            int sectionTemplateTypeId)
        {
            this.tdb = tdb;
            this.tables = tables;
            this.hyperlinkTracker = hyperlinkTracker;
            this.templates = templates;
            this.documentTemplateTypeId = documentTemplateTypeId;
            this.sectionTemplateTypeId = sectionTemplateTypeId;
        }

        public static void Append(
            IObjectRepository tdb, 
            TableCollection tableCollection, 
            HyperlinkTracker hyperlinkTracker, 
            List<Template> templates, 
            int documentTemplateTypeId,
            int sectionTemplateTypeId)
        {
            RequiredAndOptionalSectionsTableGenerator generator = new RequiredAndOptionalSectionsTableGenerator(
                tdb,
                tableCollection, 
                hyperlinkTracker, 
                templates,
                documentTemplateTypeId,
                sectionTemplateTypeId);
            generator.Generate();
        }

        private Table CreateTable()
        {
            List<Models.HeaderDescriptor> headers = new List<Models.HeaderDescriptor>();
            headers.Add(new Models.HeaderDescriptor("Document Type") { AutoResize = true, AutoWrap = true, ColumnWidth = "3386" });
            headers.Add(new Models.HeaderDescriptor("Required Sections") { AutoResize = true, AutoWrap = true, ColumnWidth = "3386" });
            headers.Add(new Models.HeaderDescriptor("Optional Sections") { AutoResize = true, AutoWrap = true, ColumnWidth = "3386" });

            return this.tables.AddTable("Required and Optional Sections for Each Document Type", headers.ToArray());
        }

        private void Generate()
        {
            Table table = this.CreateTable();

            var documentTemplates = this.templates.Where(y => y.TemplateTypeId == this.documentTemplateTypeId).OrderBy(y => y.Name);

            foreach (var documentTemplate in documentTemplates)
            {
                TableRow row = new TableRow(new TableRowProperties());
                table.Append(row);

                TableCell docTypeCell = new TableCell(new TableCellProperties());
                row.Append(docTypeCell);

                Paragraph docTypePara = new Paragraph();
                docTypeCell.Append(docTypePara);

                this.hyperlinkTracker.AddHyperlink(docTypePara, documentTemplate.Name, documentTemplate.Bookmark, Properties.Settings.Default.LinkStyle);
                docTypePara.Append(new Break(), DocHelper.CreateRun(documentTemplate.Oid));

                var sectionsConstraints = (from tc in documentTemplate.ChildConstraints
                                           join tcr in this.tdb.TemplateConstraintReferences on tc.Id equals tcr.TemplateConstraintId
                                           join st in this.templates on tcr.ReferenceIdentifier equals st.Oid
                                           join ptc in documentTemplate.ChildConstraints on tc.ParentConstraintId equals ptc.Id
                                           where tcr.ReferenceType == ConstraintReferenceTypes.Template &&
                                               st.TemplateTypeId == this.sectionTemplateTypeId
                                           select new { Constraint = tc, Template = st });

                List<Template> requiredSections = new List<Template>();
                List<Template> optionalSections = new List<Template>();

                foreach (var sectionConstraint in sectionsConstraints)
                {
                    bool isRequiredAtTopLevel = sectionConstraint.Constraint.ParentConstraintId == null && sectionConstraint.Constraint.IsRequiredConformance();
                    bool isRequiredByParent = sectionConstraint.Constraint.ParentConstraintId != null && sectionConstraint.Constraint.ParentConstraint.HasRequiredParent();

                    if (isRequiredAtTopLevel || isRequiredByParent)
                        requiredSections.Add(sectionConstraint.Template);
                    else
                        optionalSections.Add(sectionConstraint.Template);
                }

                TableCell requiredSectionsCell = new TableCell(new TableCellProperties());
                row.Append(requiredSectionsCell);

                Paragraph requiredSectionsPara = new Paragraph();
                requiredSectionsCell.Append(requiredSectionsPara);

                for (int i = 0; i < requiredSections.Count; i++)
                {
                    this.hyperlinkTracker.AddHyperlink(requiredSectionsPara, requiredSections[i].Name, requiredSections[i].Bookmark, Properties.Settings.Default.LinkStyle);

                    if (i != requiredSections.Count - 1)
                        requiredSectionsPara.Append(new Break());
                }

                if (requiredSections.Count == 0)
                    requiredSectionsPara.Append(DocHelper.CreateRun("N/A"));

                TableCell optionalSectionsCell = new TableCell(new TableCellProperties());
                row.Append(optionalSectionsCell);

                Paragraph optionalSectionsPara = new Paragraph();
                optionalSectionsCell.Append(optionalSectionsPara);

                for (int i = 0; i < optionalSections.Count; i++)
                {
                    this.hyperlinkTracker.AddHyperlink(optionalSectionsPara, optionalSections[i].Name, optionalSections[i].Bookmark, Properties.Settings.Default.LinkStyle);

                    if (i != optionalSections.Count - 1)
                        optionalSectionsPara.Append(new Break());
                }

                if (optionalSections.Count == 0)
                    optionalSectionsPara.Append(DocHelper.CreateRun("N/A"));
            }
        }
    }
}
