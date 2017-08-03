using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Trifolia.DB;
using Trifolia.Shared;
using Trifolia.Export.Terminology.SVS.MultipleValueSet;

namespace Trifolia.Export.Terminology
{
    public class MultipleSVSExporter : BaseExporter<SVS.MultipleValueSet.RetrieveMultipleValueSetsResponseType>
    {
        private const string DYNAMIC_BINDING = "dynamic";

        public MultipleSVSExporter(IObjectRepository tdb)
            : base(tdb)
        {

        }

        protected override SVS.MultipleValueSet.RetrieveMultipleValueSetsResponseType Convert(VocabularySystems systems)
        {
            var response = new RetrieveMultipleValueSetsResponseType();
            var valueSets = new List<DescribedValueSet>();

            foreach (var vocab in systems.Systems)
            {
                if (vocab.Codes.Length > 0)
                {
                    var valueSet = new DescribedValueSet();

                    valueSet.Binding = DescribedValueSetBinding.Static;

                    if (vocab.Binding != null && vocab.Binding.ToLower() == DYNAMIC_BINDING)
                        valueSet.Binding = DescribedValueSetBinding.Dynamic;

                    valueSet.BindingSpecified = true;
                    valueSet.CreationDateSpecified = false;
                    valueSet.displayName = vocab.ValueSetName;
                    valueSet.EffectiveDateSpecified = false;
                    valueSet.ExpirationDateSpecified = false;
                    valueSet.ID = vocab.ValueSetOid;
                    valueSet.RevisionDateSpecified = false;
                    valueSet.Type = DescribedValueSetType.Expanded;
                    valueSet.version = "";
                    valueSet.Source = "";
                    valueSet.SourceURI = "";

                    var concepts = new List<CE>();
                    foreach (var code in vocab.Codes)
                    {
                        concepts.Add(new CE()
                        {
                            code = code.Value,
                            codeSystem = code.CodeSystem,
                            codeSystemName = code.CodeSystemName,
                            displayName = code.DisplayName
                        });
                    }
                    valueSet.ConceptList = new ConceptListType() { Concept = concepts.ToArray() };
                    valueSets.Add(valueSet);
                }
            }

            response.DescribedValueSet = valueSets.ToArray();

            return response;
        }
    }
}
