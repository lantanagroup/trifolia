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

        private void PopulateIdentifier(ValueSet valueSet, FhirValueSet fhirValueSet)
        {
            var existingIdentifiers = valueSet.Identifiers.Where(y => y.Type == IdentifierTypes.HTTP).ToList();

            // Remove HTTP identifiers that are not found in the FHIR ValueSet's identifiers
            for (var i = existingIdentifiers.Count - 1; i >= 0; i--)
            {
                var existingIdentifier = existingIdentifiers[i];

                if (!fhirValueSet.Identifier.Any(y => y.Value.ToLower().Trim() == existingIdentifier.Identifier.ToLower().Trim()))
                    this.tdb.ValueSetIdentifiers.Remove(existingIdentifier);
            }

            // Add identifiers from the FHIR ValueSet's identifiers that don't already exist
            foreach (var fhirIdentifer in fhirValueSet.Identifier)
            {
                if (!existingIdentifiers.Any(y => y.Identifier.ToLower().Trim() == fhirIdentifer.Value.ToLower().Trim()))
                {
                    valueSet.Identifiers.Add(new ValueSetIdentifier()
                    {
                        Type = IdentifierTypes.HTTP,
                        Identifier = fhirIdentifer.Value
                    });
                }
            }
        }

        public ValueSet Convert(FhirValueSet fhirValueSet, ValueSet valueSet = null)
        {
            string fhirDescription = fhirValueSet.Description != null ? fhirValueSet.Description.Value : null;

            if (valueSet == null)
                valueSet = new ValueSet();

            if (valueSet.Name != fhirValueSet.Name)
                valueSet.Name = fhirValueSet.Name;

            if (valueSet.Description != fhirDescription)
                valueSet.Description = fhirDescription;

            if (fhirValueSet.Identifier == null)
                throw new Exception("ValueSet.identifier.value is required");

            this.PopulateIdentifier(valueSet, fhirValueSet);

            if (fhirValueSet.Expansion != null)
            {
                foreach (var expContains in fhirValueSet.Expansion.Contains)
                {
                    // Skip members that don't have a code or a code system
                    if (string.IsNullOrEmpty(expContains.Code) || string.IsNullOrEmpty(expContains.System))
                        continue;

                    CodeSystem codeSystem = (from cs in this.tdb.CodeSystems
                                             join csi in this.tdb.CodeSystemIdentifiers on cs.Id equals csi.CodeSystemId
                                             where csi.Identifier == expContains.System
                                             select cs)
                                            .FirstOrDefault();

                    if (codeSystem == null)
                    {
                        codeSystem = new CodeSystem(expContains.System);
                        codeSystem.Identifiers.Add(new CodeSystemIdentifier(expContains.System));

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
