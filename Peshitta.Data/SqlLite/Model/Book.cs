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
		public virtual ICollection<BookChapter> bookchapter { get; set; }
		public virtual ICollection<bookedition> bookedition { get; set; }
	}
}
