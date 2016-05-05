using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace Trifolia.Terminology
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IService1" in both code and config file together.
    [ServiceContract]
    public interface IVocabularyService
    {
        [OperationContract]
        string GetValueSet(string valueSetOid, int ValueSetOutputType, string Encoding);

        [OperationContract]
        string GetImplementationGuideVocabulary(int implementationGuideId, int maxValueSetMembers, int ValueSetOutputType, string Encoding);

        [OperationContract]
        byte[] GetImplementationGuideVocabularySpreadsheet(int implementationGuideId, int maxValueSetMembers);

        [OperationContract]
        List<ImplementationGuide> GetImplementationGuides();

        [OperationContract]
        VocabularyOutputTypeSpecifier[] GetVocabularyOutputTypes();

        [OperationContract]
        string[] GetSupportedEncodings();

        [OperationContract]
        string[] GetValueSetOidsForImplementationGuide(int implementationGuideId);

        [OperationContract]
        int GetValueSetMemberLength(string valueSetOid);
    }
}
