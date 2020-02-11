using System;
using System.Collections.Generic;

namespace Peshitta.Data.SqlLite.Model
{
	public class BookChapter
	{
		public BookChapter()
		{
			this.bookchapteralinea = new HashSet<bookchapteralinea>();
		}

		public int bookchapterid { get; set; }
		public int bookid { get; set; }
		public int chapter { get; set; }
		public string comments { get; set; }
		public virtual Book book { get; set; }
		public virtual ICollection<bookchapteralinea> bookchapteralinea { get; set; }
	}
}
