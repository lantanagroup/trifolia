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
        protected abstract T BaseFindValueSet(IObjectRepository tdb, string oid);

        #region IValueSetImportProcessor Implementation

        public T FindValueSet(IObjectRepository tdb, string oid)
        {
            return this.BaseFindValueSet(tdb, oid);
        }

        public void SaveValueSet(IObjectRepository tdb, ImportValueSet valueSet)
        {
            ValueSet tdbValueSet = FindOrAddValueSet(tdb, valueSet);

            foreach (ImportValueSetMember cImportMember in valueSet.Members)
            {
                FindOrAddValueSetMember(tdb, tdbValueSet, cImportMember);
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
                    importValueSet.Name != currentValueSet.Name ||
                    importValueSet.ImportSourceId != currentValueSet.ImportSourceId;

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
                    importValueSetMember.CodeSystemOid != currentMember.CodeSystem.Oid ||
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
            ValueSetIdentifierTypes type = ValueSetIdentifierTypes.Oid;

            if (identifier.StartsWith("http://") || identifier.StartsWith("https://"))
                type = ValueSetIdentifierTypes.HTTP;
            else if (identifier.StartsWith("urn:hl7ii:"))
                type = ValueSetIdentifierTypes.HL7II;

            ValueSetIdentifier vsIdentifier = valueSet.Identifiers.FirstOrDefault(y => y.Type == type);

            if (vsIdentifier == null)
            {
                vsIdentifier = new ValueSetIdentifier()
                {
                    Type = type,
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

        private ValueSet FindOrAddValueSet(IObjectRepository tdb, ImportValueSet valueSet)
        {
            ValueSet foundValueSet = (from vs in tdb.ValueSets
                                      join vsi in tdb.ValueSetIdentifiers on vs.Id equals vsi.ValueSetId
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
                tdb.ValueSets.Add(foundValueSet);
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

            if (valueSet.ImportSource == "PHIN VADS")
            {
                if (foundValueSet.ImportSource.HasValue && foundValueSet.ImportSource != ValueSetImportSources.PHINVADS)
                    throw new Exception("Cannot re-import this value set as it was imported from a different source.");

                if (!foundValueSet.ImportSource.HasValue)
                {
                    foundValueSet.ImportSource = ValueSetImportSources.PHINVADS;
                    changed = true;
                }
            }

            if (foundValueSet.ImportSourceId != valueSet.ImportSourceId)
            {
                foundValueSet.ImportSourceId = valueSet.ImportSourceId;
                changed = true;
            }

            if (changed)
                foundValueSet.LastUpdate = DateTime.Now;

            return foundValueSet;
        }

        private void FindOrAddValueSetMember(IObjectRepository tdb, ValueSet tdbValueSet, ImportValueSetMember valueSetMember)
        {
            if (valueSetMember.ImportStatus == "None")
                return; 
            
            ValueSetMember foundValueSetMember = tdbValueSet.Members.SingleOrDefault(y => y.Code == valueSetMember.Code && y.CodeSystem.Oid == valueSetMember.CodeSystemOid);
            CodeSystem codeSystem = FindOrAddCodeSystem(tdb, valueSetMember.CodeSystemOid, valueSetMember.CodeSystemName);

            string code = TruncateString(valueSetMember.Code, 254);
            string displayName = TruncateString(valueSetMember.DisplayName, 254);

            if (foundValueSetMember == null)
            {
                foundValueSetMember = new ValueSetMember();
                foundValueSetMember.ValueSet = tdbValueSet;
                tdb.ValueSetMembers.Add(foundValueSetMember);
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

        private Trifolia.DB.CodeSystem FindOrAddCodeSystem(IObjectRepository tdb, string codeSystemOid, string codeSystemName)
        {
            Trifolia.DB.CodeSystem foundCodeSystem = addedCodeSystems.SingleOrDefault(y => y.Oid == codeSystemOid);

            // If we haven't added the code system as part of this save, search the database for the code system
            if (foundCodeSystem == null)
                foundCodeSystem = tdb.CodeSystems.FirstOrDefault(y => y.Oid == codeSystemOid);

            // If no code system was found that we added recently, and it was not found in the database, create a new one
            if (foundCodeSystem == null)
            {
                foundCodeSystem = new CodeSystem()
                {
                    Oid = codeSystemOid,
                    Name = codeSystemName,
                    LastUpdate = DateTime.Now
                };

                tdb.CodeSystems.Add(foundCodeSystem);
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
