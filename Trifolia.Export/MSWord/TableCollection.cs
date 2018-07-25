using DocumentFormat.OpenXml.Wordprocessing;
using Trifolia.Export.MSWord.Models;

namespace Trifolia.Export.MSWord
{
    public class TableCollection
    {
        private int tableCount = 0;
        private Body documentBody;
        private HyperlinkTracker hyperlinkTracker;

        public TableCollection(Body documentBody, HyperlinkTracker hyperlinkTracker)
        {
            this.documentBody = documentBody;
            this.hyperlinkTracker = hyperlinkTracker;
        }

        internal Table AddTable(string title, HeaderDescriptor[] headers, string bookmark = null, string caption = "Table ")
        {
            // Add a caption for the table
            if (!string.IsNullOrEmpty(title))
            {
                Paragraph p3 = DocHelper.CreateTableCaption(
                    tableCount++,
                    title,
                    bookmark,
                    caption,
                    this.hyperlinkTracker);
                this.documentBody.AppendChild(p3);

            }

            Table t = DocHelper.CreateTable(headers);
            this.documentBody.Append(t);

            this.documentBody.Append(
                new Paragraph(
                    new ParagraphProperties(
                        new ParagraphStyleId()
                        {
                            Val = Properties.Settings.Default.BlankLinkText
                        })));        // Blank line added for spacing to next para

            return t;
        }

        public Table AddTable(string title, string[] headers, string bookmark = null, string caption = "Table ")
        {
            // Add a caption for the table
            if (!string.IsNullOrEmpty(title))
            {
                Paragraph p3 = DocHelper.CreateTableCaption(
                    tableCount++,
                    title,
                    bookmark,
                    caption);
                this.documentBody.AppendChild(p3);
            }

            Table t = DocHelper.CreateTable(
                DocHelper.CreateTableHeader(headers));

            this.documentBody.Append(t);

            this.documentBody.Append(
                new Paragraph(
                    new ParagraphProperties(
                        new ParagraphStyleId()
                        {
                            Val = Properties.Settings.Default.BlankLinkText
                        })));        // Blank line added for spacing to next para

            return t;
        }

        public void AddTable(string title, Table customTable, string bookmark = null, string caption = "Table ")
        {
            // Add a caption for the table
            if (!string.IsNullOrEmpty(title))
            {
                Paragraph p3 = DocHelper.CreateTableCaption(
                    tableCount++,
                    title,
                    bookmark,
                    caption);
                this.documentBody.AppendChild(p3);
            }

            this.documentBody.Append(customTable);

            this.documentBody.Append(
                new Paragraph(
                    new ParagraphProperties(
                        new ParagraphStyleId()
                        {
                            Val = Properties.Settings.Default.BlankLinkText
                        })));        // Blank line added for spacing to next para
        }
    }
}
