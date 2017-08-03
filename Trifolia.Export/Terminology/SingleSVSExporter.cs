using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trifolia.DB;
using Trifolia.Export.Terminology.SVS.SingleValueSet;

namespace Trifolia.Export.Terminology
{
    public class SingleSVSExporter : BaseExporter<SVS.SingleValueSet.RetrieveValueSetResponseType>
    {
        public SingleSVSExporter(IObjectRepository tdb)
            : base(tdb)
        {

        }

        protected override RetrieveValueSetResponseType Convert(VocabularySystems systems)
        {
            var response = new RetrieveValueSetResponseType();
            response.cacheExpirationHint = DateTime.Now.Date;
            response.cacheExpirationHintSpecified = true;

            if (systems.Systems.Length > 0)
            {
                ValueSetResponseType valueSet = new ValueSetResponseType();
                valueSet.displayName = systems.Systems[0].ValueSetName;
                valueSet.id = systems.Systems[0].ValueSetOid;
                valueSet.version = string.Empty;
                var concepts = new List<CE>();
                foreach (var code in systems.Systems[0].Codes)
                {
                    concepts.Add(new CE()
                    {
                        code = code.Value,
                        codeSystem = code.CodeSystem,
                        codeSystemName = code.CodeSystemName,
                        displayName = code.DisplayName
                    });
                }

                var conceptList = new ConceptListType() { Concept = concepts.ToArray() };
                valueSet.ConceptList = new ConceptListType[] { conceptList };
                response.ValueSet = valueSet;
            }

            return response;
        }
    }
}
