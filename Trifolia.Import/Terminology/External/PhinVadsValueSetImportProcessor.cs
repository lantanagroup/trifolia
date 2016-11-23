using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Trifolia.Shared;
using Trifolia.Shared.ImportExport.Model;
using Trifolia.DB;

using VadsClient;
using Trifolia.Config;

namespace Trifolia.Import.Terminology.External
{
    public class PhinVadsValueSetImportProcessor<T,V> : BaseValueSetImportProcess<T,V>
            where T : ImportValueSet
            where V : ImportValueSetMember
    {
        private const int MEMBER_PAGE_SIZE = 5000;

        protected override T BaseFindValueSet(IObjectRepository tdb, string oid)
        {
            hessiancsharp.client.CHessianProxyFactory factory = new hessiancsharp.client.CHessianProxyFactory();
            VocabService vocabService = (VocabService)factory.Create(typeof(VocabService), AppSettings.PhinVadsServiceUrl);
            ValueSetResultDto valueSetResults = null;

            try
            {
                valueSetResults = vocabService.getValueSetByOid(oid);

                if (!string.IsNullOrEmpty(valueSetResults.errorText))
                    throw new ExternalSourceConnectionException();

                if (valueSetResults == null || valueSetResults.totalResults != 1)
                    return null;

                VadsClient.ValueSet valueSetResult = valueSetResults.valueSet[0];
                Trifolia.DB.ValueSet currentValueSet = tdb.ValueSets.SingleOrDefault(y => y.Oid == oid);

                T importValueSet = CreateImportValueSet(currentValueSet, valueSetResult);

                ValueSetVersionResultDto versionResults = vocabService.getValueSetVersionsByValueSetOid(oid);
                DateTime latestVersionDate = versionResults.valueSetVersions.Max(y => y.effectiveDate);
                ValueSetVersion latestVersion = versionResults.valueSetVersions.Single(y => y.effectiveDate == latestVersionDate);

                int cPage = 1;
                ValueSetConceptResultDto valueSetConceptResults = vocabService.getValueSetConceptsByValueSetVersionId(latestVersion.id, cPage, MEMBER_PAGE_SIZE);
                Dictionary<string, string> codeSystemNames = new Dictionary<string, string>();

                while (valueSetConceptResults != null && valueSetConceptResults.valueSetConcepts != null && valueSetConceptResults.valueSetConcepts.Count > 0)
                {
                    foreach (ValueSetConcept cValueSetConceptResult in valueSetConceptResults.valueSetConcepts)
                    {
                        V importValueSetMember = CreateImportValueSetMember(currentValueSet, cValueSetConceptResult);
                        importValueSet.Members.Add(importValueSetMember);

                        // Determine the code system name
                        if (codeSystemNames.ContainsKey(importValueSetMember.CodeSystemOid))
                        {
                            importValueSetMember.CodeSystemName = codeSystemNames[importValueSetMember.CodeSystemOid];
                        }
                        else
                        {
                            CodeSystemResultDto codeSystemResults = vocabService.getCodeSystemByOid(importValueSetMember.CodeSystemOid);
                            string codeSystemName = codeSystemResults.codeSystems[0].name;

                            importValueSetMember.CodeSystemName = codeSystemName;
                            codeSystemNames.Add(importValueSetMember.CodeSystemOid, codeSystemName);
                        }
                    }

                    valueSetConceptResults = vocabService.getValueSetConceptsByValueSetVersionId(latestVersion.id, ++cPage, MEMBER_PAGE_SIZE);
                }

                return importValueSet;
            }
            catch (hessiancsharp.io.CHessianException che)
            {
                if (che.Message.Contains("404"))
                    return null;

                throw;
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Unable to connect to the remote server"))
                    throw new ExternalSourceConnectionException();

                throw;
            }
        }

        #region Private methods for initializing the models

        private T CreateImportValueSet(Trifolia.DB.ValueSet currentValueSet, VadsClient.ValueSet vadsValueSet)
        {
            T importValueSet = Activator.CreateInstance<T>();
            
            importValueSet.Oid = vadsValueSet.oid;
            importValueSet.Code = vadsValueSet.code;
            importValueSet.Description = vadsValueSet.definitionText;
            importValueSet.Name = vadsValueSet.name;
            importValueSet.ImportSource = "PHIN VADS";
            importValueSet.ImportStatus = DetermineValueSetStatus(importValueSet, currentValueSet);

            return importValueSet;
        }

        private V CreateImportValueSetMember(Trifolia.DB.ValueSet currentValueSet, VadsClient.ValueSetConcept vadsConcept)
        {
            ValueSetMember currentMember = currentValueSet != null ? 
                currentValueSet.Members.SingleOrDefault(y => y.Code == vadsConcept.conceptCode && y.CodeSystem.Oid == vadsConcept.codeSystemOid) :
                null;

            V importValueSetMember = Activator.CreateInstance<V>();

            importValueSetMember.Code = vadsConcept.conceptCode;
            importValueSetMember.CodeSystemOid = vadsConcept.codeSystemOid;
            importValueSetMember.DisplayName = vadsConcept.codeSystemConceptName;
            importValueSetMember.Status = ConvertStatus(vadsConcept.status);
            importValueSetMember.StatusDate = vadsConcept.statusDate;
            importValueSetMember.ImportStatus = DetermineValueSetMemberStatus(importValueSetMember, currentMember);

            return importValueSetMember;
        }

        private string ConvertStatus(string vadsStatus)
        {
            switch (vadsStatus)
            {
                case "Published":
                    return "active";
            }

            string msg = string.Format("Status '{0}' is unknown and cannot be converted.", vadsStatus);
            throw new Exception(msg);
        }

        #endregion
    }
}
