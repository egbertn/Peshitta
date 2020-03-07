using System;
using System.Collections.Generic;

namespace Peshitta.Infrastructure.Models
{
    public sealed class TextExpanded : Text
    {
        public TextExpanded()
        {

        }
        public TextExpanded(Text t)
        {
            this.BookChapterAlineaid = t.BookChapterAlineaid;
            this.Alineaid = t.Alineaid;
            this.bookeditionid = t.bookeditionid;
            this.TextId = t.TextId;
            this.timestamp = t.timestamp;
            this.langid = t.langid;
        }
        /// <summary>
        /// is not serialized but filled on runtime
        /// </summary>
        public string Content { get; set; }

        public string Remarks { get; set; }

        public string Header { get; set; }

        public IEnumerable<(DateTime ArchiveDate, TextExpanded expanded)> Histories { get; set; }

    }
}
