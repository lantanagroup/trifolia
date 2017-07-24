using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Trifolia.DB;

namespace Trifolia.Import.VSAC
{
    public class VSACImporter
    {
        private const string TGT_URL = "https://vsac.nlm.nih.gov/vsac/ws/Ticket";
        private const string TGT_BODY_FORMAT = "username={0}&password={1}";
        private const string ST_URL_FORMAT = "https://vsac.nlm.nih.gov/vsac/ws/Ticket/{0}";
        private const string SVS_RETRIEVE_VALUE_SET_URL_FORMAT = "https://vsac.nlm.nih.gov/vsac/svs/RetrieveMultipleValueSets?id={0}&ticket={1}";
        private const string SVS_NS = "urn:ihe:iti:svs:2008";

        private IObjectRepository tdb;
        private string ticketGrantingTicket = string.Empty;

        public VSACImporter(IObjectRepository tdb)
        {
            this.tdb = tdb;
        }

        /// <summary>
        /// Retrieves the specified value set from the VSAC and imports the value set into Trifolia.
        /// If the value set already exists, it is updated with the latest name and codes from VSAC.
        /// </summary>
        /// <remarks>Requires Authenticate() to be called first.</remarks>
        /// <param name="oid">The oid of the value set to retrieve. Should not include any prefix (i.e. "urn:oid:")</param>
        public bool ImportValueSet(string oid)
        {
            string serviceTicket = this.GetServiceTicket();
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

        /// <summary>
        /// Authenticates the user with the VSAC using the credentials specified.
        /// </summary>
        /// <param name="username">The VSAC username</param>
        /// <param name="password">The VSAC password</param>
        /// <returns>True if authenticated, otherwise false.</returns>
        public bool Authenticate(string username, string password)
        {
            HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create(TGT_URL);
            string body = string.Format(TGT_BODY_FORMAT, username, password);
            byte[] rawBody = Encoding.UTF8.GetBytes(body);
            webRequest.Method = "POST";
            webRequest.ContentType = "text/plain";
            webRequest.ContentLength = rawBody.Length;

            using (var sw = webRequest.GetRequestStream())
            {
                sw.Write(rawBody, 0, rawBody.Length);
            }

            try
            {
                HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse();

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    using (StreamReader sr = new StreamReader(response.GetResponseStream()))
                    {
                        this.ticketGrantingTicket = sr.ReadToEnd();
                        return true;
                    }
                }
            }
            catch (WebException wex)
            {
                Console.WriteLine(wex.ToString());
            }

            return false;
        }

        private string GetServiceTicket()
        {
            string url = string.Format(ST_URL_FORMAT, this.ticketGrantingTicket);
            HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create(url);
            byte[] rawBody = Encoding.UTF8.GetBytes("service=http://umlsks.nlm.nih.gov");
            webRequest.Method = "POST";
            webRequest.ContentType = "text/plain";
            webRequest.ContentLength = rawBody.Length;

            using (var sw = webRequest.GetRequestStream())
            {
                sw.Write(rawBody, 0, rawBody.Length);
            }

            HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse();

            if (response.StatusCode == HttpStatusCode.OK)
            {
                using (StreamReader sr = new StreamReader(response.GetResponseStream()))
                {
                    return sr.ReadToEnd();
                }
            }

            return null;
        }

        private bool ImportRetrieveValueSet(string retrieveValueSetResponse)
        {
            List<CodeSystem> codeSystems = this.tdb.CodeSystems.ToList();
            int importCount = 0;

            XmlDocument doc = new XmlDocument();
            XmlNamespaceManager nsManager = new XmlNamespaceManager(doc.NameTable);
            nsManager.AddNamespace("svs", SVS_NS);
            doc.LoadXml(retrieveValueSetResponse);

            var svsValueSetNodes = doc.SelectNodes("/svs:RetrieveMultipleValueSetsResponse/svs:DescribedValueSet", nsManager);

            foreach (XmlElement svsValueSetNode in svsValueSetNodes)
            {
                string svsValueSetId = svsValueSetNode.Attributes["ID"].Value;
                string svsValueSetVersion = svsValueSetNode.Attributes["version"].Value;
                string identifier = string.Format("urn:hl7ii:{0}:{1}", svsValueSetId, svsValueSetVersion);
                string name = svsValueSetNode.Attributes["displayName"].Value;
                var purposeNode = svsValueSetNode.SelectSingleNode("svs:Purpose", nsManager);
                string description = purposeNode != null ? purposeNode.InnerText : null;

                if (!string.IsNullOrEmpty(description))
                {
                    if (description.StartsWith("(") && description.EndsWith(")"))
                        description = description.Substring(1, description.Length - 2).Trim();
                }

                ValueSet foundValueSet = (from vs in this.tdb.ValueSets
                                          join vsi in this.tdb.ValueSetIdentifiers on vs.Id equals vsi.ValueSetId
                                          where vsi.Type == ValueSetIdentifierTypes.HL7II && vsi.Identifier.Trim().ToLower() == identifier.Trim().ToLower()
                                          select vs)
                                          .FirstOrDefault();

                if (foundValueSet == null)
                {
                    foundValueSet = new ValueSet();
                    this.tdb.ValueSets.Add(foundValueSet);

                    foundValueSet.Identifiers.Add(new ValueSetIdentifier()
                    {
                        Type = ValueSetIdentifierTypes.HL7II,
                        Identifier = identifier,
                        IsDefault = true
                    });
                }
                else if (!foundValueSet.ImportSource.HasValue || foundValueSet.ImportSource == ValueSetImportSources.VSAC)
                {
                    // If the value set was manually created in Trifolia,
                    // remove all members from the value-set, since VSAC is a better source
                    var allMembers = foundValueSet.Members.ToList();

                    foreach (var member in allMembers)
                    {
                        this.tdb.ValueSetMembers.Remove(member);
                    }
                }
                else if (foundValueSet.ImportSource != ValueSetImportSources.VSAC)
                {
                    throw new Exception("This value set was imported from another source. It cannot be re-imported from a different source.");
                }

                if (foundValueSet.Name != name)
                    foundValueSet.Name = name;

                if (foundValueSet.ImportSource != ValueSetImportSources.VSAC)
                    foundValueSet.ImportSource = ValueSetImportSources.VSAC;

                if (foundValueSet.ImportSourceId != svsValueSetId)
                    foundValueSet.ImportSourceId = svsValueSetId;

                if (foundValueSet.Description != description)
                    foundValueSet.Description = description;

                var svsConceptNodes = svsValueSetNode.SelectNodes("svs:ConceptList/svs:Concept", nsManager);

                foreach (XmlElement svsConceptNode in svsConceptNodes)
                {
                    string svsCodeSystemOid = "urn:oid:" + svsConceptNode.Attributes["codeSystem"].Value;
                    CodeSystem foundCodeSystem = codeSystems.SingleOrDefault(y => y.Oid.Trim().ToLower() == svsCodeSystemOid.Trim().ToLower());

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

                    bool foundMember = foundValueSet.Members.Any(y =>
                        y.Code.Trim().ToLower() == svsMember.Code.Trim().ToLower() &&
                        y.CodeSystem == svsMember.CodeSystem &&
                        y.Status == svsMember.Status);

                    if (foundMember)
                        continue;

                    foundValueSet.Members.Add(svsMember);
                }

                importCount++;
            }

            this.tdb.SaveChanges();

            return importCount > 0;
        }
    }
}