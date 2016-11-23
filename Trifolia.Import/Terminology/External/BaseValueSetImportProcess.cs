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

        private ValueSet FindOrAddValueSet(IObjectRepository tdb, ImportValueSet valueSet)
        {
            ValueSet foundValueSet = tdb.ValueSets.SingleOrDefault(y => y.Oid == valueSet.Oid);

            if (valueSet.ImportStatus == "None")
                return foundValueSet;

            string name = TruncateString(valueSet.Name, 254);
            string code = TruncateString(valueSet.Code, 254);
            string oid = TruncateString(valueSet.Oid, 254);

            if (foundValueSet == null)
            {
                foundValueSet = new ValueSet();
                tdb.ValueSets.AddObject(foundValueSet);
            }

            if (foundValueSet.Code != code)
                foundValueSet.Code = code;

            if (foundValueSet.Oid != oid)
                foundValueSet.Oid = oid;

            if (foundValueSet.Description != valueSet.Description)
                foundValueSet.Description = valueSet.Description;

            if (foundValueSet.Name != name)
                foundValueSet.Name = name;

            if (foundValueSet.EntityState != System.Data.Entity.EntityState.Unchanged)
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
                tdb.ValueSetMembers.AddObject(foundValueSetMember);
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
                foundCodeSystem = tdb.CodeSystems.SingleOrDefault(y => y.Oid == codeSystemOid);

            // If no code system was found that we added recently, and it was not found in the database, create a new one
            if (foundCodeSystem == null)
            {
                foundCodeSystem = new CodeSystem()
                {
                    Oid = codeSystemOid,
                    Name = codeSystemName,
                    LastUpdate = DateTime.Now
                };

                tdb.CodeSystems.AddObject(foundCodeSystem);
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
