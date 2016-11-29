using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Trifolia.DB
{
    public partial class ImplementationGuideFile
    {
        public const string ContentTypeGoodSample = "GoodSample";
        public const string ContentTypeBadSample = "BadSample";
        public const string ContentTypeSchematron = "Schematron";
        public const string ContentTypeSchematronHelper = "SchematronHelper";
        public const string ContentTypeVocabulary = "Vocabulary";
        public const string ContentTypeImplementationGuide = "ImplementationGuide";
        public const string ContentTypeGreenSchema = "GreenSchema";
        public const string ContentTypeGreenTransform = "GreenTransform";
        public const string ContentTypeNormativeTransform = "NormativeTransform";

        #region IG Retrieval Static Methods

        public static List<ImplementationGuideFile> GetImplementationGuideSamples(int implementationGuideId)
        {
            using (IObjectRepository tdb = DBContext.Create())
            {
                return tdb.ImplementationGuideFiles.Where(y => y.ImplementationGuideId == implementationGuideId)
                            .Where(y => y.ContentType == ContentTypeGoodSample || y.ContentType == ContentTypeBadSample)
                            .ToList();
            }
        }

        public static List<ImplementationGuideFile> GetImplementationGuideSchematrons(int implementationGuideId)
        {
            using (IObjectRepository tdb = DBContext.Create())
            {
                return tdb.ImplementationGuideFiles.Where(y => y.ImplementationGuideId == implementationGuideId && y.ContentType == ContentTypeSchematron)
                            .ToList();
            }
        }

        #endregion

        #region Instance Methods

        public ImplementationGuideFileData GetLatestData()
        {
            DateTime lastUpdateDate = this.Versions.Max(y => y.UpdatedDate);
            return this.Versions.First(y => y.UpdatedDate == lastUpdateDate);
        }

        #endregion
    }
}
