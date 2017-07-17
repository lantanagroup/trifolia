using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trifolia.DB;
using Trifolia.Shared.ImportExport.Model;

namespace Trifolia.Import.Native
{
    public class TerminologyImporter
    {
        private IObjectRepository tdb;
        private List<CodeSystem> addedCodeSystems = new List<CodeSystem>();
        private List<ValueSet> addedValueSets = new List<ValueSet>();

        public List<CodeSystem> AddedCodeSystems
        {
            get { return this.addedCodeSystems; }
        }

        public List<ValueSet> AddedValueSets
        {
            get { return this.addedValueSets; }
        }

        public TerminologyImporter(IObjectRepository tdb)
        {
            this.tdb = tdb;
        }

        public void ImportCodeSystems(List<TrifoliaCodeSystem> models)
        {
            var allCodeSystemIdentifiers = this.tdb.CodeSystems.Select(y => y.Oid);

            foreach (var model in models)
            {
                // Don't import code systems without an identifier. Don't import code systems that already exists, they may be in use by other implementation guides that are published.
                if (model.Identifier == null || string.IsNullOrEmpty(model.Identifier.value) || allCodeSystemIdentifiers.Contains(model.Identifier.value))
                    continue;

                CodeSystem newCodeSystem = new CodeSystem()
                {
                    Name = model.name,
                    Description = model.Description,
                    LastUpdate = DateTime.Now,
                    Oid = model.Identifier.value
                };

                this.tdb.CodeSystems.Add(newCodeSystem);
                this.addedCodeSystems.Add(newCodeSystem);
            }
        }

        public void ImportValueSets(List<TrifoliaValueSet> models)
        {
            var codeSystems = this.tdb.CodeSystems.ToList().Union(this.addedCodeSystems);
            var allValueSetIdentifiers = (from vs in this.tdb.ValueSets
                                          join vsi in this.tdb.ValueSetIdentifiers on vs.Id equals vsi.ValueSetId
                                          select vsi.Identifier).ToList();
            foreach (var model in models)
            {
                // Skip value sets that don't have an identifier
                if (model.Identifier.Count == 0)
                    continue;

                var foundIdentifiers = (from modelIdentifier in model.Identifier
                                        join valueSetIdentifier in allValueSetIdentifiers on modelIdentifier.value.Trim().ToLower() equals valueSetIdentifier.Trim().ToLower()
                                        select modelIdentifier.value);

                // Skip value sets that already exist
                if (foundIdentifiers.Count() > 0)
                    continue;

                ValueSet newValueSet = new ValueSet()
                {
                    Name = model.name,
                    Description = model.Description,
                    IsIncomplete = model.isIncomplete,
                    Source = model.source,
                    Intensional = model.intensional,
                    IntensionalDefinition = model.IntensionalDefinition,
                    LastUpdate = DateTime.Now
                };

                // Add identifiers to the value set
                foreach (var modelIdentifier in model.Identifier)
                {
                    ValueSetIdentifier newValueSetIdentifier = new ValueSetIdentifier()
                    {
                        Identifier = modelIdentifier.value,
                        Type = (ValueSetIdentifierTypes) Enum.Parse(typeof(ValueSetIdentifierTypes), modelIdentifier.type),
                        IsDefault = modelIdentifier.value == model.defaultIdentifier || model.Identifier.Count == 1
                    };

                    newValueSet.Identifiers.Add(newValueSetIdentifier);
                }

                // Add members to the value set
                foreach (var modelMember in model.Member)
                {
                    DateTime statusDate = DateTime.Now;
                    DateTime.TryParse(modelMember.statusDate, out statusDate);

                    string modelIdentifier = modelMember.codeSystemIdentifier;
                    CodeSystem codeSystem = codeSystems.SingleOrDefault(y => y.Oid == modelIdentifier);

                    if (codeSystem == null)
                        continue;

                    ValueSetMember newMember = new ValueSetMember()
                    {
                        Code = modelMember.code,
                        DisplayName = modelMember.displayName,
                        Status = modelMember.status.ToString(),
                        StatusDate = !string.IsNullOrEmpty(modelMember.statusDate) && modelMember.statusSpecified ? statusDate : (DateTime?) null,
                        CodeSystem = codeSystem
                    };

                    newValueSet.Members.Add(newMember);
                }

                this.tdb.ValueSets.Add(newValueSet);
                this.addedValueSets.Add(newValueSet);
            }
        }
    }
}
