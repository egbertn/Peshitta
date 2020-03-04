using System;
using System.Collections.Generic;

namespace Peshitta.Infrastructure.Sqlite.Model
{
	public class Text
	{
        public Text()
        {
            TextWords = new HashSet<TextWords>();
            TextWordsHistories = new HashSet<TextWordsHistory>();
        }
        public override bool Equals(object obj)
        {
            if (obj is Text txt)
            {
                return txt.textid == this.textid;
            }
            return false;
        }
        public override int GetHashCode()
        {
            return this.textid ;
        }
        public int textid { get; set; }
		public int BookChapterAlineaid { get; set; }
		public int Alineaid { get; set; }
		public int bookeditionid { get; set; }
		public DateTimeOffset timestamp { get; set; }
        public BookChapterAlinea bookchapteralinea { get; set; }
		public bookedition bookedition { get; set; }
	    public ICollection<TextWordsHistory> TextWordsHistories { get; set; }
        public ICollection<TextWords> TextWords { get; set; }

        //TODO subclass instead of ignore
        public string Content { get; set; }
        public string Remarks { get; set; }
        public string Header { get; set; }

    }
}
