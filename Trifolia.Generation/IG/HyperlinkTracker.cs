using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trifolia.Generation.IG
{
    public class HyperlinkTracker
    {
        public struct HyperlinkInfo
        {
            public int InternalId { get; set; }
            public bool AnchorCreated { get; set; }
        }

        private int nextDocId = 101;
        private Dictionary<string, HyperlinkInfo> ids = new Dictionary<string, HyperlinkInfo>();

        private HyperlinkInfo FindOrAddInternalId(string id)
        {
            if (!this.ids.ContainsKey(id))
            {
                this.ids.Add(id, new HyperlinkInfo()
                {
                    InternalId = this.nextDocId
                });
                this.nextDocId++;
            }

            return this.ids[id];
        }

        public void AddAnchorAround(OpenXmlElement parent, string id, params OpenXmlElement[] children)
        {
            HyperlinkInfo hyperlinkInfo = this.FindOrAddInternalId(id);

            if (hyperlinkInfo.AnchorCreated)
                throw new Exception("An anchor has already been created for this id");

            hyperlinkInfo.AnchorCreated = true;

            var bookmarkStart = new BookmarkStart()
            {
                Id = hyperlinkInfo.InternalId.ToString(),
                Name = id
            };
            
            parent.Append(bookmarkStart);
            parent.Append(children);
            parent.Append(new BookmarkEnd() { Id = bookmarkStart.Id });
        }

        public Hyperlink CreateHyperlink(string text, string anchorId, string style = null, int? size = null)
        {
            RunProperties rp = new RunProperties();

            if (!string.IsNullOrEmpty(style))
                rp.Append(
                    new RunStyle()
                    {
                        Val = style,
                    });

            if (size != null)
                rp.Append(
                    new FontSize()
                    {
                        Val = new StringValue((2 * size).ToString())
                    });

            // Ensure a hyperlink info is recorded if needed for future use
            HyperlinkInfo hyperlinkInfo = this.FindOrAddInternalId(anchorId);

            Hyperlink hyperlink = new Hyperlink() { Anchor = anchorId };
            hyperlink.Append(new ProofError() { Type = ProofingErrorValues.GrammarStart });
            hyperlink.Append(
                new Run(rp, new Text(text)));

            return hyperlink;
        }

        public void AddHyperlink(Paragraph paragraph, string text, string anchorId, string style = null, int? size = null)
        {
            var hyperlink = this.CreateHyperlink(text, anchorId, style, size);
            paragraph.Append(hyperlink);
        }

        public Hyperlink CreateUrlHyperlink(MainDocumentPart mainPart, string text, string url, string style)
        {
            RunProperties rp = new RunProperties(
                new RunStyle()
                {
                    Val = style
                });

            HyperlinkRelationship rel = mainPart.AddHyperlinkRelationship(new Uri(url), true);

            var hyperlink = new Hyperlink(
                new ProofError() { Type = ProofingErrorValues.GrammarStart },
                new Run(
                    rp,
                    new Text(text)))
            {
                History = true,
                Id = rel.Id
            };

            return hyperlink;
        }

        public void AddUrlHyperlink(MainDocumentPart mainPart, Paragraph paragraph, string text, string url, string style)
        {
            var hyperlink = this.CreateUrlHyperlink(mainPart, text, url, style);
            paragraph.Append(hyperlink);
        }
    }
}
