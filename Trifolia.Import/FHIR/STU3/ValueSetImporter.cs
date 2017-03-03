extern alias fhir_stu3;
using fhir_stu3.Hl7.Fhir.Model;
using System;
using System.Linq;
using Trifolia.DB;
using FhirValueSet = fhir_stu3.Hl7.Fhir.Model.ValueSet;
using ValueSet = Trifolia.DB.ValueSet;
using CodeSystem = Trifolia.DB.CodeSystem;
using Trifolia.Shared.FHIR;

namespace Trifolia.Import.FHIR.STU3
{
    public class ValueSetImporter
    {
        private IObjectRepository tdb;

        public ValueSetImporter(IObjectRepository tdb)
        {
            this.tdb = tdb;
        }

        public ValueSet Convert(FhirValueSet fhirValueSet, ValueSet valueSet = null)
        {
            string fhirDescription = fhirValueSet.Description != null ? fhirValueSet.Description.Value : null;
            var fhirIdentifier = fhirValueSet.Identifier.Count > 0 ? fhirValueSet.Identifier.First() : null;
            var fhirIdentifierValue = fhirIdentifier != null ? fhirIdentifier.Value : null;

            if (valueSet == null)
                valueSet = new ValueSet();

            if (valueSet.Name != fhirValueSet.Name)
                valueSet.Name = fhirValueSet.Name;

            if (valueSet.Description != fhirDescription)
                valueSet.Description = fhirDescription;

            if (fhirValueSet.Identifier == null)
                throw new Exception("ValueSet.identifier.value is required");

            if (valueSet.Oid != fhirIdentifierValue)
                valueSet.Oid = fhirIdentifierValue;

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
                        this.tdb.CodeSystems.Add(codeSystem);
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
    }
}
