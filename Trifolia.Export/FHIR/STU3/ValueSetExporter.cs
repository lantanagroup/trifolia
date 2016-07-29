extern alias fhir_stu3;
using fhir_stu3.Hl7.Fhir.Model;
using System;
using System.Linq;
using Trifolia.DB;
using FhirValueSet = fhir_stu3.Hl7.Fhir.Model.ValueSet;
using ValueSet = Trifolia.DB.ValueSet;
using CodeSystem = Trifolia.DB.CodeSystem;
using Trifolia.Shared.FHIR;

namespace Trifolia.Export.FHIR.STU3
{
    public class ValueSetExporter
    {
        private IObjectRepository tdb;

        public ValueSetExporter(IObjectRepository tdb)
        {
            this.tdb = tdb;
        }

        public FhirValueSet Convert(ValueSet valueSet, SummaryType? summaryType = null)
        {
            var implementationGuides = (from tc in valueSet.Constraints
                                        join t in this.tdb.Templates on tc.TemplateId equals t.Id
                                        select t.OwningImplementationGuide);
            bool usedByPublishedIgs = implementationGuides.Count(y => y.PublishStatus.IsPublished) > 0;

            FhirValueSet fhirValueSet = new FhirValueSet()
            {
                Id = valueSet.Id.ToString(),
                Name = valueSet.Name,
                Status = usedByPublishedIgs ? ConformanceResourceStatus.Active : ConformanceResourceStatus.Draft,
                Description = new Markdown(valueSet.Description),
                Url = valueSet.Oid
            };

            if (summaryType == null || summaryType == SummaryType.Data)
            {
                var activeMembers = valueSet.GetActiveMembers(DateTime.Now);

                if (activeMembers.Count > 0)
                {
                    // Compose
                    var compose = new FhirValueSet.ComposeComponent();
                    fhirValueSet.Compose = compose;

                    foreach (var groupedMember in activeMembers.GroupBy(y => y.CodeSystem, y => y))
                    {
                        var include = new FhirValueSet.ConceptSetComponent();
                        compose.Include.Add(include);

                        include.System = groupedMember.Key.Oid;

                        foreach (var member in groupedMember)
                        {
                            include.Concept.Add(new FhirValueSet.ConceptReferenceComponent()
                            {
                                Code = member.Code,
                                Display = member.DisplayName
                            });
                        }
                    }

                    // Expansion
                    var expansion = new FhirValueSet.ExpansionComponent();
                    expansion.Identifier = string.Format("urn:uuid:{0}", Guid.NewGuid());
                    expansion.Timestamp = FhirDateTime.Now().ToString();
                    fhirValueSet.Expansion = expansion;

                    foreach (ValueSetMember vsMember in activeMembers)
                    {
                        var fhirMember = new FhirValueSet.ContainsComponent()
                        {
                            System = STU3Helper.FormatIdentifier(vsMember.CodeSystem.Oid),
                            Code = vsMember.Code,
                            Display = vsMember.DisplayName
                        };

                        expansion.Contains.Add(fhirMember);
                    }
                }
            }

            return fhirValueSet;
        }
    }
}
