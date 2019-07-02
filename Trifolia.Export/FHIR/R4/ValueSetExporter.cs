extern alias fhir_r4;
using fhir_r4.Hl7.Fhir.Model;
using System;
using System.Linq;
using Trifolia.DB;
using FhirValueSet = fhir_r4.Hl7.Fhir.Model.ValueSet;
using ValueSet = Trifolia.DB.ValueSet;
using CodeSystem = Trifolia.DB.CodeSystem;
using Trifolia.Shared.FHIR;
using System.Collections;
using System.Collections.Generic;

namespace Trifolia.Export.FHIR.R4
{
    public class ValueSetExporter
    {
        private IObjectRepository tdb;

        public ValueSetExporter(IObjectRepository tdb)
        {
            this.tdb = tdb;
        }

        public Bundle GetImplementationGuideValueSets(Trifolia.DB.ImplementationGuide ig)
        {
            var igValueSets = ig.GetValueSets(this.tdb, readOnly: true);
            Bundle bundle = new Bundle();
            
            foreach (var igValueSet in igValueSets)
            {
                bundle.AddResourceEntry(this.Convert(igValueSet.ValueSet), igValueSet.ValueSet.GetIdentifier(ValueSetIdentifierTypes.HTTP));
            }

            return bundle;
        }

        /// <summary>
        /// Converts a Trifolia ValueSet model to a FHIR ValueSet model.
        /// </summary>
        /// <param name="valueSet">The Trifolia ValueSet model to convert to a FHIR model</param>
        /// <param name="summaryType">Does not populate certain fields when a summaryType is specified.</param>
        /// <param name="publishedValueSets">Optional list of ValueSets that are used by a published implementation guide. If not specified, queries the database for implementation guides that this value set may be published under.</param>
        /// <returns>A FHIR ValueSet model</returns>
        public FhirValueSet Convert(ValueSet valueSet, SummaryType? summaryType = null, IEnumerable<ValueSet> publishedValueSets = null, string baseUrl = null)
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
                Meta = new Meta()
                {
                    ElementId = valueSet.Id.ToString()
                },
                Id = valueSet.GetFhirId(),
                Name = valueSet.Name,
                Status = usedByPublishedIgs ? PublicationStatus.Active : PublicationStatus.Draft,
                Description = new Markdown(valueSet.Description),
                Url = valueSet.GetIdentifier(ValueSetIdentifierTypes.HTTP)
            };

            // Handle urn:oid: and urn:hl7ii: identifiers differently if a base url is provided
            // baseUrl is most likely provided when within the context of an implementation guide
            if (fhirValueSet.Url != null)
            {
                if (fhirValueSet.Url.StartsWith("urn:oid:") && !string.IsNullOrEmpty(baseUrl))
                    fhirValueSet.Url = baseUrl.TrimEnd('/') + "/ValueSet/" + fhirValueSet.Url.Substring(8);
                else if (fhirValueSet.Url.StartsWith("urn:hl7ii:") && !string.IsNullOrEmpty(baseUrl))
                    fhirValueSet.Url = baseUrl.TrimEnd('/') + "/ValueSet/" + fhirValueSet.Url.Substring(10);
            }

            List<ValueSetMember> activeMembers = valueSet.GetActiveMembers(DateTime.Now);

            if (activeMembers.Count > 0)
            {
                // If the value set was created in Trifolia, then Trifolia contains the definition
                // and it should be represented by <compose>
                if (valueSet.ImportSource == null)
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
                }
                else
                {
                    // If the value set was imported, then we have the expansion,
                    // but we don't have the defintion (the <compose>).
                    var expansion = new FhirValueSet.ExpansionComponent();
                    fhirValueSet.Expansion = expansion;
                    expansion.Identifier = fhirValueSet.Url;
                    expansion.Total = activeMembers.Count;

                    if (valueSet.LastUpdate != null)
                        expansion.TimestampElement = new FhirDateTime(valueSet.LastUpdate.Value);
                    else
                        expansion.TimestampElement = new FhirDateTime(DateTime.Now);

                    expansion.Contains = (from m in activeMembers
                                            select new FhirValueSet.ContainsComponent()
                                            {
                                                System = m.CodeSystem.Oid,
                                                Code = m.Code,
                                                Display = m.DisplayName
                                            }).ToList();
                }
            }

            return fhirValueSet;
        }
    }
}
