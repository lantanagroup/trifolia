extern alias fhir_stu3;
using fhir_stu3.Hl7.Fhir.Model;
using System;
using System.Linq;
using Trifolia.DB;
using FhirValueSet = fhir_stu3.Hl7.Fhir.Model.ValueSet;
using ValueSet = Trifolia.DB.ValueSet;
using CodeSystem = Trifolia.DB.CodeSystem;
using Trifolia.Shared.FHIR;
using System.Collections;
using System.Collections.Generic;

namespace Trifolia.Export.FHIR.STU3
{
    public class ValueSetExporter
    {
        private IObjectRepository tdb;

        public ValueSetExporter(IObjectRepository tdb)
        {
            this.tdb = tdb;
        }

        /// <summary>
        /// Converts a Trifolia ValueSet model to a FHIR ValueSet model.
        /// </summary>
        /// <param name="valueSet">The Trifolia ValueSet model to convert to a FHIR model</param>
        /// <param name="summaryType">Does not populate certain fields when a summaryType is specified.</param>
        /// <param name="publishedValueSets">Optional list of ValueSets that are used by a published implementation guide. If not specified, queries the database for implementation guides that this value set may be published under.</param>
        /// <returns>A FHIR ValueSet model</returns>
        public FhirValueSet Convert(ValueSet valueSet, SummaryType? summaryType = null, IEnumerable<ValueSet> publishedValueSets = null)
        {
            bool usedByPublishedIgs = false;

            if (publishedValueSets == null)
            {
                var implementationGuides = (from tc in valueSet.Constraints
                                            join t in this.tdb.Templates on tc.TemplateId equals t.Id
                                            select t.OwningImplementationGuide);
                usedByPublishedIgs = implementationGuides.Count(y => y.PublishStatus != null && y.PublishStatus.IsPublished) > 0;
            }
            else
            {
                usedByPublishedIgs = publishedValueSets.Contains(valueSet);
            }

            FhirValueSet fhirValueSet = new FhirValueSet()
            {
                Id = valueSet.GetFhirId(),
                Name = valueSet.Name,
                Status = usedByPublishedIgs ? PublicationStatus.Active : PublicationStatus.Draft,
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
