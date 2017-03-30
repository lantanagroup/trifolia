using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

using Trifolia.Logging;
using Trifolia.DB;

namespace Trifolia.Import.Terminology.External
{
    public class HL7RIMValueSetImportProcessor<T,V> : BaseValueSetImportProcess<T,V>
            where T : ImportValueSet
            where V : ImportValueSetMember
    {
        private XmlDocument sourceDoc;
        private XmlNamespaceManager nsManager;
        private Dictionary<string, List<string>> includedValueSets = new Dictionary<string,List<string>>();

        #region Included Valuesets

        private void AddIncludedValueSet(string valueSetOid, string includedValueSet)
        {
            List<string> inclusions = this.includedValueSets.ContainsKey(valueSetOid) ? this.includedValueSets[valueSetOid] : null;

            if (inclusions == null)
            {
                inclusions = new List<string>();
                this.includedValueSets.Add(valueSetOid, inclusions);
            }

            inclusions.Add(includedValueSet);
        }

        private bool IsAlreadyIncluded(string valueSetOid, string includingValueSet)
        {
            List<string> inclusions = this.includedValueSets.ContainsKey(valueSetOid) ? this.includedValueSets[valueSetOid] : null;

            if (inclusions == null)
                return false;

            return inclusions.Contains(includingValueSet);
        }

        #endregion

        public HL7RIMValueSetImportProcessor(XmlDocument sourceDoc)
        {
            this.sourceDoc = sourceDoc;

            if (this.sourceDoc != null)
            {
                this.nsManager = new XmlNamespaceManager(this.sourceDoc.NameTable);
                this.nsManager.AddNamespace("mif", "urn:hl7-org:v3/mif2");
            }
        }

        protected override T BaseFindValueSet(IObjectRepository tdb, string oid)
        {
            string valueSetXpath = string.Format("/mif:vocabularyModel/mif:valueSet[@id='{0}']", oid);
            XmlNode foundValueSetNode = this.sourceDoc.SelectSingleNode(valueSetXpath, this.nsManager);

            if (foundValueSetNode == null)
                return null;

            ValueSet currentValueSet = (from vs in tdb.ValueSets
                                        join vsi in tdb.ValueSetIdentifiers on vs.Id equals vsi.ValueSetId
                                        where vsi.Identifier.ToLower().Trim() == oid.ToLower().Trim()
                                        select vs)
                                      .Distinct()
                                      .FirstOrDefault();
            T importValueSet = Activator.CreateInstance<T>();

            importValueSet.Name = foundValueSetNode.Attributes["name"].Value;
            importValueSet.Code = foundValueSetNode.Attributes["name"].Value;
            importValueSet.Description = GetValueSetDescription(foundValueSetNode);
            importValueSet.Oid = oid;
            importValueSet.ImportSource = "HL7 RIM/RoseTree";
            importValueSet.ImportStatus = DetermineValueSetStatus(importValueSet, currentValueSet);

            XmlNode currentVersionNode = GetCurrentValueSetVersion(foundValueSetNode);

            PopulateMembers<V>(importValueSet, currentVersionNode, currentValueSet);

            return importValueSet;
        }

        private void PopulateMembers<X>(ImportValueSet importValueSet, XmlNode versionNode, ValueSet currentValueSet)
            where X : ImportValueSetMember
        {
            string includingValueSetName = versionNode.ParentNode.Attributes["name"].Value;

            if (IsAlreadyIncluded(importValueSet.Oid, includingValueSetName))
                return;

            AddIncludedValueSet(importValueSet.Oid, includingValueSetName);

            XmlNodeList contentUnionNodes = versionNode.SelectNodes("mif:content[not(mif:combinedContent)] | mif:content/mif:combinedContent/mif:unionWithContent", this.nsManager);

            DateTime parsedVersionDate;
            DateTime? versionDate = null;

            if (DateTime.TryParse(versionNode.Attributes["versionDate"].Value, out parsedVersionDate))
                versionDate = parsedVersionDate;

            PopulateContentMembers(
                importValueSet,
                versionDate,
                contentUnionNodes,
                currentValueSet);

            XmlNodeList codeSystemContentNodes = versionNode.SelectNodes(
                "mif:content[not(*) and @codeSystem]", this.nsManager);

            PopulateCodeSystemMembers(
                importValueSet,
                versionDate,
                codeSystemContentNodes,
                currentValueSet);

            if (importValueSet.Members.Count == 0)
            {
                string msg = string.Format("Could not find any members for the valueset '{0}' using current logic.", importValueSet.Oid);
                Log.For(this).Critical(msg);
            }
        }

        private void PopulateCodeSystemMembers(ImportValueSet importValueSet, DateTime? versionDate, XmlNodeList contentNodes, ValueSet currentValueSet, string specialization = null)
        {
            foreach (XmlNode cContentNode in contentNodes)
            {
                string codeSystemOid = cContentNode.Attributes["codeSystem"].Value;
                PopulateCodeSystemMembers(importValueSet, versionDate, codeSystemOid, currentValueSet, specialization);
            }
        }

        private void PopulateCodeSystemMembers(ImportValueSet importValueSet, DateTime? versionDate, string codeSystemOid, ValueSet currentValueSet, string specialization = null, string relationshipType = null)
        {
            string codeSystemXpath = string.Format("/mif:vocabularyModel/mif:codeSystem[@codeSystemId='{0}']", codeSystemOid);
            XmlNode codeSystemNode = this.sourceDoc.SelectSingleNode(codeSystemXpath, this.nsManager);

            if (codeSystemNode == null)
                return;

            XmlNodeList conceptNodes = null;

            if (specialization == null)
                conceptNodes = codeSystemNode.SelectNodes("mif:releasedVersion/mif:concept", this.nsManager);
            else
                conceptNodes = codeSystemNode.SelectNodes("mif:releasedVersion/mif:concept[mif:conceptRelationship[@relationshipName='Specializes'][mif:targetConcept/@code='" + specialization + "']]", this.nsManager);

            foreach (XmlNode cConceptNode in conceptNodes)
            {
                V importMember = Activator.CreateInstance<V>();

                XmlNode codeNode = cConceptNode.SelectSingleNode("mif:code", this.nsManager);
                XmlNode printNameNode = cConceptNode.SelectSingleNode("mif:printName", this.nsManager);

                if (codeNode == null || printNameNode == null)
                    continue;

                string code = codeNode.Attributes["code"].Value;

                importMember.Code = code;
                importMember.CodeSystemName = codeSystemNode.Attributes["title"].Value;
                importMember.CodeSystemOid = codeSystemOid;
                importMember.DisplayName = printNameNode.Attributes["text"].Value;
                importMember.Status = codeNode.Attributes["status"].Value;
                importMember.StatusDate = versionDate;

                AddImportMember(importValueSet, versionDate, currentValueSet, importMember, true, relationshipType);
            }
        }

        private void PopulateContentMembers(ImportValueSet importValueSet, DateTime? versionDate, XmlNodeList contentNodes, ValueSet currentValueSet)
        {
            foreach (XmlNode cContentNode in contentNodes)
            {
                string codeSystemOid = cContentNode.Attributes["codeSystem"].Value;
                XmlNodeList codeNodes = cContentNode.SelectNodes("mif:codeBasedContent", this.nsManager);

                foreach (XmlNode cCodeNode in codeNodes)
                {
                    string code = cCodeNode.Attributes["code"].Value;
                    string relationshipType = null;
                    bool includeHead = cCodeNode.Attributes["includeHeadCode"] != null ? bool.Parse(cCodeNode.Attributes["includeHeadCode"].Value) : true;
                    XmlNode includeRelatedCodesNode = cCodeNode.SelectSingleNode("mif:includeRelatedCodes[@relationshipName='Generalizes']", this.nsManager);

                    if (includeRelatedCodesNode != null)
                        relationshipType = includeRelatedCodesNode.Attributes["relationshipTraversal"] != null ? includeRelatedCodesNode.Attributes["relationshipTraversal"].Value : null;

                    V importMember = Activator.CreateInstance<V>();

                    importMember.CodeSystemOid = codeSystemOid;
                    importMember.Code = code;
                    importMember.StatusDate = versionDate;
                    importMember.Status = "active";

                    PopulateConceptFields(importMember);

                    ValueSetMember currentMember = currentValueSet != null ?
                        currentValueSet.Members.SingleOrDefault(y => y.Code == code && y.CodeSystem.Oid == codeSystemOid) :
                        null;

                    importMember.ImportStatus = DetermineValueSetMemberStatus(importMember, currentMember);

                    AddImportMember(importValueSet, versionDate, currentValueSet, importMember, includeHead, relationshipType);
                }
            }
        }

        private void AddImportMember(ImportValueSet importValueSet, DateTime? versionDate, ValueSet currentValueSet, ImportValueSetMember importValueSetMember, bool includeHead, string relationshipType)
        {
            if (includeHead)
            {
                ValueSetMember currentMember = currentValueSet != null ?
                    currentValueSet.Members.SingleOrDefault(y => y.Code == importValueSetMember.Code && y.CodeSystem.Oid == importValueSetMember.CodeSystemOid) :
                    null;

                importValueSetMember.ImportStatus = DetermineValueSetMemberStatus(importValueSetMember, currentMember);

                importValueSet.Members.Add(importValueSetMember);

            }

            if (!string.IsNullOrEmpty(relationshipType))
            {
                PopulateCodeSystemMembers(importValueSet, versionDate, importValueSetMember.CodeSystemOid, currentValueSet, importValueSetMember.Code, relationshipType == "TransitiveClosure" ? "TransitiveClosure" : null);
            }
        }

        private void PopulateConceptFields(ImportValueSetMember importMember)
        {
            string codeSystemXpath = string.Format("/mif:vocabularyModel/mif:codeSystem[@codeSystemId='{0}']", importMember.CodeSystemOid);
            XmlNode codeSystemNode = this.sourceDoc.SelectSingleNode(codeSystemXpath, this.nsManager);

            if (codeSystemNode == null)
                return;

            string conceptXpath = string.Format("mif:releasedVersion/mif:concept[mif:code/@code='{0}']", importMember.Code);
            XmlNode conceptNode = codeSystemNode.SelectSingleNode(conceptXpath, this.nsManager);

            if (conceptNode == null)
                return;

            XmlNode codeNode = conceptNode.SelectSingleNode("mif:code", this.nsManager);
            XmlNode printNameNode = conceptNode.SelectSingleNode("mif:printName", this.nsManager);

            if (codeNode != null)
                importMember.Status = codeNode.Attributes["status"].Value;

            if (printNameNode != null)
                importMember.DisplayName = printNameNode.Attributes["text"].Value;

            importMember.CodeSystemName = codeSystemNode.Attributes["title"].Value;
        }

        private XmlNode GetCurrentValueSetVersion(XmlNode valueSetNode)
        {
            XmlNodeList versionNodes = valueSetNode.SelectNodes("mif:version", this.nsManager);
            DateTime latestDate = DateTime.MinValue;
            XmlNode latestNode = null;

            foreach (XmlNode cVersionNode in versionNodes)
            {
                if (cVersionNode.Attributes["versionDate"] == null)
                    continue;

                DateTime versionDate = DateTime.Parse(cVersionNode.Attributes["versionDate"].Value);

                if (versionDate > latestDate)
                {
                    latestDate = versionDate;
                    latestNode = cVersionNode;
                }
            }

            return latestNode;
        }

        private string GetValueSetDescription(XmlNode valueSetNode)
        {
            XmlNode descriptionNode = valueSetNode.SelectSingleNode("mif:annotations/mif:documentation/mif:description", this.nsManager);

            if (descriptionNode == null)
                return string.Empty;

            string descriptionText = descriptionNode.InnerText;

            return descriptionText;
        }
    }
}
