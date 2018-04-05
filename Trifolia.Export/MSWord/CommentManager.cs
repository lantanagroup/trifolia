using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Wordprocessing;

namespace Trifolia.Export.MSWord
{
    public class CommentManager
    {
        private Comments comments;
        private int commentId = 1;

        public CommentManager(Comments comments)
        {
            this.comments = comments;
        }

        public void AddCommentRange(Paragraph range, string comment)
        {
            Paragraph cmdPara = new Paragraph(new Run(new Text(comment)));
            Comment cmt = new Comment()
            {
                Id = this.commentId.ToString(),
                Author = "Trifolia",
                Initials = "TRIF",
                Date = DateTime.Now
            };
            cmt.AppendChild(cmdPara);
            this.comments.AppendChild(cmt);

            range.InsertBefore(new CommentRangeStart() { Id = this.commentId.ToString() }, range.GetFirstChild<Run>());
            var cmtEnd = range.InsertAfter(new CommentRangeEnd() { Id = this.commentId.ToString() }, range.Elements<Run>().Last());
            range.InsertAfter(new Run(new CommentReference() { Id = this.commentId.ToString() }), cmtEnd);

            this.commentId++;
        }
    }
}
