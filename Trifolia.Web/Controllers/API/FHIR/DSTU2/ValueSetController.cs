extern alias fhir_dstu2;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Net.Http.Formatting;
using System.Runtime.Serialization;

using Trifolia.Shared;
using Trifolia.DB;
using Trifolia.Authorization;
using ImplementationGuide = Trifolia.DB.ImplementationGuide;
using ValueSet = Trifolia.DB.ValueSet;
using Trifolia.Web.Formatters.FHIR.DSTU2;
using Trifolia.Generation.XML.FHIR.DSTU2;

using fhir_dstu2.Hl7.Fhir.Model;
using FhirImplementationGuide = fhir_dstu2.Hl7.Fhir.Model.ImplementationGuide;
using FhirValueSet = fhir_dstu2.Hl7.Fhir.Model.ValueSet;
using FhirConformance = fhir_dstu2.Hl7.Fhir.Model.Conformance;
using fhir_dstu2.Hl7.Fhir.Serialization;

namespace Trifolia.Web.Controllers.API.FHIR.DSTU2
{
    [DSTU2Config]
    public class FHIR2ValueSetController : ApiController
    {
        private IObjectRepository tdb;

        #region Constructors

        public FHIR2ValueSetController(IObjectRepository tdb)
        {
            this.tdb = tdb;
        }

        public FHIR2ValueSetController()
            : this(new TemplateDatabaseDataSource())
        {

        }

        #endregion

        private FhirValueSet Convert(ValueSet valueSet, SummaryType? summaryType = null)
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
                Description = valueSet.Description,
                Url = valueSet.Oid
            };

            if (summaryType == null || summaryType == SummaryType.Data)
            {
                var activeMembers = valueSet.GetActiveMembers(DateTime.Now);

                if (activeMembers.Count > 0)
                {
                    // Compose
                    var compose = new FhirValueSet.ValueSetComposeComponent();
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
                    var expansion = new FhirValueSet.ValueSetExpansionComponent();
                    expansion.Identifier = string.Format("urn:uuid:{0}", Guid.NewGuid());
                    expansion.Timestamp = FhirDateTime.Now().ToString();
                    fhirValueSet.Expansion = expansion;

                    foreach (ValueSetMember vsMember in activeMembers)
                    {
                        var fhirMember = new FhirValueSet.ValueSetExpansionContainsComponent()
                        {
                            System = Shared.FormatIdentifier(vsMember.CodeSystem.Oid),
                            Code = vsMember.Code,
                            Display = vsMember.DisplayName
                        };

                        expansion.Contains.Add(fhirMember);
                    }
                }
            }

            return fhirValueSet;
        }

        private ValueSet Convert(FhirValueSet fhirValueSet, ValueSet valueSet = null)
        {
            if (valueSet == null)
                valueSet = new ValueSet();

            if (valueSet.Name != fhirValueSet.Name)
                valueSet.Name = fhirValueSet.Name;

            if (valueSet.Description != fhirValueSet.Description)
                valueSet.Description = fhirValueSet.Description;

            if (fhirValueSet.Identifier == null)
                throw new Exception("ValueSet.identifier.value is required");

            if (valueSet.Oid != fhirValueSet.Identifier.Value)
                valueSet.Oid = fhirValueSet.Identifier.Value;

            if (fhirValueSet.Expansion != null)
            {
                foreach (var expContains in fhirValueSet.Expansion.Contains)
                {
                    // Skip members that don't have a code or a code system
                    if (string.IsNullOrEmpty(expContains.Code) || string.IsNullOrEmpty(expContains.System))
                        continue;

                    CodeSystem codeSystem = this.tdb.CodeSystems.SingleOrDefault(y => y.Oid == expContains.System);

                    if (codeSystem == null)
                    {
                        codeSystem = new CodeSystem()
                        {
                            Oid = expContains.System,
                            Name = expContains.System
                        };
                        this.tdb.CodeSystems.AddObject(codeSystem);
                    }

                    ValueSetMember newMember = valueSet.Members.SingleOrDefault(y => y.CodeSystem == codeSystem && y.Code == expContains.Code);

                    if (newMember == null)
                        newMember = new ValueSetMember()
                        {
                            CodeSystem = codeSystem,
                            Code = expContains.Code
                        };

                    if (newMember.DisplayName != expContains.Display)
                        newMember.DisplayName = expContains.Display;

                    DateTime versionDateVal = DateTime.MinValue;
                    if (!DateTime.TryParse(fhirValueSet.Version, out versionDateVal))
                        DateTime.TryParse(fhirValueSet.Date, out versionDateVal);
                    DateTime? versionDate = versionDateVal != DateTime.MinValue ? (DateTime?)versionDateVal : null;

                    if (newMember.StatusDate != versionDate)
                        newMember.StatusDate = versionDate;

                    if (newMember.StatusDate != null && newMember.Status != "active")
                        newMember.Status = "active";

                    valueSet.Members.Add(newMember);
                }
            }

            return valueSet;
        }

        /// <summary>
        /// Gets the specified value set from Trifolia, converts it to, and returns a ValueSet resource.
        /// </summary>
        /// <param name="valueSetId">The id of the value set to search for. If specified and found, only a single ValueSet resource is returned in the Bundle.</param>
        /// <param name="format">Optional. The format to respond with (ex: "application/fhir+xml" or "application/fhir+json")</param>
        /// <param name="summary">Optional. The type of summary to respond with.</param>
        /// <returns>Hl7.Fhir.Model.ValueSet</returns>
        /// <permission cref="Trifolia.Authorization.SecurableNames.VALUESET_LIST">Only users with the ability to list value sets can execute this operation</permission>
        [HttpGet]
        [Route("api/FHIR2/ValueSet/{valueSetId}")]
        [SecurableAction(SecurableNames.VALUESET_LIST)]
        public HttpResponseMessage GetValueSet(
            [FromUri] int valueSetId,
            [FromUri(Name = "_format")] string format = null,
            [FromUri(Name = "_summary")] SummaryType? summary = null)
        {
            ValueSet valueSet = this.tdb.ValueSets.Single(y => y.Id == valueSetId);
            FhirValueSet fhirValueSet = Convert(valueSet, summary);

            return Shared.GetResponseMessage(this.Request, format, fhirValueSet);
        }

        /// <summary>
        /// Searches value sets within Trifolia and returns them as ValueSet resources within a Bundle.
        /// </summary>
        /// <param name="name">The name of the value set to search for</param>
        /// <param name="valueSetId">The id of the value set to search for. If specified and found, only a single ValueSet resource is returned in the Bundle.</param>
        /// <param name="format">Optional. The format to respond with (ex: "application/fhir+xml" or "application/fhir+json")</param>
        /// <param name="summary">Optional. The type of summary to respond with.</param>
        /// <returns>Hl7.Fhir.Model.Bundle&lt;Hl7.Fhir.Model.ValueSet&gt;</returns>
        /// <permission cref="Trifolia.Authorization.SecurableNames.VALUESET_LIST">Only users with the ability to list value sets can execute this operation</permission>
        [HttpGet]
        [Route("api/FHIR2/ValueSet")]
        [Route("api/FHIR2/ValueSet/_search")]
        [SecurableAction]
        public HttpResponseMessage GetValueSets(
            [FromUri(Name = "_id")] int? valueSetId = null,
            [FromUri(Name = "name")] string name = null,
            [FromUri(Name = "_format")] string format = null,
            [FromUri(Name = "_summary")] SummaryType? summary = null)
        {
            var valueSets = this.tdb.ValueSets.Where(y => y.Id >= 0);

            if (valueSetId != null)
                valueSets = valueSets.Where(y => y.Id == valueSetId);

            if (!string.IsNullOrEmpty(name))
                valueSets = valueSets.Where(y => y.Name.ToLower().Contains(name.ToLower()));

            Bundle bundle = new Bundle()
            {
                Type = Bundle.BundleType.BatchResponse
            };

            foreach (var valueSet in valueSets)
            {
                var fhirValueSet = Convert(valueSet, summary);
                var fullUrl = string.Format("{0}://{1}/api/FHIR2/{2}",
                    this.Request.RequestUri.Scheme,
                    this.Request.RequestUri.Authority,
                    fhirValueSet.Url);

                bundle.AddResourceEntry(fhirValueSet, fullUrl);
            }

            return Shared.GetResponseMessage(this.Request, format, bundle);
        }

        [HttpPost]
        [Route("api/FHIR2/ValueSet")]
        [SecurableAction(SecurableNames.VALUESET_EDIT)]
        public HttpResponseMessage CreateValueSet(
            [FromBody] FhirValueSet fhirValueSet,
            [FromUri(Name = "_format")] string format = null)
        {
            if (fhirValueSet.Identifier != null && this.tdb.ValueSets.Count(y => y.Oid == fhirValueSet.Identifier.Value) > 0)
                throw new Exception("ValueSet already exists with this identifier. Use a PUT instead");

            ValueSet valueSet = Convert(fhirValueSet);

            if (valueSet.Oid == null)
                valueSet.Oid = string.Empty;

            if (this.tdb.ValueSets.Count(y => y.Oid == valueSet.Oid) > 0)
                throw new Exception("A ValueSet with that identifier already exists");

            this.tdb.ValueSets.AddObject(valueSet);
            this.tdb.SaveChanges();

            string location = string.Format("{0}://{1}/api/FHIR2/ValueSet/{2}",
                    this.Request.RequestUri.Scheme,
                    this.Request.RequestUri.Authority,
                    fhirValueSet.Id);

            Dictionary<string, string> headers = new Dictionary<string, string>();
            headers.Add("Location", location);

            FhirValueSet createdFhirValueSet = Convert(valueSet);
            return Shared.GetResponseMessage(this.Request, format, createdFhirValueSet, statusCode: 201, headers: headers);
        }

        [HttpPut]
        [Route("api/FHIR2/ValueSet/{valueSetId}")]
        [SecurableAction(SecurableNames.VALUESET_EDIT)]
        public HttpResponseMessage UpdateValueSet(
            [FromBody] FhirValueSet fhirValueSet,
            [FromUri] int valueSetId,
            [FromUri(Name = "_format")] string format = null)
        {
            ValueSet originalValueSet = this.tdb.ValueSets.Single(y => y.Id == valueSetId);
            ValueSet newValueSet = Convert(fhirValueSet, valueSet: originalValueSet);

            if (originalValueSet == null)
                this.tdb.ValueSets.AddObject(newValueSet);

            this.tdb.SaveChanges();

            string location = string.Format("{0}://{1}/api/FHIR2/ValueSet/{2}",
                    this.Request.RequestUri.Scheme,
                    this.Request.RequestUri.Authority,
                    fhirValueSet.Id);

            Dictionary<string, string> headers = new Dictionary<string, string>();
            headers.Add("Location", location);

            FhirValueSet updatedFhirValueSet = Convert(newValueSet);
            return Shared.GetResponseMessage(this.Request, format, updatedFhirValueSet, originalValueSet != null ? 200 : 201, headers);
        }
    }
}
