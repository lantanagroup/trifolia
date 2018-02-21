extern alias fhir_stu3;

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
using FhirValueSet = fhir_stu3.Hl7.Fhir.Model.ValueSet;
using fhir_stu3.Hl7.Fhir.Serialization;

namespace Trifolia.Import.VSAC
{
    public class VSACImporter
    {
        /*
        private const string ST_URL_FORMAT = "https://vsac.nlm.nih.gov/vsac/ws/Ticket/{0}";
        private const string SVS_RETRIEVE_VALUE_SET_URL_FORMAT = "https://vsac.nlm.nih.gov/vsac/svs/RetrieveMultipleValueSets?id={0}&ticket={1}";
        private const string VSAC_SOURCE_URL_FORMAT = "https://vsac.nlm.nih.gov/valueset/{0}/expansion";
        private const string SVS_NS = "urn:ihe:iti:svs:2008";
        */
        private const string FHIR_EXPAND_URL_FORMAT = "https://cts.nlm.nih.gov/fhir/ValueSet/{0}/$expand";

        private IObjectRepository tdb;
        private string basicCredentials;
        private List<CodeSystem> addedCodeSystems = new List<CodeSystem>();

        public VSACImporter(IObjectRepository tdb)
        {
            this.tdb = tdb;
        }

        public bool Authenticate(string username, string password)
        {
            this.basicCredentials = System.Convert.ToBase64String(System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes(username + ":" + password));
            return true;
        }

        /// <summary>
        /// Retrieves the specified value set from the VSAC and imports the value set into Trifolia.
        /// If the value set already exists, it is updated with the latest name and codes from VSAC.
        /// </summary>
        /// <remarks>Requires Authenticate() to be called first.</remarks>
        /// <param name="oid">The oid of the value set to retrieve. Should not include any prefix (i.e. "urn:oid:")</param>
        public bool ImportValueSet(string oid)
        {
            this.addedCodeSystems = new List<CodeSystem>();

            //string serviceTicket = this.GetServiceTicket();
            string url = string.Format(FHIR_EXPAND_URL_FORMAT, oid);
            HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create(url);

            webRequest.Accept = "application/xml";

            if (!string.IsNullOrEmpty(this.basicCredentials))
                webRequest.Headers.Add("Authorization", "Basic " + this.basicCredentials);

            HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse();

            if (response.StatusCode == HttpStatusCode.OK)
            {
                using (StreamReader sr = new StreamReader(response.GetResponseStream()))
                {
                    string responseBody = sr.ReadToEnd();
                    return this.ImportFHIRExpandValueSet(responseBody, url);
                }
            }

            return false;
        }

        private DateTime GetFhirValueSetDate(FhirValueSet valueSet)
        {
            if (valueSet.Meta != null && valueSet.Meta.LastUpdated != null)
                return valueSet.Meta.LastUpdated.Value.DateTime;

            if (!string.IsNullOrEmpty(valueSet.Date))
                return DateTime.Parse(valueSet.Date);

            throw new Exception("Expected FHIR ValueSet to have either ValueSet.meta.lastUpdated or ValueSet.date");
        }

        private bool ImportFHIRExpandValueSet(string valueSetXml, string url)
        {
            FhirXmlParser parser = new FhirXmlParser();
            FhirValueSet fhirValueSet = parser.Parse<FhirValueSet>(valueSetXml);

            ValueSetIdentifier foundIdentifier = null;
            ValueSetIdentifier newIdentifier = new ValueSetIdentifier();

            if (fhirValueSet.Id.StartsWith("2.16"))             // This is an OID
            {
                DateTime valueSetDate = this.GetFhirValueSetDate(fhirValueSet);
                string localIdentifier = string.Format("urn:hl7ii:{0}:{1}", fhirValueSet.Id, valueSetDate.ToString("yyyyMMdd"));

                foundIdentifier = this.tdb.ValueSetIdentifiers.SingleOrDefault(y => y.Type == ValueSetIdentifierTypes.HL7II && y.Identifier == localIdentifier);
                newIdentifier.Type = ValueSetIdentifierTypes.HL7II;
                newIdentifier.Identifier = localIdentifier;
            }
            else
            {
                foundIdentifier = this.tdb.ValueSetIdentifiers.SingleOrDefault(y => y.Type == ValueSetIdentifierTypes.HTTP && y.Identifier == fhirValueSet.Url);
                newIdentifier.Type = ValueSetIdentifierTypes.HTTP;
                newIdentifier.Identifier = fhirValueSet.Url;
            }

            ValueSet foundValueSet = foundIdentifier != null ? foundIdentifier.ValueSet : null;

            if (foundValueSet == null)
            {
                foundValueSet = new ValueSet();
                foundValueSet.Identifiers.Add(newIdentifier);
                this.tdb.ValueSets.Add(foundValueSet);
            }

            foundValueSet.Description = fhirValueSet.Description != null ? fhirValueSet.Description.Value : null;
            foundValueSet.Name = fhirValueSet.Name;
            foundValueSet.ImportSource = ValueSetImportSources.VSAC;
            foundValueSet.ImportSourceId = fhirValueSet.Id;
            foundValueSet.LastUpdate = fhirValueSet.Meta != null && fhirValueSet.Meta.LastUpdated != null ? fhirValueSet.Meta.LastUpdated.Value.DateTime : DateTime.Now;
            foundValueSet.IsIncomplete = false;

            // Set the source to a url that can be loaded in a browser, if dealing with VSAC value sets
            string source = fhirValueSet.Url;
            if (url.StartsWith("https://cts.nlm.nih.gov/fhir/"))
                source = "https://vsac.nlm.nih.gov/valueset/" + fhirValueSet.Id + "/expansion";

            foundValueSet.Source = source;

            // Remove all existing codes in the value set so they can be re-added
            List<ValueSetMember> currentCodes = foundValueSet.Members.ToList();
            currentCodes.ForEach(m => this.tdb.ValueSetMembers.Remove(m));

            List<CodeSystem> localCodeSystems = this.tdb.CodeSystems.ToList();

            this.AddContains(foundValueSet, fhirValueSet.Expansion.Contains);

            return true;
        }

        private CodeSystem FindOrAddCodeSystem(string fhirSystem)
        {
            string identifier = fhirSystem;

            if (identifier.StartsWith("2.16"))
                identifier = "urn:oid:" + identifier;

            CodeSystem foundCodeSystem = this.addedCodeSystems.SingleOrDefault(y => y.Oid == identifier);

            if (foundCodeSystem == null)
                foundCodeSystem = this.tdb.CodeSystems.SingleOrDefault(y => y.Oid == identifier);

            if (foundCodeSystem == null)
            {
                foundCodeSystem = new CodeSystem();
                foundCodeSystem.Oid = identifier;
                foundCodeSystem.Name = identifier;
                foundCodeSystem.Description = "Created as a result of importing from VSAC on " + DateTime.Now.ToShortDateString();
                this.tdb.CodeSystems.Add(foundCodeSystem);
                this.addedCodeSystems.Add(foundCodeSystem);
            }

            return foundCodeSystem;
        }

        private void AddContains(ValueSet valueSet, List<FhirValueSet.ContainsComponent> contains)
        {
            foreach (var fhirMember in contains)
            {
                if (string.IsNullOrEmpty(fhirMember.System) || string.IsNullOrEmpty(fhirMember.Code))
                    continue;

                CodeSystem codeSystem = this.FindOrAddCodeSystem(fhirMember.System);

                ValueSetMember newMember = new ValueSetMember();
                newMember.CodeSystem = codeSystem;
                newMember.DisplayName = string.IsNullOrEmpty(fhirMember.Display) ? fhirMember.Code : fhirMember.Display;
                newMember.Code = fhirMember.Code;
                newMember.Status = "active";

                valueSet.Members.Add(newMember);

                this.AddContains(valueSet, fhirMember.Contains);
            }
        }

        /*

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
                    foundValueSet = new ValueSet();
                    foundValueSet.LastUpdate = DateTime.Now;
                    this.tdb.ValueSets.Add(foundValueSet);

                    foundValueSet.Identifiers.Add(new ValueSetIdentifier()
                    {
                        Type = ValueSetIdentifierTypes.HL7II,
                        Identifier = identifier,
                        IsDefault = true
                    });
                }
                else if (foundValueSet.ImportSource.HasValue && foundValueSet.ImportSource != ValueSetImportSources.VSAC)
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

                if (foundValueSet.Source != source)
                    foundValueSet.Source = source;

                foundValueSet.Intensional = false;
                foundValueSet.IntensionalDefinition = string.Empty;
                foundValueSet.Code = null;
                foundValueSet.IsIncomplete = false;
                foundValueSet.LastUpdate = DateTime.Now;

                // Remove all existing codes in the value set so they can be re-added
                List<ValueSetMember> currentCodes = foundValueSet.Members.ToList();
                currentCodes.ForEach(m => this.tdb.ValueSetMembers.Remove(m));

                // Add all codes to value set
                var svsConceptNodes = svsValueSetNode.SelectNodes("svs:ConceptList/svs:Concept", nsManager);

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

                    foundValueSet.Members.Add(svsMember);
                }

                importCount++;
            }

            return importCount > 0;
        }
        */
    }
}