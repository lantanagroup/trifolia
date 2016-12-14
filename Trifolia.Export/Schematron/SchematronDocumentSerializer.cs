using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Trifolia.Export.Schematron.Model;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using Trifolia.Shared;

namespace Trifolia.Export.Schematron
{
    /// <summary>
    /// Creates a valid Schematron document from a list of <see cref="Rule"/> and <see cref="Assertion"/>
    /// </summary>
    public class SchematronDocumentSerializer
    {

        #region Private Constants

        private const string DEFAULT_NAMESPACE_ALIAS = "sch";
        private const string DEFAULT_NAMESPACE_URI = "http://purl.oclc.org/dsdl/schematron";

        #endregion

        #region Private Fields

        private readonly XmlDocument _currentDocument = new XmlDocument();
        private Dictionary<string, string> namespaces = new Dictionary<string, string>();
        private const string DisclaimerFormat = "\n\nTHIS SOFTWARE IS PROVIDED \"AS IS\" AND ANY EXPRESSED OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL LANTANA CONSULTING GROUP LLC, OR ANY OF THEIR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.\nSchematron generated from Trifolia on {0}\n";

        #endregion

        #region Public Methods

        public string SerializeDocument(SchematronDocument aDocument)
        {
            this.CreateHeaderAndRoot();

            // Phases must be placed at the top of the document
            this.RegisterPhases(aDocument.Phases);

            // Patterns must be placed after all of the phases
            foreach (Phase lCurrentPhase in aDocument.Phases)
            {
                this.RegisterPatterns(lCurrentPhase.ActivePatterns);
            }

            return this.CreateXmlString();
        }

        public void AddNamespace(string prefix, string uri)
        {
            // Don't add namespaces that are automatically added to the document
            if (prefix.ToLower() == "xsi" || prefix.ToLower() == "voc" || prefix.ToLower() == "svs" || prefix.ToLower() == "sdtc")
                return;

            if (this.namespaces.ContainsKey(prefix))
                this.namespaces[prefix] = uri;
            else
                this.namespaces.Add(prefix, uri);
        }

        public void RemoveNamespace(string prefix)
        {
            if (this.namespaces.ContainsKey(prefix))
                this.namespaces.Remove(prefix);
        }

        #endregion

        #region Private Methods

        private string CreateXmlString()
        {
            XmlWriterSettings lWriterSettings = new XmlWriterSettings();
            lWriterSettings.Indent = true;
            lWriterSettings.IndentChars = "  ";
            lWriterSettings.NewLineChars = "\r\n";
            lWriterSettings.CheckCharacters = false;
            lWriterSettings.NewLineHandling = NewLineHandling.Replace;
            lWriterSettings.Encoding = Encoding.UTF8;

            var lDocumentBuilder = new StringWriterWithEncoding(Encoding.UTF8);            
            using (XmlWriter lDocumentWriter = XmlWriter.Create(lDocumentBuilder, lWriterSettings))
            {
                _currentDocument.Save(lDocumentWriter);
            }

            return lDocumentBuilder
                .ToString()
                .Replace("&#xA;", " ")
                .Replace("&amp;#xA;", " ");
        }

        private void RegisterPhases(List<Phase> aPhases)
        {
            XmlElement lRoot = _currentDocument.DocumentElement;

            foreach (Phase lCurrentPhase in aPhases)
            {
                XmlElement lPhase = this.CreatePhaseElement(lCurrentPhase.ID);
                this.CreateActiveElements(lPhase, lCurrentPhase.ActivePatterns);

                lRoot.AppendChild(lPhase);
            }
        }

        private void RegisterPatterns(List<Pattern> aPatterns)
        {
            XmlElement lRoot = _currentDocument.DocumentElement;

            foreach (Pattern lCurrentPattern in aPatterns)
            {
                string patternId = lCurrentPattern.ID.Replace(":", "-");
                XmlElement lPatternElement = this.CreatePatternElement(patternId, lCurrentPattern.Name, lCurrentPattern.IsA);

                if (!string.IsNullOrEmpty(lCurrentPattern.CustomXML))
                {
                    string customXml = string.Format("<root>{0}</root>", lCurrentPattern.CustomXML);
                    XmlDocument customDoc = new XmlDocument();

                    XmlNamespaceManager customNsManager = CreateNamespaceManager(customDoc);
                    XmlParserContext context = new XmlParserContext(null, customNsManager, null, XmlSpace.None);
                    XmlReaderSettings settings = new XmlReaderSettings()
                    {
                        ConformanceLevel = ConformanceLevel.Fragment
                    };

                    try
                    {
                        XmlReader customReader = XmlReader.Create(new StringReader(customXml), settings, context);
                        customDoc.Load(customReader);

                        foreach (XmlNode cChildCustomNode in customDoc.DocumentElement.ChildNodes)
                        {
                            XmlNode importNode = this._currentDocument.ImportNode(cChildCustomNode, true);
                            lPatternElement.AppendChild(importNode);
                        }
                    }
                    catch (Exception ex)
                    {
                        string msg = string.Format("Error loading custom schematron pattern '{0}': {1}", lCurrentPattern.ID, ex.Message);
                        throw new Exception(msg, ex);
                    }
                }
                else
                {
                    if (lCurrentPattern.IsImplied)
                    {
                        XmlComment lPatternImpliedComment = this._currentDocument.CreateComment("Pattern is used in an implied relationship.");
                        lPatternElement.AppendChild(lPatternImpliedComment);
                    }

                    foreach (Rule lCurrentRule in lCurrentPattern.Rules)
                    {
                        XmlElement lAbstractRule = this.CreateAbstractRuleElement(lCurrentRule.Id, lCurrentRule.Context, lCurrentRule.Extends);

                        var abstractRules = lCurrentRule.Assertions.Where(a => a.IsInheritable);
                        foreach (Assertion lCurrentAssertion in abstractRules)
                        {
                            XmlElement lAssertionElement = this.CreateAssertionElement(lCurrentAssertion.Id,
                                                                                       lCurrentAssertion.Test,
                                                                                       lCurrentAssertion.AssertionMessage,
                                                                                       lCurrentAssertion.IdPostFix);

                            // If there is a comment for the assertion, add it before the assertion element
                            if (!string.IsNullOrEmpty(lCurrentAssertion.Comment))
                            {
                                XmlComment lAssertionComment = this._currentDocument.CreateComment(lCurrentAssertion.Comment);
                                lAbstractRule.AppendChild(lAssertionComment);
                            }

                            //if there is a blank test, then we should only add a comment, no test. The comment should help the user go back to tdb and fix the bad element or attribute.
                            if (string.IsNullOrEmpty(lCurrentAssertion.Test))
                            {
                                XmlComment lAssertionComment = this._currentDocument.CreateComment("Blank assertion generated. Please check template database, template constraint id: " + 
                                    (lCurrentAssertion.Id != null ? lCurrentAssertion.Id : "UNKNOWN"));
                                lAbstractRule.AppendChild(lAssertionComment);
                            }

                            lAbstractRule.AppendChild(lAssertionElement);
                        }

                        lPatternElement.AppendChild(lAbstractRule);

                        //add each concrete assertion
                        XmlElement lConcreteRule = this.CreateConcreteRuleElement(lCurrentRule.Id, lCurrentRule.Context);
                        var concreteRules = lCurrentRule.Assertions.Where(a => !a.IsInheritable);
                        foreach (Assertion lCurrentAssertion in concreteRules)
                        {
                            XmlElement lAssertionElement = this.CreateAssertionElement(lCurrentAssertion.Id,
                                                                                       lCurrentAssertion.Test,
                                                                                       lCurrentAssertion.AssertionMessage,
                                                                                       lCurrentAssertion.IdPostFix);

                            // If there is a comment for the assertion, add it before the assertion element
                            if (!string.IsNullOrEmpty(lCurrentAssertion.Comment))
                            {
                                XmlComment lAssertionComment = this._currentDocument.CreateComment(lCurrentAssertion.Comment);
                                lAbstractRule.AppendChild(lAssertionComment);
                            }

                            //if there is a blank test, then we should only add a comment, no test. The comment should help the user go back to tdb and fix the bad element or attribute.
                            if (string.IsNullOrEmpty(lCurrentAssertion.Test))
                            {
                                XmlComment lAssertionComment = this._currentDocument.CreateComment("Blank assertion generated. Please check template database, template constraint id: " + lCurrentAssertion.Id.ToString());
                                lConcreteRule.AppendChild(lAssertionComment);
                            }

                            lConcreteRule.AppendChild(lAssertionElement);
                        }
                        lPatternElement.AppendChild(lConcreteRule);
                    }
                }

                lRoot.AppendChild(lPatternElement);
            }
        }


        private void CreateHeaderAndRoot()
        {
            XmlDeclaration lDeclaration = _currentDocument.CreateXmlDeclaration("1.0", "UTF-8", "yes");
            _currentDocument.AppendChild(lDeclaration);

            XmlComment lDateComment = _currentDocument.CreateComment(string.Format(DisclaimerFormat, DateTime.Now.ToShortDateString()));
            _currentDocument.AppendChild(lDateComment);

            XmlElement lSchemaRoot = _currentDocument.CreateElement(DEFAULT_NAMESPACE_ALIAS, "schema", DEFAULT_NAMESPACE_URI);
            lSchemaRoot.SetAttribute("xmlns:voc", "http://www.lantanagroup.com/voc");
            lSchemaRoot.SetAttribute("xmlns:svs", "urn:ihe:iti:svs:2008");
            lSchemaRoot.SetAttribute("xmlns:xsi", "http://www.w3.org/2001/XMLSchema-instance");
            lSchemaRoot.SetAttribute("xmlns:sdtc", "urn:hl7-org:sdtc");
            lSchemaRoot.SetAttribute("xmlns", "urn:hl7-org:v3");

            // VOC
            XmlElement lVocElement = _currentDocument.CreateElement(DEFAULT_NAMESPACE_ALIAS, "ns", DEFAULT_NAMESPACE_URI);
            lVocElement.SetAttribute("prefix", "voc");
            lVocElement.SetAttribute("uri", "http://www.lantanagroup.com/voc");

            lSchemaRoot.AppendChild(lVocElement);

            // SVS
            XmlElement lSVSVocElement = _currentDocument.CreateElement(DEFAULT_NAMESPACE_ALIAS, "ns", DEFAULT_NAMESPACE_URI);
            lSVSVocElement.SetAttribute("prefix", "svs");
            lSVSVocElement.SetAttribute("uri", "urn:ihe:iti:svs:2008");

            lSchemaRoot.AppendChild(lSVSVocElement);

            // XSI
            XmlElement lXsiElement = _currentDocument.CreateElement(DEFAULT_NAMESPACE_ALIAS, "ns",
                                                                    DEFAULT_NAMESPACE_URI);
            lXsiElement.SetAttribute("prefix", "xsi");
            lXsiElement.SetAttribute("uri", "http://www.w3.org/2001/XMLSchema-instance");

            lSchemaRoot.AppendChild(lXsiElement);

            // SDTC
            XmlElement lsdtcElement = _currentDocument.CreateElement(DEFAULT_NAMESPACE_ALIAS, "ns",
                                                                    DEFAULT_NAMESPACE_URI);
            lsdtcElement.SetAttribute("prefix", "sdtc");
            lsdtcElement.SetAttribute("uri", "urn:hl7-org:sdtc");

            lSchemaRoot.AppendChild(lsdtcElement);

            // Set the prefixes
            foreach (string cPrefix in this.namespaces.Keys)
            {
                string cUri = this.namespaces[cPrefix];

                lSchemaRoot.SetAttribute("xmlns:" + cPrefix, cUri);

                XmlElement lNsElement = _currentDocument.CreateElement(DEFAULT_NAMESPACE_ALIAS, "ns",
                                                                        DEFAULT_NAMESPACE_URI);
                lNsElement.SetAttribute("prefix", cPrefix);
                lNsElement.SetAttribute("uri", cUri);

                lSchemaRoot.AppendChild(lNsElement);
            }

            _currentDocument.AppendChild(lSchemaRoot);
        }

        private void CreateActiveElements(XmlElement aPhaseParent, List<Pattern> aActivePatterns)
        {
            foreach (Pattern lPattern in aActivePatterns)
            {
                string patternId = lPattern.ID.Replace(":", "-");
                XmlElement lActiveElement = this.CreateActiveElement(patternId);
                aPhaseParent.AppendChild(lActiveElement);
            }
        }

        private XmlElement CreatePatternElement(string aPatternID, string aPatternName, string aIsA)
        {
            XmlElement lElement = this.CreateNamespaceAliasedXmlElement("pattern");
            lElement.SetAttribute("id", aPatternID);

            return lElement;
        }

        private XmlElement CreatePhaseElement(string aPhaseID)
        {
            XmlElement lElement = this.CreateNamespaceAliasedXmlElement("phase");
            lElement.SetAttribute("id", aPhaseID);

            return lElement;
        }

        private XmlElement CreateActiveElement(string aPatternID)
        {
            XmlElement lElement = this.CreateNamespaceAliasedXmlElement("active");
            lElement.SetAttribute("pattern", aPatternID);

            return lElement;
        }

        private XmlElement CreateAbstractRuleElement(string aId, string aContext, string aExtends)
        {
            XmlElement lElement = this.CreateNamespaceAliasedXmlElement("rule");

            if (!string.IsNullOrEmpty(aId))
                lElement.SetAttribute("id", string.Format("{0}-abstract", aId.Replace(":", "-")));

            if (!string.IsNullOrEmpty(aExtends))
            {
                XmlElement lExtendsElement = this.CreateNamespaceAliasedXmlElement("extends");
                lExtendsElement.SetAttribute("rule", string.Format("{0}-abstract", aExtends.Replace(":", "-")));
                lElement.AppendChild(lExtendsElement);
            }

            lElement.SetAttribute("abstract", "true");

            return lElement;
        }

        private XmlElement CreateConcreteRuleElement(string aId, string aContext)
        {
            XmlElement lElement = this.CreateNamespaceAliasedXmlElement("rule");

            if (!string.IsNullOrEmpty(aId))
                lElement.SetAttribute("id", aId.Replace(":", "-"));

            string rule = string.Format("{0}-abstract", aId);
            rule = rule.Replace(":", "-");

            XmlElement lExtendsElement = this.CreateNamespaceAliasedXmlElement("extends");
            lExtendsElement.SetAttribute("rule", rule);
            lElement.AppendChild(lExtendsElement);

            lElement.SetAttribute("context", aContext);

            return lElement;
        }

        private XmlElement CreateAssertionElement(string id, string aTest, string aAssertionMessage, string idPostFix = "")
        {
            XmlElement lElement = this.CreateNamespaceAliasedXmlElement("assert");

            if (id != null)
            {
                string formattedId = string.Format("a-{0}", id);
                if (!string.IsNullOrEmpty(idPostFix))
                {
                    formattedId += idPostFix;
                }
                lElement.SetAttribute("id", formattedId);
            }

            lElement.SetAttribute("test", aTest);
            lElement.InnerText = aAssertionMessage;
            return lElement;
        }

        private XmlElement CreateNamespaceAliasedXmlElement(string aElementName)
        {
            XmlElement lElement = _currentDocument.CreateElement(DEFAULT_NAMESPACE_ALIAS, aElementName,
                                                                 DEFAULT_NAMESPACE_URI);
            return lElement;
        }

        private XmlNamespaceManager CreateNamespaceManager(XmlDocument doc)
        {
            XmlNamespaceManager nsManager = new XmlNamespaceManager(doc.NameTable);

            nsManager.AddNamespace(DEFAULT_NAMESPACE_ALIAS, DEFAULT_NAMESPACE_URI);
            nsManager.AddNamespace("voc", "http://www.lantanagroup.com/voc");
            nsManager.AddNamespace("svs", "urn:ihe:iti:svs:2008");
            nsManager.AddNamespace("xsi", "http://www.w3.org/2001/XMLSchema-instance");
            nsManager.AddNamespace("cda", "urn:hl7-org:v3");

            return nsManager;
        }

        #endregion
    }
}