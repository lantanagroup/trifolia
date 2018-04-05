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
        public static void Append(OpenXmlElement source, OpenXmlElement destination)
        {
            List<Paragraph> paragraphs = source.ChildElements.OfType<Paragraph>().ToList();

            for (int i = 0; i < paragraphs.Count; i++)
            {
                paragraphs[i].ChildElements.ToList().ForEach(c =>
                {
                    destination.Append(
                        c.CloneNode(true));
                });

                if (i < paragraphs.Count - 1)
                    destination.Append(new Break());
            }
        }
    }
}
