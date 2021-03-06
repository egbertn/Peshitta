﻿using System.Collections.Generic;

namespace Peshitta.Infrastructure.Sqlite.Model
{
    [System.Diagnostics.DebuggerDisplay("\"{word}\"")]
    public class words
	{
        public override bool Equals(object obj)
        {
            if (obj is words wrds)
            {
                return wrds.id == this.id;
            }
            return false;
        }
        public override int GetHashCode()
        {
            return id.Value;
        }
        public override string ToString()
        {
            return word;
        }

        public int? id { get; set; }
		public string word { get; set; }
		public bool IsSymbol { get; set; }
		public bool IsHtml { get; set; }
		public short LangId { get; set; }
		public bool IsNumber { get; set; }
		public int? number { get; set; }
        public int? hash { get; set; }
		public ICollection<TextWords> textwords { get; set; }
		public ICollection<TextWordsHistory> textwordsHistory { get; set; }
	}
}
