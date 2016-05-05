using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using Schemas.VocabularyService.SVS.MultipleValueSet;

namespace Trifolia.Terminology
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "ISVS_ValueSetRepository" in both code and config file together.
    [ServiceContract]
    public interface ISVS_ValueSetRepository
    {
        [OperationContract]
        [WebInvoke(Method="POST")]
        RetrieveValueSetResponseType RetrieveValueSet(RetrieveValueSetRequestType request);

        [OperationContract]
        RetrieveMultipleValueSetsResponseType RetrieveMultipleValueSets(RetrieveMultipleValueSetsRequest request);
    }
}
