using System.Collections.Generic;

namespace Peshitta.Infrastructure.Sqlite.Model
{
	public class BookChapterAlinea
	{
		
		public int bookchapteralineaid { get; set; }
		public int bookchapterid { get; set; }
		public int Alineaid { get; set; }
		public string comments { get; set; }

		public BookChapter bookchapter { get; set; }
        public ICollection<Text> Texts { get; set; }
	}
}
