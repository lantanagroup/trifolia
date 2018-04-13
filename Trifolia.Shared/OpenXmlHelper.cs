using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trifolia.Shared
{
    public static class OpenXmlHelper
    {
        private static void AppendChildren(IEnumerable<OpenXmlElement> sourceElements, OpenXmlElement destination)
        {
            foreach (var source in sourceElements)
            {
                if (source is Paragraph && destination is Paragraph)
                {
                    AppendChildren(source.ChildElements, destination);
                    continue;
                }

                if (source is ParagraphProperties && destination.ChildElements.OfType<ParagraphProperties>().Count() > 0)
                    continue;

                destination.Append(source.CloneNode(true));
            }
        }

        public static void Append(OpenXmlElement source, OpenXmlElement destination)
        {
            if (source is Body)
                AppendChildren(source.ChildElements, destination);
            else
                AppendChildren(new OpenXmlElement[] { source }, destination);
        }
    }
}
