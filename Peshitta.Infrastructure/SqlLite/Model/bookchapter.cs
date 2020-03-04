using System;
using System.Collections.Generic;

namespace Peshitta.Infrastructure.Sqlite.Model
{
	public class BookChapter
	{
		public BookChapter()
		{
			this.bookchapteralinea = new HashSet<BookChapterAlinea>();
		}

		public int bookchapterid { get; set; }
		public int bookid { get; set; }
		public int chapter { get; set; }
		public string comments { get; set; }
		public Book book { get; set; }
		public ICollection<BookChapterAlinea> bookchapteralinea { get; set; }
	}
}
