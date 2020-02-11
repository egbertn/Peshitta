using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Peshitta.Data.SqlLite.Model
{
	public class Text
	{
		public Text()
		{
			textwords = new HashSet<TextWords>();
			textwordsHistory = new HashSet<textwordsHistory>();
		}
		public int textid { get; set; }
		public int BookChapterAlineaid { get; set; }
		public int Alineaid { get; set; }
		public int bookeditionid { get; set; }
		public DateTimeOffset timestamp { get; set; }

		public virtual bookchapteralinea bookchapteralinea { get; set; }
		public virtual bookedition bookedition { get; set; }
		public virtual ICollection<TextWords> textwords { get; set; }
		public virtual ICollection<textwordsHistory> textwordsHistory { get; set; }
	}
}
