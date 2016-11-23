using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Http;
using System.Xml;
using Trifolia.Authorization;
using Trifolia.Config;
using Trifolia.DB;
using Trifolia.Import.Terminology.Excel;
using Trifolia.Import.Terminology.External;
using Trifolia.Shared;
using Trifolia.Terminology;
using Trifolia.Web.Models.TerminologyManagement;

namespace Trifolia.Web.Controllers.API
{
    public class TerminologyController : ApiController
    {
        private IObjectRepository tdb;

        #region Constructor

        public TerminologyController()
            : this(DBContext.CreateAuditable(CheckPoint.Instance.UserName, CheckPoint.Instance.HostAddress))
        {
        }

        public TerminologyController(IObjectRepository tdb)
        {
            this.tdb = tdb;
        }

        #endregion

        #region Value Sets

        [HttpGet, Route("api/Terminology/ValueSet/{valueSetId}/Relationships"), SecurableAction(SecurableNames.VALUESET_LIST)]
        public List<Relationship> GetRelationships(int valueSetId)
        {
            List<Relationship> relationships = new List<Relationship>();
            User currentUser = CheckPoint.Instance.GetUser(this.tdb);
            var templateIds = this.tdb.ViewTemplatePermissions.Where(y => y.UserId == currentUser.Id).Select(y => y.TemplateId);

            if (CheckPoint.Instance.IsDataAdmin)
                templateIds = this.tdb.Templates.Select(y => y.Id);

            var publishStatus = PublishStatus.GetPublishedStatus(this.tdb);
            var constraints = (from t in templateIds
                               join tc in this.tdb.TemplateConstraints on t equals tc.TemplateId
                               where tc.ValueSetId == valueSetId
                               select tc)
                               .OrderBy(y => y.Template.OwningImplementationGuide.Name)
                               .ThenBy(y => y.Template.Name);

            foreach (var constraint in constraints)
            {
                Relationship relationship = relationships.SingleOrDefault(y => y.TemplateOid == constraint.Template.Oid);

                if (relationship == null)
                {
                    relationship = new Relationship()
                    {
                        ImplementationGuideId = constraint.Template.OwningImplementationGuideId,
                        ImplementationGuideName = constraint.Template.OwningImplementationGuide.GetDisplayName(),
                        TemplateName = constraint.Template.Name,
                        TemplateOid = constraint.Template.Oid,
                        TemplateUrl = constraint.Template.GetViewUrl(),
                        IsImplementationGuidePublished = constraint.Template.OwningImplementationGuide.PublishStatus == publishStatus
                    };
                    relationships.Add(relationship);
                }

                relationship.Bindings.Add(new Binding()
                {
                    ConstraintNumber = constraint.GetFormattedNumber(),
                    Date = constraint.ValueSetDate,
                    Strength = constraint.IsStatic == true ? BindingStrengths.Static : BindingStrengths.Dynamic
                });
            }

            return relationships;
        }

        [HttpGet, Route("api/Terminology/ValueSet/Basic"), SecurableAction(SecurableNames.VALUESET_LIST)]
        public IEnumerable<BasicItem> GetBasicValueSets()
        {
            return (from vs in this.tdb.ValueSets
                    select new BasicItem()
                    {
                        Id = vs.Id,
                        Name = vs.Name,
                        Oid = vs.Oid
                    });
        }

        [HttpGet, Route("api/Terminology/ValueSet/{valueSetId}/Concepts/{activeDate}"), SecurableAction(SecurableNames.VALUESET_LIST)]
        public ConceptItems Concepts(int valueSetId, DateTime activeDate)
        {
            var valueSet = this.tdb.ValueSets.Single(y => y.Id == valueSetId);
            var activeMembers = valueSet.GetActiveMembers(activeDate);
            ConceptItems ci = new ConceptItems();
            ci.rows = (from am in activeMembers
                       select new ConceptItem()
                       {
                           Id = am.Id,
                           Code = am.Code,
                           DisplayName = am.DisplayName,
                           CodeSystemId = am.CodeSystemId,
                           CodeSystemName = am.CodeSystem.Name,
                           CodeSystemOid = am.CodeSystem.Oid,
                           Status = am.Status,
                           StatusDate = am.StatusDate
                       }).ToArray();
            ci.total = ci.rows.Length;

            return ci;
        }

        [HttpGet, Route("api/Terminology/ValueSet/{valueSetId}/Concepts"), SecurableAction(SecurableNames.VALUESET_LIST)]
        public ConceptItems Concepts(int valueSetId, int? page = null, string query = null, int count = 20)
        {
            ConceptItems concepts = new ConceptItems();
            var rows = (from vsm in this.tdb.ValueSetMembers
                        where vsm.ValueSetId == valueSetId &&
                        (string.IsNullOrEmpty(query) || (
                           vsm.Code.ToLower().Contains(query.ToLower()) ||
                           vsm.DisplayName.ToLower().Contains(query.ToLower())
                        ))
                        orderby vsm.DisplayName
                        select new ConceptItem()
                        {
                            Id = vsm.Id,
                            Code = vsm.Code,
                            DisplayName = vsm.DisplayName,
                            CodeSystemId = vsm.CodeSystemId,
                            CodeSystemName = vsm.CodeSystem.Name,
                            CodeSystemOid = vsm.CodeSystem.Oid,
                            Status = vsm.Status,
                            StatusDate = vsm.StatusDate
                        });

            concepts.total = rows.Count();

            if (page != null)
            {
                int skipCount = (page.Value - 1) * count;
                rows = rows.Skip(skipCount).Take(count);
            }

            concepts.rows = rows.ToArray();

            return concepts;
        }

        [HttpGet, Route("api/Terminology/ValueSets"), SecurableAction(SecurableNames.VALUESET_LIST)]
        public ValueSetItems ValueSets(string search = null)
        {
            return ValueSets(search, "Name", 1, Int32.MaxValue, "asc");
        }

        [HttpGet, SecurableAction(SecurableNames.VALUESET_LIST), Route("api/Terminology/ValueSets/SortedAndPaged")]
        public ValueSetItems ValueSets(
            string search = null,
            string sort = "Name", 
            int page = 1, 
            int rows = 50, 
            string order = "desc")
        {
            ValueSetItems result = new ValueSetItems();
            var permitModify = CheckPoint.Instance.HasSecurables(SecurableNames.VALUESET_EDIT);
            bool permitOverride = CheckPoint.Instance.HasSecurables(SecurableNames.TERMINOLOGY_OVERRIDE);
            bool userIsInternal = CheckPoint.Instance.IsDataAdmin;
            int userId = CheckPoint.Instance.GetUser(this.tdb).Id;

            var searchResults = this.tdb.SearchValueSet(userId, search, rows, page, sort, order == "desc").ToList();

            if (searchResults.Count > 0)
                result.TotalItems = searchResults.First().TotalItems;

            result.Items = (from r in searchResults
                            select new ValueSetItem()
                            {
                                Id = r.Id,
                                Code = r.Code,
                                Name = r.Name,
                                Oid = r.Oid,
                                Description = r.Description,
                                IntentionalDefinition = r.IntensionalDefinition,
                                IsIntentional = r.Intensional == true,
                                IsComplete = r.IsComplete,
                                SourceUrl = r.SourceUrl,
                                PermitModify = permitModify,
                                CanModify = !r.HasPublishedIg,
                                CanOverride = permitOverride && (userIsInternal || r.CanEditPublishedIg)
                            }).ToList();

            return result;
        }

        [HttpGet, Route("api/Terminology/ValueSet/{valueSetId}"), SecurableAction(SecurableNames.VALUESET_EDIT)]
        public ValueSetModel ValueSet(int valueSetId)
        {
            ValueSet valueSet = this.tdb.ValueSets.Single(y => y.Id == valueSetId);
            ValueSetModel model = new ValueSetModel()
            {
                Id = valueSet.Id,
                Name = valueSet.Name,
                Code = valueSet.Code,
                Description = valueSet.Description,
                IntentionalDefinition = valueSet.IntensionalDefinition,
                IsComplete = !valueSet.IsIncomplete,
                IsIntentional = valueSet.Intensional.HasValue ? valueSet.Intensional.Value : false,
                Oid = valueSet.Oid,
                SourceUrl = valueSet.Source
            };

            return model;
        }

        [HttpGet, Route("api/Terminology/ValueSet/Find"), SecurableAction(SecurableNames.VALUESET_LIST)]
        public int? FindValueSet(string identifier)
        {
            if (string.IsNullOrEmpty(identifier))
                throw new ArgumentNullException("identifier");

            var valueSets = this.tdb.ValueSets.Where(y => y.Oid == identifier);

            if (valueSets.Count() > 0)
                return valueSets.First().Id;

            return null;
        }

        [HttpGet, Route("api/Terminology/CodeSystem/Find"), SecurableAction(SecurableNames.CODESYSTEM_LIST)]
        public int? FindCodeSystem(string identifier)
        {
            if (string.IsNullOrEmpty(identifier))
                throw new ArgumentNullException("identifier");

            var codeSystems = this.tdb.CodeSystems.Where(y => y.Oid == identifier);

            if (codeSystems.Count() > 0)
                return codeSystems.First().Id;

            return null;
        }

        [HttpDelete, Route("api/Terminology/ValueSet/{valueSetId}"), SecurableAction(SecurableNames.VALUESET_EDIT)]
        public void DeleteValueSet(int valueSetId)
        {
            using (IObjectRepository auditedTdb = DBContext.CreateAuditable(CheckPoint.Instance.UserName, CheckPoint.Instance.HostAddress))
            {
                var valueSet = auditedTdb.ValueSets.SingleOrDefault(y => y.Id == valueSetId);

                if (!valueSet.CanModify(auditedTdb) && !valueSet.CanOverride(auditedTdb))
                    throw new AuthorizationException("You do not have the permission to delete this valueset");

                // Nullify the references to the valueset on constraints
                valueSet.Constraints.ToList().ForEach(y =>
                {
                    y.ValueSet = null;
                });

                // Remove members from the valueset
                valueSet.Members.ToList().ForEach(y => {
                    auditedTdb.ValueSetMembers.DeleteObject(y);
                });

                // Delete the actual valueset
                auditedTdb.ValueSets.DeleteObject(valueSet);

                auditedTdb.SaveChanges();
            }
        }

        [HttpPost, Route("api/Terminology/ValueSet/Concepts"), SecurableAction(SecurableNames.VALUESET_EDIT)]
        public void SaveValueSetConcepts(SaveValueSetConceptsModel model)
        {
            using (IObjectRepository auditedTdb = DBContext.CreateAuditable(CheckPoint.Instance.UserName, CheckPoint.Instance.HostAddress))
            {
                var valueSet = auditedTdb.ValueSets.Single(y => y.Id == model.ValueSetId);

                if (model.Concepts != null)
                {
                    foreach (var concept in model.Concepts)
                    {
                        ValueSetMember member = null;

                        if (concept.Id != null)
                        {
                            member = valueSet.Members.Single(y => y.Id == concept.Id);
                        }
                        else
                        {
                            member = new ValueSetMember();
                            valueSet.Members.Add(member);
                        }

                        if (member.Code != concept.Code)
                            member.Code = concept.Code;

                        if (member.DisplayName != concept.DisplayName)
                            member.DisplayName = concept.DisplayName;

                        if (member.CodeSystemId != concept.CodeSystemId)
                            member.CodeSystemId = concept.CodeSystemId;

                        if (member.Status != concept.Status)
                            member.Status = concept.Status;

                        if (member.StatusDate != concept.StatusDate)
                            member.StatusDate = concept.StatusDate;
                    }
                }

                if (model.RemovedConcepts != null)
                {
                    foreach (var concept in model.RemovedConcepts)
                    {
                        ValueSetMember member = valueSet.Members.Single(y => y.Id == concept.Id);
                        auditedTdb.ValueSetMembers.DeleteObject(member);
                    }
                }

                auditedTdb.SaveChanges();
            }
        }

        [HttpPost, Route("api/Terminology/ValueSet/Save"), SecurableAction(SecurableNames.VALUESET_EDIT)]
        public int SaveValueSet(SaveValueSetModel model)
        {
            using (IObjectRepository auditedTdb = DBContext.CreateAuditable(CheckPoint.Instance.UserName, CheckPoint.Instance.HostAddress))
            {
                ValueSetModel valueSetModel = model.ValueSet;
                ValueSet valueSet = auditedTdb.ValueSets.SingleOrDefault(y => y.Id == valueSetModel.Id);

                if (valueSet == null)
                {
                    valueSet = new ValueSet();
                    auditedTdb.ValueSets.AddObject(valueSet);
                }

                // Set properties for the value set
                if (valueSetModel.Code != valueSet.Code)
                    valueSet.Code = valueSetModel.Code;

                if (valueSetModel.Description != valueSet.Description)
                    valueSet.Description = valueSetModel.Description;

                if (valueSetModel.IntentionalDefinition != valueSet.IntensionalDefinition)
                    valueSet.IntensionalDefinition = valueSetModel.IntentionalDefinition;

                var isIncomplete = !valueSetModel.IsComplete;
                if (isIncomplete != valueSet.IsIncomplete)
                    valueSet.IsIncomplete = isIncomplete;

                if (valueSetModel.IsIntentional != valueSet.Intensional)
                    valueSet.Intensional = valueSetModel.IsIntentional;

                if (valueSetModel.Name != valueSet.Name)
                    valueSet.Name = valueSetModel.Name;

                if (valueSetModel.Oid != valueSet.Oid)
                    valueSet.Oid = valueSetModel.Oid;

                if (valueSetModel.SourceUrl != valueSet.Source)
                    valueSet.Source = valueSetModel.SourceUrl;

                // Remove concepts
                if (model.RemovedConcepts != null)
                {
                    foreach (var concept in model.RemovedConcepts)
                    {
                        var valueSetMember = auditedTdb.ValueSetMembers.Single(y => y.Id == concept.Id && y.ValueSetId == valueSet.Id);
                        auditedTdb.ValueSetMembers.DeleteObject(valueSetMember);
                    }
                }

                // Update concepts
                if (model.Concepts != null)
                {
                    foreach (var concept in model.Concepts)
                    {
                        ValueSetMember valueSetMember = auditedTdb.ValueSetMembers.SingleOrDefault(y => y.Id == concept.Id && y.ValueSetId == valueSet.Id);

                        if (valueSetMember == null)
                        {
                            valueSetMember = new ValueSetMember();
                            valueSetMember.ValueSet = valueSet;
                            auditedTdb.ValueSetMembers.AddObject(valueSetMember);
                        }

                        this.SaveConceptProperties(auditedTdb, valueSetMember, concept);
                    }
                }

                auditedTdb.SaveChanges();

                return valueSet.Id;
            }
        }

        private void SaveConceptProperties(IObjectRepository tdb, ValueSetMember member, ConceptItem concept)
        {
            if (member.Code != concept.Code)
                member.Code = concept.Code;

            if (member.DisplayName != concept.DisplayName)
                member.DisplayName = concept.DisplayName;

            CodeSystem foundCodeSystem = tdb.CodeSystems.Single(y => y.Id == concept.CodeSystemId);
            if (member.CodeSystem != foundCodeSystem)
                member.CodeSystem = foundCodeSystem;

            if (member.Status != concept.Status)
                member.Status = concept.Status;

            if (member.StatusDate != concept.StatusDate)
                member.StatusDate = concept.StatusDate;
        }

        #endregion

        #region Code Systems

        [HttpGet, Route("api/Terminology/CodeSystem/Basic"), SecurableAction()]
        public IEnumerable<BasicItem> GetBasicCodeSystems()
        {
            return (from cs in this.tdb.CodeSystems
                    select new BasicItem()
                    {
                        Id = cs.Id,
                        Name = cs.Name, 
                        Oid = cs.Oid
                    }).OrderBy(y => y.Name);
        }

        [HttpGet, Route("api/Terminology/CodeSystems"), SecurableAction(SecurableNames.CODESYSTEM_LIST)]
        public CodeSystemItems CodeSystems(string search = null)
        {
            return CodeSystems("Name", 1, Int32.MaxValue, "asc", search);
        }

        [HttpGet, Route("api/Terminology/CodeSystems/SortedAndPaged"), SecurableAction(SecurableNames.CODESYSTEM_LIST)]
        public CodeSystemItems CodeSystems(
            string sort,
            int page,
            int rows,
            string order,
            string search)
        {
            var codeSystems = this.tdb.CodeSystems.Where(y => y.Name != null);

            if (!string.IsNullOrEmpty(search))
            {
                var searchLower = search.ToLower();

                codeSystems = (from cs in this.tdb.CodeSystems
                               where cs.Name.ToLower().Contains(searchLower) ||
                               cs.Oid.ToLower().Contains(searchLower) ||
                               cs.Description.ToLower().Contains(searchLower)
                               select cs);
            }

            int total = codeSystems.Count();
            int start = (page - 1) * rows;

            switch (sort)
            {
                case "MemberCount":
                    if (order == "desc")
                        codeSystems = codeSystems.OrderByDescending(y => y.Members.Count);
                    else
                        codeSystems = codeSystems.OrderBy(y => y.Members.Count);
                    break;
                case "ConstraintCount":
                    if (order == "desc")
                        codeSystems = codeSystems.OrderByDescending(y => y.Constraints.Count);
                    else
                        codeSystems = codeSystems.OrderBy(y => y.Constraints.Count);
                    break;
                case "Oid":
                    if (order == "desc")
                        codeSystems = codeSystems.OrderByDescending(y => y.Oid);
                    else
                        codeSystems = codeSystems.OrderBy(y => y.Oid);
                    break;
                case "Name":
                default:
                    if (order == "desc")
                        codeSystems = codeSystems.OrderByDescending(y => y.Name);
                    else
                        codeSystems = codeSystems.OrderBy(y => y.Name);
                    break;

            }

            // Pagination
            codeSystems = codeSystems
                .Skip(start)
                .Take(rows);

            CodeSystemItem[] retCodeSystems = codeSystems.AsEnumerable().Select(y => new CodeSystemItem()
            {
                Id = y.Id,
                Name = y.Name,
                Oid = y.Oid,
                Description = y.Description,
                ConstraintCount = y.Constraints.Count,
                MemberCount = y.Members.Count,
                PermitModify = CheckPoint.Instance.HasSecurables(SecurableNames.CODESYSTEM_EDIT),
                CanModify = y.CanModify(this.tdb),
                CanOverride = y.CanOverride(this.tdb)
            }).ToArray();

            CodeSystemItems result = new CodeSystemItems()
            {
                total = total,
                rows = retCodeSystems
            };

            return result;
        }

        [HttpPost, Route("api/Terminology/CodeSystems/Save"), SecurableAction(SecurableNames.CODESYSTEM_EDIT)]
        public CodeSystemItem SaveCodeSystem(CodeSystemItem item)
        {
            var foundCodeSystem = this.tdb.CodeSystems.SingleOrDefault(y => y.Id == item.Id);

            if (foundCodeSystem != null && !foundCodeSystem.CanModify(this.tdb) && !foundCodeSystem.CanOverride(this.tdb))
                throw new Exception("You cannot modify this code system.");

            if (foundCodeSystem == null)
            {
                foundCodeSystem = new CodeSystem();
                this.tdb.CodeSystems.AddObject(foundCodeSystem);
            }

            if (foundCodeSystem.Name != item.Name)
                foundCodeSystem.Name = item.Name;

            if (foundCodeSystem.Oid != item.Oid)
                foundCodeSystem.Oid = item.Oid;

            if (foundCodeSystem.Description != item.Description)
                foundCodeSystem.Description = item.Description;

            this.tdb.SaveChanges();

            // Make sure the item's Id is correct (if the code system was added)
            item.Id = foundCodeSystem.Id;

            return item;
        }

        [HttpDelete, Route("api/Terminology/CodeSystem/{codeSystemId}"), SecurableAction(SecurableNames.CODESYSTEM_EDIT)]
        public void DeleteCodeSystem(int codeSystemId)
        {
            var foundCodeSystem = this.tdb.CodeSystems.SingleOrDefault(y => y.Id == codeSystemId);

            if (!foundCodeSystem.CanModify(this.tdb) && !foundCodeSystem.CanOverride(this.tdb))
                throw new Exception("You cannot delete this code system.");

            // Remove the reference to the code system from the constraints
            foreach (var currentConstraint in foundCodeSystem.Constraints.ToList())
            {
                currentConstraint.CodeSystem = null;
            }

            // Remove members of the code system
            foreach (var currentMember in foundCodeSystem.Members.ToList())
            {
                this.tdb.ValueSetMembers.DeleteObject(currentMember);
            }

            this.tdb.CodeSystems.DeleteObject(foundCodeSystem);

            this.tdb.SaveChanges();
        }

        #endregion

        #region Export

        [HttpGet, Route("api/Terminology/Export/ValueSet/{valueSetOid}"), SecurableAction(SecurableNames.EXPORT_VOCAB)]
        public string ExportValueSet(string valueSetOid, VocabularyOutputType format = VocabularyOutputType.Default, string encoding = "UTF-8")
        {
            VocabularyService service = new VocabularyService(this.tdb);
            return service.GetValueSet(valueSetOid, (int)format, encoding);
        }

        [HttpGet, Route("api/Terminology/Export/ImplementationGuide/{implementationGuideId}"), SecurableAction(SecurableNames.EXPORT_VOCAB)]
        public string ExportImplementationGuide(int implementationGuideId, int maxMembers = 100, VocabularyOutputType format = VocabularyOutputType.Default, string encoding = "UTF-8")
        {
            VocabularyService service = new VocabularyService(this.tdb);
            return service.GetImplementationGuideVocabulary(implementationGuideId, maxMembers, (int)format, encoding);
        }

        #endregion

        #region Import from Excel

        [HttpPost, Route("api/Terminology/Import/Excel"), SecurableAction(SecurableNames.ADMIN)]
        public void ExcelImport(ImportCheckResponse checkResponse)
        {
            using (IObjectRepository auditedTdb = DBContext.CreateAuditable(CheckPoint.Instance.UserName, CheckPoint.Instance.HostAddress))
            {
                ExcelImporter importer = new ExcelImporter(auditedTdb);
                importer.Import(checkResponse);
            }
        }

        [HttpPost, Route("api/Terminology/Import/Excel/Check"), SecurableAction(SecurableNames.ADMIN)]
        public ImportCheckResponse CheckExcelImport(ImportCheckRequest request)
        {
            ExcelImporter importer = new ExcelImporter(this.tdb);
            return importer.GetCheckResponse(request);
        }

        #endregion

        #region Import External

        [HttpGet, Route("api/Terminology/Import/PhinVads/Search"), SecurableAction(SecurableNames.ADMIN)]
        public ImportValueSet SearchPhinVads(string oid)
        {
            PhinVadsValueSetImportProcessor<ImportValueSet, ImportValueSetMember> processor = 
                new PhinVadsValueSetImportProcessor<ImportValueSet, ImportValueSetMember>();

            ImportValueSet valueSet = processor.FindValueSet(this.tdb, oid);
            return valueSet;
        }

        [HttpGet, Route("api/Terminology/Import/RoseTree/Search"), SecurableAction(SecurableNames.ADMIN)]
        public ImportValueSet SearchRoseTree(string oid)
        {
            string roseTreeLocation = AppSettings.HL7RoseTreeLocation;
            XmlDocument roseTreeDoc = new XmlDocument();
            roseTreeDoc.Load(roseTreeLocation);

            HL7RIMValueSetImportProcessor<ImportValueSet, ImportValueSetMember> processor =
                new HL7RIMValueSetImportProcessor<ImportValueSet, ImportValueSetMember>(roseTreeDoc);

            ImportValueSet valueSet = processor.FindValueSet(this.tdb, oid);
            return valueSet;
        }

        [HttpPost, Route("api/Terminology/Import/External"), SecurableAction(SecurableNames.ADMIN)]
        public void SaveExternalValueSet(ImportValueSet valueSet)
        {
            BaseValueSetImportProcess<ImportValueSet, ImportValueSetMember> processor;

            if (valueSet.ImportSource == "PHIN VADS")
            {
                processor = new PhinVadsValueSetImportProcessor<ImportValueSet, ImportValueSetMember>();
            }
            else if (valueSet.ImportSource == "HL7 RIM/RoseTree")
            {
                string roseTreeLocation = AppSettings.HL7RoseTreeLocation;
                XmlDocument roseTreeDoc = new XmlDocument();
                roseTreeDoc.Load(roseTreeLocation);

                processor = new HL7RIMValueSetImportProcessor<ImportValueSet, ImportValueSetMember>(roseTreeDoc);
            }
            else
                throw new Exception("Cannot identify which external soure the value set came from.");

            processor.SaveValueSet(this.tdb, valueSet);

            this.tdb.SaveChanges();
        }

        #endregion
    }

    public static class ValueSetExtensions
    {
        public static bool CanModify(this ValueSet valueSet, IObjectRepository tdb)
        {
            if (valueSet == null)
                return true;

            if (!CheckPoint.Instance.HasSecurables(SecurableNames.VALUESET_EDIT))
                return false;

            var publishedImplementationGuides = (from tc in valueSet.Constraints
                                                 join t in tdb.Templates on tc.TemplateId equals t.Id
                                                 where t.OwningImplementationGuide.IsPublished()
                                                 select t.OwningImplementationGuide.Id).AsEnumerable();

            return publishedImplementationGuides.Count() == 0;
        }

        public static bool CanOverride(this ValueSet valueSet, IObjectRepository tdb)
        {
            if (valueSet == null)
                return true;

            if (CheckPoint.Instance.IsDataAdmin)
                return true;

            if (!CheckPoint.Instance.HasSecurables(SecurableNames.TERMINOLOGY_OVERRIDE))
                return false;

            var publishedImplementationGuides = (from tc in valueSet.Constraints
                                                 join t in tdb.Templates on tc.TemplateId equals t.Id
                                                 where t.OwningImplementationGuide.IsPublished()
                                                 select t.OwningImplementationGuide.Id).AsEnumerable();

            var uneditablePublishedImplementationGuides = (from igid in publishedImplementationGuides
                                                           where !CheckPoint.Instance.GrantEditImplementationGuide(igid)
                                                           select igid);
            
            return uneditablePublishedImplementationGuides.Count() == 0;
        }
    }

    public static class CodeSystemExtensions
    {
        public static bool CanModify(this CodeSystem codeSystem, IObjectRepository tdb)
        {
            if (codeSystem == null)
                return true;

            var publishedImplementationGuides = (from tc in codeSystem.Constraints
                                                 join t in tdb.Templates on tc.TemplateId equals t.Id
                                                 where t.OwningImplementationGuide.IsPublished()
                                                 select t.OwningImplementationGuide.Id).AsEnumerable();

            return publishedImplementationGuides.Count() == 0;
        }

        public static bool CanOverride(this CodeSystem codeSystem, IObjectRepository tdb)
        {
            if (codeSystem == null)
                return true;

            if (!CheckPoint.Instance.HasSecurables(SecurableNames.TERMINOLOGY_OVERRIDE))
                return false;

            if (CheckPoint.Instance.IsDataAdmin)
                return true;

            var publishedImplementationGuides = (from tc in codeSystem.Constraints  // Published constraints that directly reference the CS
                                                 join t in tdb.Templates on tc.TemplateId equals t.Id
                                                 where t.OwningImplementationGuide.IsPublished()
                                                 select t.OwningImplementationGuideId)
                                                 .Union(        // Valuesets that are bound to a published constraint that use the CS
                                                 from vsm in codeSystem.Members
                                                 join tc in tdb.TemplateConstraints on vsm.ValueSetId equals tc.ValueSetId
                                                 join t in tdb.Templates on tc.TemplateId equals t.Id
                                                 where t.OwningImplementationGuide.IsPublished()
                                                 select t.OwningImplementationGuideId).AsEnumerable();

            var uneditablePublishedImplementationGuides = (from igid in publishedImplementationGuides
                                                           where !CheckPoint.Instance.GrantEditImplementationGuide(igid)
                                                           select igid);

            return uneditablePublishedImplementationGuides.Count() == 0;
        }
    }
}
