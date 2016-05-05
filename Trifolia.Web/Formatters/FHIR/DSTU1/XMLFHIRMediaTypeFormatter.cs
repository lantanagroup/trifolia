extern alias fhir_dstu1;

using System;
using System.Xml;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;
using System.Web;
using System.IO;
using System.Net;
using System.Threading.Tasks;

using fhir_dstu1.Hl7.Fhir.Model;
using fhir_dstu1.Hl7.Fhir.Rest;
using fhir_dstu1.Hl7.Fhir.Serialization;

namespace Trifolia.Web.Formatters.FHIR.DSTU1
{
    public class XMLFHIRMediaTypeFormatter : FhirMediaTypeFormatter
    {
        public XMLFHIRMediaTypeFormatter()
            : base()
        {
            foreach (var mediaType in ContentType.XML_CONTENT_HEADERS)
                SupportedMediaTypes.Add(new MediaTypeHeaderValue(mediaType));
        }

        public override Task<object> ReadFromStreamAsync(Type type, Stream readStream, HttpContent content, IFormatterLogger formatterLogger)
        {
            return Task.Factory.StartNew<object>( () => 
            {
                try
                {
                    var body = base.ReadBodyFromStream(readStream, content);

                    if (type == typeof(Profile))
                    {
                        Profile resource = (Profile)FhirParser.ParseResourceFromXml(body);
                        return resource;
                    }
                    else if (type == typeof(ResourceEntry))
                    {
                        Resource resource = FhirParser.ParseResourceFromXml(body);
                        ResourceEntry entry = ResourceEntry.Create(resource);
                        return entry;
                    }
                    else if (type == typeof(Bundle))
                    {
                        if (XmlSignatureHelper.IsSigned(body))
                        {
                            if (!XmlSignatureHelper.VerifySignature(body))
                                throw new Exception("Digital signature in body failed verification");
                        }

                        return FhirParser.ParseBundleFromXml(body);
                    }
                    else
                        throw new NotSupportedException(String.Format("Cannot read unsupported type {0} from body",type.Name));
                }
                catch (FormatException exc)
                {
                    throw new Exception("Body parsing failed: " + exc.Message);
                }
            });
        }

        public override Task WriteToStreamAsync(Type type, object value, Stream writeStream, HttpContent content, TransportContext transportContext)
        {
            return Task.Factory.StartNew(() =>
            {
                XmlWriter writer = new XmlTextWriter(writeStream, Encoding.UTF8);
                // todo: klopt het dat Bundle wel en <?xml ...> header heeft een Resource niet?
                // follow up: is now ticket in FhirApi. Check later.

                if (type == typeof(Profile)) 
                {
                    Profile resource = (Profile)value;
                    FhirSerializer.SerializeResource(resource, writer);
                }
                else if (type == typeof(ResourceEntry))
                {
                    ResourceEntry entry = (ResourceEntry)value;
                    FhirSerializer.SerializeResource(entry.Resource, writer);
                }
                else if (type == typeof(Bundle))
                {
                    FhirSerializer.SerializeBundle((Bundle)value, writer);
                }
                
                writer.Flush();
            });
        }
    }

    public class XmlSignatureHelper
    {
        public static bool VerifySignature(string xml)
        {
            if (xml == null) throw new ArgumentNullException("xml");

            XmlDocument doc = new XmlDocument();
            doc.PreserveWhitespace = true;
            doc.LoadXml(xml);

            // If there's no signature => return that we are "valid"
            XmlNode signatureNode = findSignatureElement(doc);
            if (signatureNode == null) return true;

            SignedXml signedXml = new SignedXml(doc);
            signedXml.LoadXml((XmlElement)signatureNode);

            //var x509Certificates = signedXml.KeyInfo.OfType<KeyInfoX509Data>();
            //var certificate = x509Certificates.SelectMany(cert => cert.Certificates.Cast<X509Certificate2>()).FirstOrDefault();

            //if (certificate == null) throw new InvalidOperationException("Signature does not contain a X509 certificate public key to verify the signature");
            //return signedXml.CheckSignature(certificate, true);

            return signedXml.CheckSignature();
        }


        private static XmlNode findSignatureElement(XmlDocument doc)
        {
            var signatureElements = doc.DocumentElement.GetElementsByTagName("Signature", "http://www.w3.org/2000/09/xmldsig#");
            if (signatureElements.Count == 1)
                return signatureElements[0];
            else if (signatureElements.Count == 0)
                return null;
            else
                throw new InvalidOperationException("Document has multiple xmldsig Signature elements");
        }


        public static bool IsSigned(string xml)
        {
            if (xml == null) throw new ArgumentNullException("xml");

            // First, a quick check, before reading the full document
            if (!xml.Contains("Signature")) return false;

            var doc = new XmlDocument();
            doc.LoadXml(xml);
            return findSignatureElement(doc) != null;
        }


        public static string Sign(string xml, X509Certificate2 certificate)
        {
            if (xml == null) throw new ArgumentNullException("xml");
            if (certificate == null) throw new ArgumentNullException("certificate");
            if (!certificate.HasPrivateKey) throw new ArgumentException("certificate", "Certificate should have a private key");

            XmlDocument doc = new XmlDocument();

            doc.PreserveWhitespace = true;
            doc.LoadXml(xml);

            SignedXml signedXml = new SignedXml(doc);
            signedXml.SigningKey = certificate.PrivateKey;

            // Attach certificate KeyInfo
            KeyInfoX509Data keyInfoData = new KeyInfoX509Data(certificate);
            KeyInfo keyInfo = new KeyInfo();
            keyInfo.AddClause(keyInfoData);
            signedXml.KeyInfo = keyInfo;

            // Attach transforms
            var reference = new Reference("");
            reference.AddTransform(new XmlDsigEnvelopedSignatureTransform(includeComments: false));
            reference.AddTransform(new XmlDsigExcC14NTransform(includeComments: false));
            signedXml.AddReference(reference);

            // Compute signature
            signedXml.ComputeSignature();
            var signatureElement = signedXml.GetXml();

            // Add signature to bundle
            doc.DocumentElement.AppendChild(doc.ImportNode(signatureElement, true));

            return doc.OuterXml;
        }
    }
}