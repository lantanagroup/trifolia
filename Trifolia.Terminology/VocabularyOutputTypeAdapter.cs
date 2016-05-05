using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Serialization;
using System.IO;
using Trifolia.Shared;
using System.Text;
using LantanaGroup.ValidationUtility;

namespace Trifolia.Terminology
{
    public class VocabularyOutputTypeAdapter
    {
        private const string DYNAMIC_BINDING = "dynamic";

        private VocabularyOutputType _vocabOutputType;
        private VocabularySystems _vocabSystems;
        private Encoding _encoding;

        public VocabularyOutputTypeAdapter(VocabularySystems aVocabSystems, VocabularyOutputType aOutputType)
        {
            _vocabOutputType = aOutputType;
            _vocabSystems = aVocabSystems;
            _encoding = Encoding.UTF8;
        }


        public VocabularyOutputTypeAdapter(VocabularySystems aVocabSystems, VocabularyOutputType aOutputType, Encoding aEncoding)
            : this(aVocabSystems, aOutputType)
        {
            _encoding = aEncoding;
        }

        public string AsXML()
        {
            //TODO: Refactor the AsDefaultXML, AsSVSXML and AsSVS_SingleValueSetXML into a more dynamic pattern.
            switch (_vocabOutputType)
            {
                case VocabularyOutputType.Default:
                    return AsDefaultXML();
                case VocabularyOutputType.SVS:
                    return AsSVSXML();
                case VocabularyOutputType.SVS_SingleValueSet:
                    return AsSVS_SingleValueSetXML();
                case VocabularyOutputType.FHIR:
                    var svsXml = AsSVSXML();
                    return AsFHIRXML(svsXml);
                default:
                    throw new ArgumentException(string.Format("No implementation for Vocabulary Type of '{0}'.", _vocabOutputType.ToString()));
            }
        }

        private string AsDefaultXML()
        {
            return CreateXML(typeof(VocabularySystems), _vocabSystems);
        }

        private string AsSVSXML()
        {
            return CreateXML(typeof(Schemas.VocabularyService.SVS.MultipleValueSet.RetrieveMultipleValueSetsResponseType), CreateRetrieveMultipleValueSetsResponse());
        }

        private string AsSVS_SingleValueSetXML()
        {
            return CreateXML(typeof(Schemas.VocabularyService.SVS.SingleValueSet.RetrieveValueSetResponseType), CreateRetrieveValueSetResponse());
        }

        private string AsFHIRXML(string svsXml)
        {
            string stylesheetContent = string.Empty;

            using (StreamReader sr = new StreamReader(System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Trifolia.Terminology.Transforms.Svs2FhirValueSet.xslt")))
            {
                stylesheetContent = sr.ReadToEnd();
            }

            return TransformFactory.Transform(svsXml, stylesheetContent, "http://www.w3.org/1999/XSL/Transform", new LantanaXmlResolver());
        }

        private string CreateXML(Type aType, object aObjectToSerialize)
        {
            XmlSerializer serializer = new XmlSerializer(aType);

            using (StringWriterWithEncoding sw = new StringWriterWithEncoding(_encoding))
            {
                serializer.Serialize(sw, aObjectToSerialize);
                return sw.ToString();
            }
        }

        private Schemas.VocabularyService.SVS.SingleValueSet.RetrieveValueSetResponseType CreateRetrieveValueSetResponse()
        {
            var response = new Schemas.VocabularyService.SVS.SingleValueSet.RetrieveValueSetResponseType();
            response.cacheExpirationHint = DateTime.Now.Date;
            response.cacheExpirationHintSpecified = true;
            if (_vocabSystems.Systems.Length > 0)
            {
                Schemas.VocabularyService.SVS.SingleValueSet.ValueSetResponseType valueSet = new Schemas.VocabularyService.SVS.SingleValueSet.ValueSetResponseType();
                valueSet.displayName = _vocabSystems.Systems[0].ValueSetName;
                valueSet.id = _vocabSystems.Systems[0].ValueSetOid;
                valueSet.version = string.Empty;
                var concepts = new List<Schemas.VocabularyService.SVS.SingleValueSet.CE>();
                foreach (var code in _vocabSystems.Systems[0].Codes)
                {
                    concepts.Add(new Schemas.VocabularyService.SVS.SingleValueSet.CE()
                    {
                        code = code.Value,
                        codeSystem = code.CodeSystem,
                        codeSystemName = code.CodeSystemName,
                        displayName = code.DisplayName
                    });
                }

                var conceptList = new Schemas.VocabularyService.SVS.SingleValueSet.ConceptListType() { Concept = concepts.ToArray() };
                valueSet.ConceptList = new Schemas.VocabularyService.SVS.SingleValueSet.ConceptListType[] {conceptList};
                response.ValueSet = valueSet;
            }
            return response;
        }

        private Schemas.VocabularyService.SVS.MultipleValueSet.RetrieveMultipleValueSetsResponseType CreateRetrieveMultipleValueSetsResponse()
        {
            var response = new Schemas.VocabularyService.SVS.MultipleValueSet.RetrieveMultipleValueSetsResponseType();
            var valueSets = new List<Schemas.VocabularyService.SVS.MultipleValueSet.DescribedValueSet>();
            foreach (var vocab in _vocabSystems.Systems)
            {
                if (vocab.Codes.Length > 0)
                {
                    var valueSet = new Schemas.VocabularyService.SVS.MultipleValueSet.DescribedValueSet();
                    valueSet.Binding = Schemas.VocabularyService.SVS.MultipleValueSet.DescribedValueSetBinding.Static;
                    if (vocab.Binding != null && vocab.Binding.ToLower() == DYNAMIC_BINDING)
                    {
                        valueSet.Binding = Schemas.VocabularyService.SVS.MultipleValueSet.DescribedValueSetBinding.Dynamic;
                    }
                    valueSet.BindingSpecified = true;
                    valueSet.CreationDateSpecified = false;
                    valueSet.displayName = vocab.ValueSetName;
                    valueSet.EffectiveDateSpecified = false;
                    valueSet.ExpirationDateSpecified = false;
                    valueSet.ID = vocab.ValueSetOid;
                    valueSet.RevisionDateSpecified = false;
                    valueSet.Type = Schemas.VocabularyService.SVS.MultipleValueSet.DescribedValueSetType.Expanded;
                    valueSet.version = "";
                    valueSet.Source = "";
                    valueSet.SourceURI = "";

                    var concepts = new List<Schemas.VocabularyService.SVS.MultipleValueSet.CE>();
                    foreach (var code in vocab.Codes)
                    {
                        concepts.Add(new Schemas.VocabularyService.SVS.MultipleValueSet.CE()
                        {
                            code = code.Value,
                            codeSystem = code.CodeSystem,
                            codeSystemName = code.CodeSystemName,
                            displayName = code.DisplayName
                        });
                    }
                    valueSet.ConceptList = new Schemas.VocabularyService.SVS.MultipleValueSet.ConceptListType() { Concept = concepts.ToArray() };
                    valueSets.Add(valueSet);
                }
            }
            response.DescribedValueSet = valueSets.ToArray();

            return response;
        }
    }
}