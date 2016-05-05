using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace Trifolia.ValidationService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IValidationService" in both code and config file together.
    [ServiceContract]
    public interface IValidationService
    {
        [OperationContract]
        List<ValidationResult> ValidateDocument(int profileId, List<ValidationContent> contents);

        [OperationContract]
        List<ValidationProfile> GetValidationProfiles();

        [OperationContract]
        List<ValidationDocument> GetValidationPackage(int implementationGuideId, GenerationOptions options, DateTime? lastRetrieveDate);

        [OperationContract]
        List<ValidationImplementationGuide> GetValidationImplementationGuides();
    }
}
