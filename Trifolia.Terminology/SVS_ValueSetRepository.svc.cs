using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.Text;
using Schemas.VocabularyService.SVS.MultipleValueSet;

namespace Trifolia.Terminology
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "SVS_ValueSetRepository" in code, svc and config file together.
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, InstanceContextMode = InstanceContextMode.Single)]
    public class SVS_ValueSetRepository : ISVS_ValueSetRepository
    {
        public RetrieveValueSetResponseType RetrieveValueSet(RetrieveValueSetRequestType request)
        {
            return new RetrieveValueSetResponseType();
        }

        public RetrieveMultipleValueSetsResponseType RetrieveMultipleValueSets(RetrieveMultipleValueSetsRequest request)
        {
            throw new NotImplementedException();
        }
    }
}
