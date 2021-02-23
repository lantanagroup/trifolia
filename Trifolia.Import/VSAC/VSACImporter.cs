using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Trifolia.DB;
using Trifolia.Shared;

namespace Trifolia.Import.VSAC
{
    public class VSACImporter
    {
        private const string ST_URL_FORMAT = "https://vsac.nlm.nih.gov/vsac/ws/Ticket/{0}";
        private const string SVS_RETRIEVE_VALUE_SET_URL_FORMAT = "https://vsac.nlm.nih.gov/vsac/svs/RetrieveMultipleValueSets?id={0}&ticket={1}";
        private const string VSAC_SOURCE_URL_FORMAT = "https://vsac.nlm.nih.gov/valueset/{0}/expansion";
        private const string SVS_NS = "urn:ihe:iti:svs:2008";

        private IObjectRepository tdb;
        private string ticketGrantingTicket = string.Empty;

        public VSACImporter(IObjectRepository tdb)
        {
            this.tdb = tdb;
        }

        public bool Authenticate(string apiKey)
        {
            string tgt = UmlsHelper.GetTicketGrantingTicket(apiKey);
            this.ticketGrantingTicket = tgt;
            return !string.IsNullOrEmpty(this.ticketGrantingTicket);
        }

        /// <summary>
        /// Retrieves the specified value set from the VSAC and imports the value set into Trifolia.
        /// If the value set already exists, it is updated with the latest name and codes from VSAC.
        /// </summary>
        /// <remarks>Requires Authenticate() to be called first.</remarks>
        /// <param name="oid">The oid of the value set to retrieve. Should not include any prefix (i.e. "urn:oid:")</param>
        public bool ImportValueSet(string oid)
        {
            string serviceTicket = UmlsHelper.GetServiceTicket(this.ticketGrantingTicket);

            if (string.IsNullOrEmpty(serviceTicket)) return false;

            string url = string.Format(SVS_RETRIEVE_VALUE_SET_URL_FORMAT, oid, serviceTicket);
            HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create(url);
            HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse();

            if (response.StatusCode == HttpStatusCode.OK)
            {
                using (StreamReader sr = new StreamReader(response.GetResponseStream()))
                {
                    string responseBody = sr.ReadToEnd();
                    return this.ImportRetrieveValueSet(responseBody);
                }
            }

            return false;
        }

        private bool ImportRetrieveValueSet(string retrieveValueSetResponse)
        {
            List<CodeSystem> codeSystems = this.tdb.CodeSystems.ToList();
            int importCount = 0;

            XmlDocument doc = new XmlDocument();
            XmlNamespaceManager nsManager = new XmlNamespaceManager(doc.NameTable);
            nsManager.AddNamespace("svs", SVS_NS);

            try
            {
                doc.LoadXml(retrieveValueSetResponse);
            }
            catch (Exception ex)
            {
                Logging.Log.For(this).Error("Error parsing response from VSAC: " + ex.Message);
                throw ex;
            }

            var svsValueSetNodes = doc.SelectNodes("/svs:RetrieveMultipleValueSetsResponse/svs:DescribedValueSet", nsManager);

            if (svsValueSetNodes.Count > 1)
            {
                string msg = "Found " + svsValueSetNodes.Count + " value sets in results. Not continuing.";
                Logging.Log.For(this).Error(msg);
                throw new Exception(msg);
            }

            foreach (XmlElement svsValueSetNode in svsValueSetNodes)
            {
                string svsValueSetId = svsValueSetNode.Attributes["ID"].Value;

                Logging.Log.For(this).Debug("Parsing VSAC value set " + svsValueSetId);

                string svsValueSetVersion = svsValueSetNode.Attributes["version"].Value;
                string identifier = string.Format("urn:oid:{0}", svsValueSetId);
                string name = svsValueSetNode.Attributes["displayName"].Value;
                var purposeNode = svsValueSetNode.SelectSingleNode("svs:Purpose", nsManager);
                string description = purposeNode != null ? purposeNode.InnerText : string.Empty;
                string source = string.Format(VSAC_SOURCE_URL_FORMAT, svsValueSetId);

                description += string.Format("{0}This value set was imported on {1} with a version of {2}.", 
                    description.Length > 0 ? "\n\n" : string.Empty,
                    DateTime.Now.ToShortDateString(), 
                    svsValueSetVersion);

                if (!string.IsNullOrEmpty(description))
                {
                    if (description.StartsWith("(") && description.EndsWith(")"))
                        description = description.Substring(1, description.Length - 2).Trim();
                }

                ValueSet foundValueSet = (from vs in this.tdb.ValueSets
                                          join vsi in this.tdb.ValueSetIdentifiers on vs.Id equals vsi.ValueSetId
                                          where vsi.Type == ValueSetIdentifierTypes.Oid && vsi.Identifier.Trim().ToLower() == identifier.Trim().ToLower()
                                          select vs)
                                          .FirstOrDefault();

                if (foundValueSet == null)
                {
                    Logging.Log.For(this).Debug("No existing value set with the same identifier found, creating a new one");

                    foundValueSet = new ValueSet();
                    foundValueSet.LastUpdate = DateTime.Now;
                    this.tdb.ValueSets.Add(foundValueSet);

                    foundValueSet.Identifiers.Add(new ValueSetIdentifier()
                    {
                        Type = ValueSetIdentifierTypes.Oid,
                        Identifier = identifier,
                        IsDefault = true
                    });
                }
                else if (foundValueSet.ImportSource.HasValue && foundValueSet.ImportSource != ValueSetImportSources.VSAC)
                {
                    string msg = "This value set was imported from another source. It cannot be re-imported from a different source.";
                    Logging.Log.For(this).Error(msg);
                    throw new Exception(msg);
                }
                else
                {
                    Logging.Log.For(this).Debug("Found already-existing value set with identifier " + identifier + ". Going to update it.");
                }

                if (foundValueSet.Name != name)
                    foundValueSet.Name = name;

                if (foundValueSet.ImportSource != ValueSetImportSources.VSAC)
                    foundValueSet.ImportSource = ValueSetImportSources.VSAC;

                if (foundValueSet.ImportSourceId != svsValueSetId)
                    foundValueSet.ImportSourceId = svsValueSetId;

                if (foundValueSet.Description != description)
                    foundValueSet.Description = description;

                if (foundValueSet.Source != source)
                    foundValueSet.Source = source;

                foundValueSet.Intensional = false;
                foundValueSet.IntensionalDefinition = string.Empty;
                foundValueSet.Code = null;
                foundValueSet.IsIncomplete = false;
                foundValueSet.LastUpdate = DateTime.Now;

                Logging.Log.For(this).Debug("Done setting meta-data properties of value set. Removing existing members from value set first.");

                // Remove all existing codes in the value set so they can be re-added
                this.tdb.ValueSetMembers.RemoveRange(foundValueSet.Members);

                Logging.Log.For(this).Debug(foundValueSet.Members.Count() + " codes already exist in the value set and are being removed to be overwritten");

                // Add all codes to value set
                var svsConceptNodes = svsValueSetNode.SelectNodes("svs:ConceptList/svs:Concept", nsManager);

                Logging.Log.For(this).Debug("Found " + svsConceptNodes.Count + " codes in the VSAC value set. Parsing all codes for import");

                foreach (XmlElement svsConceptNode in svsConceptNodes)
                {
                    string svsCodeSystemOid = "urn:oid:" + svsConceptNode.Attributes["codeSystem"].Value;
                    CodeSystem foundCodeSystem = codeSystems.FirstOrDefault(y => y.Oid.Trim().ToLower() == svsCodeSystemOid.Trim().ToLower());

                    if (foundCodeSystem == null)
                    {
                        foundCodeSystem = new CodeSystem()
                        {
                            Name = svsConceptNode.Attributes["codeSystemName"].Value,
                            Oid = svsCodeSystemOid
                        };
                        this.tdb.CodeSystems.Add(foundCodeSystem);
                        codeSystems.Add(foundCodeSystem);
                    }

                    ValueSetMember svsMember = new ValueSetMember()
                    {
                        Code = svsConceptNode.Attributes["code"].Value,
                        DisplayName = svsConceptNode.Attributes["displayName"].Value,
                        CodeSystem = foundCodeSystem,
                        Status = "active"
                    };

                    if (svsMember.DisplayName != null && svsMember.DisplayName.Length > 1021)
                    {
                        Logging.Log.For(this).Debug("Found code " + svsMember.Code + " has more than 1024 characters for a display name. Truncating.");
                        svsMember.DisplayName = "..." + svsMember.DisplayName.Substring(svsMember.DisplayName.Length - 1021);
                    }

                    foundValueSet.Members.Add(svsMember);
                }

                importCount++;
            }

            return importCount > 0;
        }
    }
}