using System.Collections.Generic;

namespace Peshitta.Data.SqlLite.Model
{
	public class bookchapteralinea
	{
		public bookchapteralinea()
		{
			this.Text = new HashSet<Text>();
		}

		public int bookchapteralineaid { get; set; }
		public int bookchapterid { get; set; }
		public int Alineaid { get; set; }
		public string comments { get; set; }

		public virtual BookChapter bookchapter { get; set; }
		public virtual ICollection<Text> Text { get; set; }
	}
}
