using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trifolia.Export.Types
{
    public abstract class BaseTypeExporter
    {
        protected byte[] ConvertToBytes(string content)
        {
            return System.Text.Encoding.UTF8.GetBytes(content);
        }
    }
}
