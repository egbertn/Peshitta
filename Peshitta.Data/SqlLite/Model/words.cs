using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Peshitta.Data.SqlLite.Model
{
	public class words
	{
		public words()
		{
			this.textwords = new HashSet<TextWords>();
			this.textwordsHistory = new HashSet<textwordsHistory>();
		}

		public short id { get; set; }
		public byte[] word { get; set; }
		public bool IsSymbol { get; set; }
		public bool IsHtml { get; set; }
		public short LangId { get; set; }
		public bool IsNumber { get; set; }
		public int? number { get; set; }

		public virtual ICollection<TextWords> textwords { get; set; }
		public virtual ICollection<textwordsHistory> textwordsHistory { get; set; }
	}
}
