﻿using System;
using System.Collections.Generic;

namespace Peshitta.Infrastructure.Sqlite.Model
{
	public class bookedition
	{
		public bookedition()
		{
			this.Text = new HashSet<Text>();
		}
		public int bookEditionid { get; set; }
		public int bookid { get; set; }
		public string publishercode { get; set; }
		public int? year { get; set; }
		public string isbn { get; set; }
		public short langid { get; set; }
		public string Copyright { get; set; }
		public string title { get; set; }
		public string EnglishTitle { get; set; }
		public string Author { get; set; }
		public string keywords { get; set; }
		public string description { get; set; }
		public string robots { get; set; }
		public DateTimeOffset? PressDate { get; set; }
		public int? forwordId { get; set; }
		public float Version { get; set; }
		public string subject { get; set; }
		public string nativeAbbreviation { get; set; }
		public short bookOrder { get; set; }
		public bool active { get; set; }

		public Book book { get; set; }
		public  ICollection<Text> Text { get; set; }
	}
}
