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
    /// <summary>
    /// Supports adding a code system appendix to exported word documents
    /// List of code systems is based on constraints that directly reference a code system and
    /// constraints that reference a valueset that has members associated with code systems.
    /// </summary>
    public class CodeSystemTable
    {
        private IObjectRepository tdb;
        private Body documentBody;
        private List<Template> templates;
        private TableCollection tables;
        private IEnumerable<ViewImplementationGuideCodeSystem> codeSystems;

        public CodeSystemTable(IObjectRepository tdb, Body documentBody, List<Template> templates, TableCollection tables)
        {
            this.tdb = tdb;
            this.documentBody = documentBody;
            this.templates = templates;
            this.tables = tables;

            var implementationGuides = this.templates.Select(y => y.OwningImplementationGuideId).Distinct();

            this.codeSystems = (from igcs in this.tdb.ViewImplementationGuideCodeSystems
                                join ig in implementationGuides on igcs.ImplementationGuideId equals ig
                                select igcs)
                                .Distinct()
                                .OrderBy(y => y.Name);
        }

        /// <summary>
        /// Adds an appendix heading for "Code Systems in this Guide" and a table listing all code systems
        /// used, by name and oid
        /// </summary>
        public void AddCodeSystemAppendix()
        {
            if (this.codeSystems.Count() > 0)
            {
                this.AddAppendixHeading();

                this.AddCodeSystemTable();
            }
        }

        private void AddAppendixHeading()
        {
            Paragraph heading = new Paragraph(
                new ParagraphProperties(
                    new ParagraphStyleId() { Val = Properties.Settings.Default.TemplateTypeHeadingStyle }),
                new Run(
                    new Text("Code Systems in This Guide")));
            this.documentBody.AppendChild(heading);
        }

        private void AddCodeSystemTable()
        {
            string[] headers = new string[] { "Name", "OID" };
            Table t = this.tables.AddTable("Code Systems", headers);

            foreach (var cCodeSystem in codeSystems)
            {
                TableRow newRow = new TableRow(
                    new TableCell(
                        new Paragraph(
                            DocHelper.CreateRun(cCodeSystem.Name))),
                    new TableCell(
                        new Paragraph(
                            DocHelper.CreateRun(cCodeSystem.Identifier))));

                t.Append(newRow);
            }
        }
    }
}
