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
        private IEnumerable<CodeSystem> codeSystems;

        public CodeSystemTable(IObjectRepository tdb, Body documentBody, List<Template> templates, TableCollection tables)
        {
            this.tdb = tdb;
            this.documentBody = documentBody;
            this.templates = templates;
            this.tables = tables;

            this.codeSystems = (from t1 in this.templates
                                join tc in this.tdb.TemplateConstraints.AsNoTracking() on t1.Id equals tc.TemplateId
                                where tc.CodeSystemId != null
                                select tc.CodeSystem).Union(
                               (from t2 in this.templates
                                join tc in this.tdb.TemplateConstraints.AsNoTracking() on t2.Id equals tc.TemplateId
                                join vs in this.tdb.ValueSets.AsNoTracking() on tc.ValueSetId equals vs.Id
                                join vsm in this.tdb.ValueSetMembers.AsNoTracking() on vs.Id equals vsm.ValueSetId
                                select vsm.CodeSystem))
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

            foreach (CodeSystem cCodeSystem in codeSystems)
            {
                TableRow newRow = new TableRow(
                    new TableCell(
                        new Paragraph(
                            DocHelper.CreateRun(cCodeSystem.Name))),
                    new TableCell(
                        new Paragraph(
                            DocHelper.CreateRun(cCodeSystem.Oid))));

                t.Append(newRow);
            }
        }
    }
}
