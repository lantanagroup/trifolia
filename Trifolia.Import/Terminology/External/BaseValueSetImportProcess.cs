using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Trifolia.Shared;
using Trifolia.DB;

namespace Trifolia.Import.Terminology.External
{
    public abstract class BaseValueSetImportProcess<T,V> : IValueSetImportProcessor<T,V>
            where T : ImportValueSet
            where V : ImportValueSetMember
    {
        protected IObjectRepository tdb;
        protected abstract T BaseFindValueSet(string oid);

        public BaseValueSetImportProcess(IObjectRepository tdb)
        {
            this.tdb = tdb;
        }

        #region IValueSetImportProcessor Implementation

        public T FindValueSet(string oid)
        {
            return this.BaseFindValueSet(oid);
        }

        public void SaveValueSet(ImportValueSet valueSet)
        {
            ValueSet tdbValueSet = FindOrAddValueSet(valueSet);

            foreach (ImportValueSetMember cImportMember in valueSet.Members)
            {
                FindOrAddValueSetMember(tdbValueSet, cImportMember);
            }
        }

        #endregion

        protected string DetermineValueSetStatus(ImportValueSet importValueSet, ValueSet currentValueSet)
        {
            if (currentValueSet == null)
            {
                return "Add";
            }
            else
            {
                bool valueSetIsChanged =
                    importValueSet.Code != currentValueSet.Code ||
                    importValueSet.Description != currentValueSet.Description ||
                    importValueSet.Name != currentValueSet.Name;

                if (valueSetIsChanged)
                    return "Update";
            }

            return "None";
        }

        protected string DetermineValueSetMemberStatus(ImportValueSetMember importValueSetMember, ValueSetMember currentMember)
        {
            if (currentMember == null)
            {
                return "Add";
            }
            else
            {
                bool valueSetMemberIsChanged =
                    importValueSetMember.Code != currentMember.Code ||
                    importValueSetMember.CodeSystemOid != currentMember.CodeSystem.GetIdentifierValue() ||
                    importValueSetMember.DisplayName != currentMember.DisplayName ||
                    importValueSetMember.Status != currentMember.Status ||
                    importValueSetMember.StatusDate != currentMember.StatusDate;

                if (valueSetMemberIsChanged)
                    return "Update";
            }

            return "None";
        }

        #region Model population

        private bool PopulateIdentifier(ValueSet valueSet, string identifier)
        {
            bool changed = false;
            IdentifierTypes type = IdentifierTypes.Oid;

            if (identifier.StartsWith("http://") || identifier.StartsWith("https://"))
                type = IdentifierTypes.HTTP;

            ValueSetIdentifier vsIdentifier = valueSet.Identifiers.FirstOrDefault(y => y.Type == type);

            if (vsIdentifier == null)
            {
                vsIdentifier = new ValueSetIdentifier()
                {
                    Type = IdentifierTypes.HTTP,
                    Identifier = identifier
                };

                valueSet.Identifiers.Add(vsIdentifier);
                changed = true;
            }

            if (!valueSet.Identifiers.Any(y => y.IsDefault))
            {
                vsIdentifier.IsDefault = true;
                changed = true;
            }

            return changed;
        }

        private ValueSet FindOrAddValueSet(ImportValueSet valueSet)
        {
            ValueSet foundValueSet = (from vs in this.tdb.ValueSets
                                      join vsi in this.tdb.ValueSetIdentifiers on vs.Id equals vsi.ValueSetId
                                      where vsi.Identifier.ToLower().Trim() == valueSet.Oid.ToLower().Trim()
                                      select vs)
                                      .Distinct()
                                      .FirstOrDefault();

            if (valueSet.ImportStatus == "None")
                return foundValueSet;

            bool changed = false;
            string name = TruncateString(valueSet.Name, 254);
            string code = TruncateString(valueSet.Code, 254);
            string oid = TruncateString(valueSet.Oid, 254);

            if (foundValueSet == null)
            {
                foundValueSet = new ValueSet();
                this.tdb.ValueSets.Add(foundValueSet);
                changed = true;
            }

            if (foundValueSet.Code != code)
            {
                foundValueSet.Code = code;
                changed = true;
            }

            if (this.PopulateIdentifier(foundValueSet, oid))
                changed = true;

            if (foundValueSet.Description != valueSet.Description)
            {
                foundValueSet.Description = valueSet.Description;
                changed = true;
            }

            if (foundValueSet.Name != name)
            {
                foundValueSet.Name = name;
                changed = true;
            }

            if (changed)
                foundValueSet.LastUpdate = DateTime.Now;

            return foundValueSet;
        }

        private void FindOrAddValueSetMember(ValueSet tdbValueSet, ImportValueSetMember valueSetMember)
        {
            if (valueSetMember.ImportStatus == "None")
                return;

            ValueSetMember foundValueSetMember = null;
            CodeSystem codeSystem = FindOrAddCodeSystem(valueSetMember.CodeSystemOid, valueSetMember.CodeSystemName);

            foundValueSetMember = (from vsm in tdbValueSet.Members
                                   join csi in this.tdb.CodeSystemIdentifiers on vsm.CodeSystemId equals csi.CodeSystemId
                                   where vsm.Code.Trim().ToLower() == valueSetMember.Code.Trim().ToLower() && csi.Identifier.Trim().ToLower() == valueSetMember.CodeSystemOid.Trim().ToLower()
                                   select vsm)
                                  .Distinct()
                                  .FirstOrDefault();

            string code = TruncateString(valueSetMember.Code, 254);
            string displayName = TruncateString(valueSetMember.DisplayName, 254);

            if (foundValueSetMember == null)
            {
                foundValueSetMember = new ValueSetMember();
                foundValueSetMember.ValueSet = tdbValueSet;
                this.tdb.ValueSetMembers.Add(foundValueSetMember);
            }

            if (foundValueSetMember.Code != code)
                foundValueSetMember.Code = code;

            if (foundValueSetMember.DisplayName != displayName)
                foundValueSetMember.DisplayName = displayName;

            if (foundValueSetMember.CodeSystem != codeSystem)
            {
                foundValueSetMember.CodeSystem = codeSystem;
                foundValueSetMember.CodeSystemId = codeSystem.Id;
            }

            if (foundValueSetMember.Status != valueSetMember.Status)
                foundValueSetMember.Status = valueSetMember.Status;

            if (foundValueSetMember.StatusDate != valueSetMember.StatusDate)
                foundValueSetMember.StatusDate = valueSetMember.StatusDate;
        }

        private List<Trifolia.DB.CodeSystem> addedCodeSystems = new List<CodeSystem>();

        private Trifolia.DB.CodeSystem FindOrAddCodeSystem(string codeSystemOid, string codeSystemName)
        {
            Trifolia.DB.CodeSystem foundCodeSystem = (from acs in addedCodeSystems
                                                      join acsi in addedCodeSystems.SelectMany(y => y.Identifiers) on acs equals acsi.CodeSystem
                                                      where acsi.Identifier.Trim().ToLower() == codeSystemOid.Trim().ToLower()
                                                      select acs)
                                                      .Distinct()
                                                      .FirstOrDefault();

            // If we haven't added the code system as part of this save, search the database for the code system
            if (foundCodeSystem == null)
            {
                foundCodeSystem = (from cs in this.tdb.CodeSystems
                                   join csi in this.tdb.CodeSystemIdentifiers on cs.Id equals csi.CodeSystemId
                                   where csi.Identifier.Trim().ToLower() == codeSystemOid.Trim().ToLower()
                                   select cs)
                                   .Distinct()
                                   .FirstOrDefault();
            }

            // If no code system was found that we added recently, and it was not found in the database, create a new one
            if (foundCodeSystem == null)
            {
                foundCodeSystem = new CodeSystem(codeSystemName)
                {
                    LastUpdate = DateTime.Now
                };
                foundCodeSystem.Identifiers.Add(new CodeSystemIdentifier(codeSystemOid));

                this.tdb.CodeSystems.Add(foundCodeSystem);
                addedCodeSystems.Add(foundCodeSystem);
            }

            return foundCodeSystem;
        }

        private string TruncateString(string value, int maxLength)
        {
            if (!string.IsNullOrEmpty(value) && value.Length > maxLength)
                return value.Substring(0, maxLength);

            return value;
        }

        #endregion
    }
}
