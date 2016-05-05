using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Trifolia.Shared
{
    public class VocabularyOutputTypeTranslator
    {
        public static VocabularyOutputType FromInt(int aTypeNum)
        {
            string[] values = System.Enum.GetNames(typeof(VocabularyOutputType));
            VocabularyOutputType outputType = VocabularyOutputType.Default;
            if (values.GetLength(0) >= aTypeNum)
                outputType = (VocabularyOutputType)aTypeNum;

            return outputType;
        }
    }
}
