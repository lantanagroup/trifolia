using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Runtime.CompilerServices;

using Trifolia.Shared;
using Trifolia.DB;

namespace Trifolia.Generation.IG
{
    public class TrifoliaExport
    {
        private int implementationGuideId;
        private StringBuilder output = new StringBuilder();
        private IObjectRepository tdb = null;
        private Dictionary<string, int> templateTypes = null;
        private Dictionary<string, int> conformanceTypes = null;
        private Dictionary<string, int> contexts = null;

        public TrifoliaExport(IObjectRepository tdb, int implementationGuideId)
        {
            this.implementationGuideId = implementationGuideId;
            this.tdb = tdb;
        }

        public void BuildExport()
        {
            this.templateTypes = new Dictionary<string, int>();
            this.conformanceTypes = new Dictionary<string, int>();
            this.contexts = new Dictionary<string, int>();

            this.output.AppendLine("SET foreign_key_checks = 0;\n");

            this.ExportDictionaryContext();

            this.ExportCodeSystem();

            this.ExportValueSet();

            this.ExportValueSetMembers();

            this.ExportConformanceTypes();

            this.ExportVocabularyBindingTypes();

            this.ExportTemplateTypes();

            this.ExportImplementationGuide();

            this.output.Append("\n/* Users */\n");
            this.output.Append("INSERT INTO `tdb_users` (id, user_name, user_password) VALUES (1,'defaultUser','ž·Ó');\n");

            this.ExportTemplates();

            this.ExportTemplateConstraints();

            this.ExportTemplateAssociations();

            this.output.AppendLine("\nSET foreign_key_checks = 1;");
        }

        public string GetExport()
        {
            return this.output.ToString();
        }

        #region Dictionary Context

        private void ExportDictionaryContext()
        {
            var contexts = (from tc in this.tdb.TemplateConstraints
                            join t in this.tdb.Templates on tc.TemplateId equals t.Id
                            where t.OwningImplementationGuideId == this.implementationGuideId
                            select tc.Context)
                            .Union(
                            from t in this.tdb.Templates
                            where t.OwningImplementationGuideId == this.implementationGuideId
                            select (!string.IsNullOrEmpty(t.PrimaryContext) ? t.PrimaryContext : t.TemplateType.RootContext));

            this.output.AppendLine("\n/* Dictionary Contexts */");

            if (contexts.Count() > 0)
            {
                int currentContextId = 0;
                foreach (string currentContext in contexts.Distinct())
                {
                    if (currentContext == null)
                        continue;

                    Dictionary<string, string> columns = new Dictionary<string, string>()
                    {
                        { "ID", (++currentContextId).ToString() },
                        { "context", MakeQuoted(currentContext) },
                        { "lastUpdate", GetDateTime(DateTime.Now) }
                    };

                    this.contexts.Add(currentContext.ToLower(), currentContextId);

                    this.output.AppendLine(
                        CreateInsertStatement("dictionarycontext", columns));
                }
            }
            else
            {
                this.output.AppendLine("/* None */");
            }
        }

        #endregion

        #region Code System

        private void ExportCodeSystem()
        {
            var codeSystems = (from dc in this.tdb.CodeSystems
                            join tc in this.tdb.TemplateConstraints on dc.Id equals tc.CodeSystemId
                            join t in this.tdb.Templates on tc.TemplateId equals t.Id
                            where t.OwningImplementationGuideId == this.implementationGuideId
                            select dc).Distinct();

            this.output.AppendLine("\n/* Dictionary Code System */");

            if (codeSystems.Count() > 0)
            {
                foreach (CodeSystem currentCodeSystems in codeSystems)
                {
                    Dictionary<string, string> columns = new Dictionary<string, string>()
                    {
                        { "ID", currentCodeSystems.Id.ToString() },
                        { "OID", MakeQuoted(currentCodeSystems.Oid) },
                        { "codeSystemName", MakeQuoted(currentCodeSystems.Name) },
                        { "description", MakeQuoted(currentCodeSystems.Description) },
                        { "lastUpdate", GetDateTime(currentCodeSystems.LastUpdate) }
                    };

                    this.output.AppendLine(
                        CreateInsertStatement("dictionarycontext", columns));
                }
            }
            else
            {
                this.output.AppendLine("/* None */");
            }
        }

        #endregion

        #region Value Sets

        private void ExportValueSet()
        {
            var valueSets = (from vs in this.tdb.ValueSets
                             join tc in this.tdb.TemplateConstraints on vs.Id equals tc.ValueSetId
                             join t in this.tdb.Templates on tc.TemplateId equals t.Id
                             where t.OwningImplementationGuideId == this.implementationGuideId
                             select vs).Distinct();

            this.output.AppendLine("\n/* Value Sets */");

            if (valueSets.Count() > 0)
            {
                foreach (ValueSet currentValueSet in valueSets)
                {
                    Dictionary<string, string> columns = new Dictionary<string, string>()
                    {
                        { "ID", currentValueSet.Id.ToString() },
                        { "OID", MakeQuoted(currentValueSet.Oid) },
                        { "valueSetName", MakeQuoted(currentValueSet.Name) },
                        { "valueSetCode", MakeQuoted(currentValueSet.Code) },
                        { "description", MakeQuoted(currentValueSet.Description) },
                        { "intensional", MakeBit(currentValueSet.Intensional) },
                        { "intensionalDefinition", MakeQuoted(currentValueSet.IntensionalDefinition) },
                        { "lastUpdate", GetDateTime(currentValueSet.LastUpdate) }
                    };

                    this.output.AppendLine(
                        CreateInsertStatement("valueset", columns));
                }
            }
            else
            {
                this.output.AppendLine("/* None */");
            }
        }

        private void ExportValueSetMembers()
        {
            var valueSetMembers = (from vs in this.tdb.ValueSets
                                   join vsm in this.tdb.ValueSetMembers on vs.Id equals vsm.ValueSetId
                                   join tc in this.tdb.TemplateConstraints on vs.Id equals tc.ValueSetId
                                   join t in this.tdb.Templates on tc.TemplateId equals t.Id
                                   where t.OwningImplementationGuideId == this.implementationGuideId
                                   select vsm).Distinct();

            this.output.AppendLine("\n/* Value Set Members */");

            if (valueSetMembers.Count() > 0)
            {
                foreach (ValueSetMember currentValueSetMember in valueSetMembers)
                {
                    Dictionary<string, string> columns = new Dictionary<string, string>()
                    {
                        { "ID", currentValueSetMember.Id.ToString() },
                        { "valueSetId", currentValueSetMember.ValueSetId.ToString() },
                        { "valueSetOId", MakeQuoted(currentValueSetMember.ValueSet.Oid) },
                        { "codeSystemId", MakeQuoted(currentValueSetMember.CodeSystemId) },
                        { "code", MakeQuoted(currentValueSetMember.Code) },
                        { "codeSystemOID", MakeQuoted(currentValueSetMember.CodeSystem.Oid) },
                        { "displayName", MakeQuoted(currentValueSetMember.DisplayName) },
                        { "dateOfValueSetStatus", MakeQuoted(currentValueSetMember.StatusDate) },
                        { "lastUpdate", GetDateTime(DateTime.Now) }
                    };

                    this.output.AppendLine(
                        CreateInsertStatement("valuesetmember", columns));
                }
            }
            else
            {
                this.output.AppendLine("/* None */");
            }
        }

        #endregion

        #region Conformance and Binding Types

        private void ExportConformanceTypes()
        {
            var conformanceTypes = (from tc in this.tdb.TemplateConstraints
                                    join t in this.tdb.Templates on tc.TemplateId equals t.Id
                                    where t.OwningImplementationGuideId == this.implementationGuideId && tc.Conformance != null
                                    select tc.Conformance).Distinct();

            this.output.AppendLine("\n/* Conformance Types */");

            if (conformanceTypes.Count() > 0)
            {
                int conformanceTypeId = 0;
                foreach (string currentConformanceType in conformanceTypes)
                {
                    Dictionary<string, string> columns = new Dictionary<string, string>()
                    {
                        { "id", (++conformanceTypeId).ToString() },
                        { "conftype", MakeQuoted(currentConformanceType) },
                        { "lastUpdate", GetDateTime(DateTime.Now) }
                    };
                    this.conformanceTypes.Add(currentConformanceType, conformanceTypeId);

                    this.output.AppendLine(
                        CreateInsertStatement("conformance_type", columns));
                }
            }
            else
            {
                this.output.AppendLine("/* None */");
            }
        }

        private void ExportVocabularyBindingTypes()
        {
            Dictionary<string, string> columns1 = new Dictionary<string, string>()
            {
                { "id", "1" },
                { "bindingtype", MakeQuoted("STATIC") },
                { "lastUpdate", GetDateTime(DateTime.Now) }
            };

            this.output.AppendLine(
                CreateInsertStatement("conformance_type", columns1));

            Dictionary<string, string> columns2 = new Dictionary<string, string>()
            {
                { "id", "2" },
                { "bindingtype", MakeQuoted("DYNAMIC") },
                { "lastUpdate", GetDateTime(DateTime.Now) }
            };

            this.output.AppendLine(
                CreateInsertStatement("conformance_type", columns2));
        }

        #endregion

        #region Template Types

        private void ExportTemplateTypes()
        {
            var templateTypes = (from tt in this.tdb.TemplateTypes
                                 join t in this.tdb.Templates on tt.Id equals t.TemplateTypeId
                                 where t.OwningImplementationGuideId == this.implementationGuideId
                                 select tt).Distinct();

            this.output.AppendLine("\n/* Template Types */");

            if (templateTypes.Count() > 0)
            {
                foreach (TemplateType currentTemplateType in templateTypes)
                {
                    if (this.templateTypes.ContainsKey(currentTemplateType.Name))
                        continue;

                    int id = this.templateTypes.Count > 0 ? this.templateTypes.Values.Max() + 1 : 1;

                    this.templateTypes.Add(currentTemplateType.Name, id);

                    Dictionary<string, string> columns = new Dictionary<string, string>()
                    {
                        { "id", id.ToString() },
                        { "templatetype", MakeQuoted(currentTemplateType.Name) },
                        { "lastUpdate", GetDateTime(DateTime.Now) }
                    };

                    this.output.AppendLine(
                        CreateInsertStatement("template_type", columns));
                }
            }
            else
            {
                this.output.AppendLine("/* None */");
            }
        }

        #endregion

        #region Implementation Guides

        private void ExportImplementationGuide()
        {
            ImplementationGuide ig = this.tdb.ImplementationGuides.Single(y => y.Id == this.implementationGuideId);

            this.output.AppendLine("\n/* Implementation Guides */");

            Dictionary<string, string> columns = new Dictionary<string, string>()
            {
                { "id", ig.Id.ToString() },
                { "title", MakeQuoted(ig.Name) },
                { "lastUpdate", GetDateTime(DateTime.Now) }
            };

            this.output.AppendLine(
                CreateInsertStatement("implementationguide", columns));
        }

        #endregion

        #region Templates

        private void ExportTemplates()
        {
            var templates = (from t in this.tdb.Templates
                             where t.OwningImplementationGuideId == this.implementationGuideId
                             select t).Distinct();

            this.output.AppendLine("\n/* Templates */");

            if (templates.Count() > 0)
            {
                foreach (Template currentTemplate in templates)
                {
                    int templateTypeId = this.templateTypes[currentTemplate.TemplateType.Name];
                    int? contextId = !string.IsNullOrEmpty(currentTemplate.PrimaryContext) ?
                        new Nullable<int>(this.contexts[currentTemplate.PrimaryContext.ToLower()]) : 
                        null;

                    Dictionary<string, string> columns = new Dictionary<string, string>()
                    {
                        { "ID", currentTemplate.Id.ToString() },
                        { "OID", MakeQuoted(currentTemplate.Oid) },
                        { "isOpen", MakeBit(currentTemplate.IsOpen) },
                        { "title", MakeQuoted(currentTemplate.Name) },
                        { "templatetype", templateTypeId.ToString() },
                        { "description", MakeQuoted(currentTemplate.Description) },
                        { "keywords", "NULL" },
                        { "primaryContext", contextId != null ? contextId.Value.ToString() : "NULL" },
                        { "impliedTemplate", currentTemplate.ImpliedTemplateId != null ? currentTemplate.ImpliedTemplateId.ToString() : "NULL" },
                        { "notes", MakeQuoted(currentTemplate.Notes) },
                        { "lastUpdate", GetDateTime(DateTime.Now) },
                        { "uname", MakeQuoted("defaultUser") }
                    };

                    this.output.AppendLine(
                        CreateInsertStatement("template", columns));
                }
            }
            else
            {
                this.output.AppendLine("/* None */");
            }
        }

        class TemplateConstraintComparer : IEqualityComparer<TemplateConstraint>
        {
            #region IEqualityComparer<TemplateConstraint> Members

            public bool Equals(TemplateConstraint x, TemplateConstraint y)
            {
                return x.Id.Equals(y.Id);
            }

            public int GetHashCode(TemplateConstraint obj)
            {
                return obj.Id.GetHashCode();
            }

            #endregion
        }

        private void ExportTemplateConstraints()
        {
            List<TemplateConstraint> templateConstraints = (from tc in this.tdb.TemplateConstraints
                                       join t in this.tdb.Templates on tc.TemplateId equals t.Id
                                       where t.OwningImplementationGuideId == this.implementationGuideId
                                       select tc).ToList();

            templateConstraints = templateConstraints.Distinct().ToList();

            this.output.AppendLine("\n/* Template Constraints */");

            if (templateConstraints.Count() > 0)
            {
                foreach (TemplateConstraint currentTemplateConstraint in templateConstraints)
                {
                    int? contextId = !string.IsNullOrEmpty(currentTemplateConstraint.Context) ? new Nullable<int>(this.contexts[currentTemplateConstraint.Context.ToLower()]) : null;
                    int? conformanceId = !string.IsNullOrEmpty(currentTemplateConstraint.Conformance) ? new Nullable<int>(this.conformanceTypes[currentTemplateConstraint.Conformance]) : null;
                    int? valueConformanceId = !string.IsNullOrEmpty(currentTemplateConstraint.ValueConformance) ? new Nullable<int>(this.conformanceTypes[currentTemplateConstraint.ValueConformance]) : null;
                    string staticDynamic = "NULL";

                    if (currentTemplateConstraint.IsStatic == true)
                        staticDynamic = "1";
                    else if (currentTemplateConstraint.IsStatic == false)
                        staticDynamic = "2";

                    Dictionary<string, string> columns = new Dictionary<string, string>()
                    {
                        { "ID", currentTemplateConstraint.Id.ToString() },
                        { "parentConstraintID", currentTemplateConstraint.ParentConstraintId != null ? currentTemplateConstraint.ParentConstraintId.Value.ToString() : "NULL" },
                        { "templateID", currentTemplateConstraint.TemplateId.ToString() },
                        { "order", currentTemplateConstraint.Order.ToString() },
                        { "isBranch", MakeBit(currentTemplateConstraint.IsBranch) },
                        { "businessConformance", conformanceId != null ? conformanceId.Value.ToString() : "NULL" },
                        { "cardinality", MakeQuoted(currentTemplateConstraint.Cardinality) },
                        { "context", contextId != null ? contextId.ToString() : "NULL" },
                        { "containedTemplate", currentTemplateConstraint.ContainedTemplateId != null ? currentTemplateConstraint.ContainedTemplateId.Value.ToString() : "NULL" },
                        { "isPrimitive", MakeBit(currentTemplateConstraint.IsPrimitive) },
                        { "constraintNarrative", MakeQuoted(currentTemplateConstraint.Notes) },
                        { "valueConformance", valueConformanceId != null ? valueConformanceId.Value.ToString() : "NULL" },
                        { "staticDynamic", staticDynamic },
                        { "valueSetOID", currentTemplateConstraint.ValueSetId != null ? currentTemplateConstraint.ValueSetId.Value.ToString() : "NULL" },
                        { "version", GetDateTime(currentTemplateConstraint.ValueSetDate) },
                        { "codeOrValue", MakeQuoted(currentTemplateConstraint.Value) },
                        { "codeSystemOID", currentTemplateConstraint.CodeSystemId != null ? currentTemplateConstraint.CodeSystemId.Value.ToString() : "NULL" },
                        { "displayName", MakeQuoted(currentTemplateConstraint.DisplayName) },
                        { "datatype", MakeQuoted(currentTemplateConstraint.DataType) },
                        { "schematronTest", MakeQuoted(currentTemplateConstraint.Schematron) },
                        { "lastUpdate", GetDateTime(DateTime.Now) },
                        { "uname", MakeQuoted("defaultUser") }
                    };

                    this.output.AppendLine(
                        CreateInsertStatement("template_constraint", columns));
                }
            }
            else
            {
                this.output.AppendLine("/* None */");
            }
        }

        private void ExportTemplateAssociations()
        {
            var templates = (from t in this.tdb.Templates
                             where t.OwningImplementationGuideId == this.implementationGuideId
                             select t).Distinct();

            this.output.AppendLine("\n/* Templates <-> IG Assocation */");

            if (templates.Count() > 0)
            {
                int id = 1;

                foreach (Template currentTemplate in templates)
                {
                    Dictionary<string, string> columns = new Dictionary<string, string>()
                    {
                        { "ID", id.ToString() },
                        { "templateId", currentTemplate.Id.ToString() },
                        { "implementationGuideId", currentTemplate.OwningImplementationGuideId.ToString() },
                        { "lastUpdate", GetDateTime(DateTime.Now) }
                    };

                    this.output.AppendLine(
                        CreateInsertStatement("associationtemplateimplementationguide", columns));

                    id++;
                }
            }
            else
            {
                this.output.AppendLine("/* None */");
            }
        }

        #endregion

        #region Utility

        internal static string CreateInsertStatement(string table, Dictionary<string, string> columnValues)
        {
            string columns = string.Format("`{0}`", string.Join("`, `", columnValues.Keys.ToArray<string>()));
            string values = string.Join(", ", columnValues.Values.ToArray<string>());

            string insertStatement = string.Format("INSERT INTO `{0}` ({1}) VALUES ({2});", table, columns, values);

            return insertStatement;
        }

        private static string MakeBit(bool? value)
        {
            if (value == null)
                return "NULL";

            if (value == true)
                return "1";
            
            return "0";
        }

        private static string MakeQuoted(object value)
        {
            if (value == null)
                return "NULL";

            return string.Format("'{0}'", 
                value.ToString().Replace("'", "''"));       // Make ' be '' so that it does not break the insert statement
        }

        private static string GetDateTime(DateTime? date)
        {
            if (date == null)
                return "NULL";

            return string.Format("'{0}'",
                date.Value.ToString("yyyy-MM-dd hh:mm:ss"));
        }

        #endregion
    }
}
