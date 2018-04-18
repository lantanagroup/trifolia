using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using Trifolia.DB;
using Trifolia.Logging;
using Trifolia.Plugins;
using Trifolia.Shared;

namespace Trifolia.Export.Terminology
{
    public abstract class BaseExporter<T>
    {
        protected IObjectRepository tdb;

        public BaseExporter(IObjectRepository tdb)
        {
            this.tdb = tdb;
        }

        protected abstract T Convert(VocabularySystems systems);

        public byte[] GetExport(int valueSetId, Encoding encoding)
        {
            ValueSet valueSet = this.tdb.ValueSets.Single(y => y.Id == valueSetId);

            VocabularySystems systems = new VocabularySystems();
            systems.Systems = new VocabularySystem[] { this.GetSystem(null, this.tdb, valueSet, DateTime.Now, false) };

            T model = this.Convert(systems);

            return this.Serialize(model, encoding);
        }

        public byte[] GetExport(int implementationGuideId, int maxValueSetMembers, Encoding encoding, bool? onlyStatic = null)
        {
            VocabularySystems systems = this.GetSystems(implementationGuideId, maxValueSetMembers, onlyStatic);
            T model = this.Convert(systems);

            return this.Serialize(model, encoding);
        }

        private byte[] Serialize(T model, Encoding encoding)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));

            using (StringWriterWithEncoding sw = new StringWriterWithEncoding(encoding))
            {
                serializer.Serialize(sw, model);
                string content = sw.ToString();
                return encoding.GetBytes(content);
            }
        }

        protected VocabularySystems GetSystems(int implementationGuideId, int maxValueSetMembers, bool? onlyStatic = null)
        {
            try
            {
                ImplementationGuide ig = tdb.ImplementationGuides.SingleOrDefault(y => y.Id == implementationGuideId);

                if (ig == null)
                    throw new Exception("Could not find ImplementationGuide specified.");

                IIGTypePlugin igTypePlugin = ig.ImplementationGuideType.GetPlugin();
                List<ImplementationGuideValueSet> valueSets = ig.GetValueSets(tdb, onlyStatic);
                List<VocabularySystem> systems = new List<VocabularySystem>();
                bool isCDA = ig.ImplementationGuideType.SchemaURI == "urn:hl7-org:v3";

                foreach (ImplementationGuideValueSet cValueSet in valueSets)
                {
                    VocabularySystem newSystem = this.GetSystem(igTypePlugin, tdb, cValueSet.ValueSet, cValueSet.BindingDate, isCDA);
                    systems.Add(newSystem);
                }

                VocabularySystems schema = new VocabularySystems();
                schema.Systems = systems.ToArray();

                return schema;
            }
            catch (Exception ex)
            {
                Log.For(this).Critical("Error occurred while retrieving vocabulary for an implementation guide.", ex);
                throw;
            }
        }

        private VocabularySystem GetSystem(IIGTypePlugin igTypePlugin, IObjectRepository tdb, ValueSet valueSet, DateTime? bindingDate, bool isCDA)
        {
            if (valueSet == null)
                throw new Exception("Could not find ValueSet specified.");

            VocabularySystems schema = new VocabularySystems();
            VocabularySystem schemaValueSet = new VocabularySystem()
            {
                ValueSetOid = valueSet.GetIdentifier(igTypePlugin),
                ValueSetName = valueSet.Name
            };

            if (isCDA && schemaValueSet.ValueSetOid.StartsWith("urn:oid:"))
                schemaValueSet.ValueSetOid = schemaValueSet.ValueSetOid.Substring(8);

            schemaValueSet.Codes = GetCodes(valueSet, bindingDate, isCDA);

            return schemaValueSet;
        }

        private VocabularyCode[] GetCodes(ValueSet valueSet, DateTime? bindingDate, bool isCDA)
        {
            List<ValueSetMember> members = valueSet.GetActiveMembers(bindingDate);
            List<VocabularyCode> vocabularyCodes = new List<VocabularyCode>();

            foreach (var vc in members)
            {
                VocabularyCode vocabularyCode = new VocabularyCode()
                {
                    Value = vc.Code,
                    DisplayName = vc.DisplayName,
                    CodeSystem = vc.CodeSystem.Oid,
                    CodeSystemName = vc.CodeSystem.Name
                };

                if (isCDA && vocabularyCode.CodeSystem.StartsWith("urn:oid:"))
                    vocabularyCode.CodeSystem = vocabularyCode.CodeSystem.Substring(8);

                vocabularyCodes.Add(vocabularyCode);
            }

            return vocabularyCodes.ToArray();
        }
    }
}
