using Microsoft.EntityFrameworkCore;
using Peshitta.Data.SqlLite.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Peshitta.Data.SqlLite
{
	public class DbSqlContext : DbContext
	{
		public DbSqlContext(DbContextOptions<DbSqlContext> options)
		  : base(options)
		{
			
		}
		//protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		//{
		//	var conn = ConfigurationManager.ConnectionStrings["bijbel"].ConnectionString;
		//	Trace.TraceInformation(conn);
		//	optionsBuilder.UseSqlite(conn);
		//}

		public DbSet<Book> Books { get; set; }
		public DbSet<BookChapter> BookChapter { get; set; }
		public DbSet<bookchapteralinea> BookchapterAlinea { get; set; }
		public DbSet<Text> Text { get; set; }
		public DbSet<TextWords> TextWords { get; set; }
		public DbSet<TextWords> TextwordsHistory { get; set; }
		public DbSet<bookedition> BookEdition { get; set; }
		public DbSet<words> Words { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			var bookEnt = modelBuilder.Entity<Book>();
			bookEnt.ToTable("book");
			bookEnt.HasKey(k => k.bookid);
			bookEnt.Property(k => k.bookid).ValueGeneratedOnAdd();
			bookEnt.Property(p => p.Title).IsRequired().HasMaxLength(50);
			bookEnt.Property(p => p.abbrevation).IsRequired().HasMaxLength(8);

			var bookChapter = modelBuilder.Entity<BookChapter>();
			bookChapter.ToTable("bookchapter");
			bookChapter.HasKey(k => k.bookchapterid);

			var bca = modelBuilder.Entity<bookchapteralinea>();
			bca.HasKey(k => k.bookchapteralineaid);
			bca.ToTable("bookchapteralinea");

			var text = modelBuilder.Entity<Text>();
			text.HasKey(k => k.textid);

			var tw = modelBuilder.Entity<TextWords>();
			tw.HasKey(k => k.id);

			var twh = modelBuilder.Entity<textwordsHistory>();
			twh.HasKey(k => k.id);

			var w = modelBuilder.Entity<words>();
			w.HasKey(k => k.id);

			var bookedition = modelBuilder.Entity<bookedition>();
			bookedition.ToTable("bookedition");
			bookedition.HasKey(k => k.bookEditionid);
			bookedition.Property(p => p.description).HasMaxLength(1024);
			bookedition.Property(p => p.EnglishTitle).HasMaxLength(256);

			var textwords = modelBuilder.Entity<TextWords>();
			textwords.HasKey(k => k.id);
			textwords.Ignore(i => i.AddSpace)
			.Ignore(i => i.IsAllCaps)
			.Ignore(i => i.IsFootNote)
			.Ignore(i => i.AddDot)
			.Ignore(i => i.AddComma)
			.Ignore(i => i.IsHeader)
			.Ignore(i => i.LParentThesis)
			.Ignore(i => i.RParentThesis)
			.Ignore(i => i.LBracket)
			.Ignore(i => i.RBracket)
			.Ignore(i => i.LAngle)
			.Ignore(i => i.Rangle)
			.Ignore(i => i.AddColon)
			.Ignore(i => i.AddHyphenMin)
			.Ignore(i => i.RDQuote)
			.Ignore(i => i.LDQuote)
			.Ignore(i => i.RSQuote)
			.Ignore(i => i.LSQuote)
			.Ignore(i => i.AddLT)
			.Ignore(i => i.AddGT)
			.Ignore(i => i.AddSlash)
			.Ignore(i => i.AddBang)
			.Ignore(i => i.QMark)
			.Ignore(i => i.AddSlashAfter)
			.Ignore(i => i.AddEqual)
			.Ignore(i => i.AddAmp);

			var textwordsHistory = modelBuilder.Entity<textwordsHistory>();
			textwordsHistory.HasKey(k => k.id);
			textwordsHistory.Ignore(i => i.AddSpace)
			.Ignore(i => i.IsAllCaps)
			.Ignore(i => i.IsFootNote)
			.Ignore(i => i.AddDot)
			.Ignore(i => i.AddComma)
			.Ignore(i => i.IsHeader)
			.Ignore(i => i.LParentThesis)
			.Ignore(i => i.RParentThesis)
			.Ignore(i => i.LBracket)
			.Ignore(i => i.RBracket)
			.Ignore(i => i.LAngle)
			.Ignore(i => i.Rangle)
			.Ignore(i => i.AddColon)
			.Ignore(i => i.AddHyphenMin)
			.Ignore(i => i.RDQuote)
			.Ignore(i => i.LDQuote)
			.Ignore(i => i.RSQuote)
			.Ignore(i => i.LSQuote)
			.Ignore(i => i.AddLT)
			.Ignore(i => i.AddGT)
			.Ignore(i => i.AddSlash)
			.Ignore(i => i.AddBang)
			.Ignore(i => i.QMark)
			.Ignore(i => i.AddSlashAfter)
			.Ignore(i => i.AddEqual)
			.Ignore(i => i.AddAmp);

		}
	}
}
