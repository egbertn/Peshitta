using System.Collections.Generic;

namespace Peshitta.Data.SqlLite.Model
{
	public class Book
	{
		public Book()
		{
			this.bookchapter = new HashSet<BookChapter>();
			this.bookedition = new HashSet<bookedition>();
		}
		public int bookid { get; set; }

		public string Title { get; set; }

		public string abbrevation { get; set; }
		public ICollection<BookChapter> bookchapter { get; set; }
		public ICollection<bookedition> bookedition { get; set; }
	}
}
