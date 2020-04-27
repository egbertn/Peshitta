using Microsoft.EntityFrameworkCore;
using Peshitta.Infrastructure.Sqlite.Model;
using System;

namespace Peshitta.Infrastructure.Sqlite
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
        #region modelling
       
        public DbSet<Publication> Publications { get; set; }
        public DbSet<Book> Books { get; set; }
		public DbSet<BookChapter> BookChapter { get; set; }
		public DbSet<BookChapterAlinea> BookchapterAlinea { get; set; }
		public DbSet<Text> Text { get; set; }
		public DbSet<TextWords> TextWords { get; set; }
		public DbSet<TextWordsHistory> TextwordsHistory { get; set; }
		public DbSet<bookedition> BookEdition { get; set; }
		public DbSet<words> Words { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Book>(p =>
            {
                p.ToTable("book");
                p.HasKey(k => k.bookid);
                p.Property(k => k.bookid).ValueGeneratedNever();
                p.Property(prop => prop.Title).IsRequired().HasMaxLength(50);
                p.Property(prop => prop.abbrevation).IsUnicode(false).IsRequired().HasMaxLength(8);
            });


            modelBuilder.Entity<BookChapter>(p =>
            {
                p.ToTable("bookchapter")
                .HasKey(k => k.bookchapterid);
                p.HasOne(e => e.book).WithMany(m => m.bookchapter).HasForeignKey(f => f.bookid);
                p.Property(prop => prop.bookchapterid).ValueGeneratedNever();
                
            });


            modelBuilder.Entity<BookChapterAlinea>(p =>
            {
                p.HasKey(k => new { k.bookchapteralineaid, k.Alineaid });
                p.ToTable("bookchapteralinea");
                p.Property(pr => pr.bookchapteralineaid).ValueGeneratedNever();
                p.HasOne(e => e.bookchapter).WithMany(m => m.bookchapteralinea).HasForeignKey(f => f.bookchapterid);               
              
            });


            modelBuilder.Entity<Text>(p =>
            {
                p.ToTable("text");
                p.HasKey(k => k.textid);
                p.HasOne(o => o.bookedition).WithMany(m => m.Text).HasForeignKey(f => f.bookeditionid);
                //p.Property(prop => prop.timestamp).HasColumnType("bigint").HasConversion(from => from.ToUnixTimeSeconds(), to => DateTimeOffset.FromUnixTimeSeconds(to));
                p.Property(prop => prop.timestamp).HasColumnType("bigint").HasConversion(from => from.ToUnixTimeSeconds(), to => DateTimeOffset.FromUnixTimeSeconds(to));
                p.HasOne(o => o.bookchapteralinea).WithMany(m => m.Texts).HasForeignKey(f => new { f.BookChapterAlineaid, f.Alineaid }).OnDelete(DeleteBehavior.NoAction);
                p.Property(prop => prop.textid).ValueGeneratedNever();
                //TODO subclass instead of this
                p.Ignore(i => i.Content).Ignore(i => i.Header).Ignore(i => i.Remarks);
            });

          
            modelBuilder.Entity<TextWords>(p =>
            {
                p.ToTable("textwords");
                p.HasKey(k => k.id);
                p.Property(k => k.id).ValueGeneratedNever();              
                p.HasOne(o => o.words).WithMany(m => m.textwords).HasForeignKey(f => f.wordid);
                p.HasOne(o => o.Text).WithMany(m => m.TextWords).HasForeignKey(f => f.textid);

            });
            

            modelBuilder.Entity<TextWordsHistory>( p =>
            {
                p.ToTable("textwordshistory");
                p.HasKey(k => k.id);
                p.Property(k => k.id).ValueGeneratedNever();
              //  p.HasIndex(i => new { i.textid, i.ArchiveDate }).HasName("ix_archive");
                p.Property(prop => prop.ArchiveDate).HasColumnType("bigint").HasConversion(from => from.ToUnixTimeMilliseconds() , to => DateTimeOffset.FromUnixTimeMilliseconds(to) );
                p.HasOne(o => o.words).WithMany(m => m.textwordsHistory).HasForeignKey(f => f.wordid);
                p.HasOne(o => o.Text).WithMany(m => m.TextWordsHistories).HasForeignKey(f => f.textid);               
            });
           

            modelBuilder.Entity<words>(p =>
            {
                p.ToTable("words");
                p.HasKey(k => k.id);
                p.HasIndex(k => new { k.word, k.LangId }).HasName("idx_words");
                p.Property(prop => prop.id).ValueGeneratedNever().IsRequired();
                p.Property(prop => prop.word).HasColumnType("nvarchar(100)");
            });
            

            modelBuilder.Entity<bookedition>(p => 
            {
                p.ToTable("bookedition");
                p.HasKey(k => k.bookEditionid);
                p.Property(prop => prop.bookEditionid).ValueGeneratedNever();
                p.Property(prop => prop.description).HasMaxLength(1024);
                p.Property(prop => prop.EnglishTitle).HasMaxLength(256);
                p.HasOne(o => o.book).WithMany(m => m.bookedition);
             
            });
            modelBuilder.Entity<Publication>(p => 
            {
                p.ToTable("publication")
                .HasKey(k => k.Code);
                p.Property(prop => prop.Code).HasColumnType("varchar(2)").HasMaxLength(2);
                p.Property(prop => prop.Name).IsRequired();
                p.Property(prop => prop.Date).HasDefaultValue(DateTimeOffset.Now);
                p.Property(prop => prop.Searchable).HasDefaultValue(true);
            });
            modelBuilder.Entity<TextWords>(p =>
            {
                p.ToTable("textwords");
                p.HasKey(k => k.id);
                p.Ignore(i => i.AddSpace)
                .Ignore(i => i.IsAllCaps)
                .Ignore(i => i.IsFootNote)
                .Ignore(i => i.AddDot)
                .Ignore(i => i.AddComma)
                .Ignore(i => i.IsHeader)
                .Ignore(i => i.LParentThesis)
                .Ignore(i => i.RParentThesis)
                .Ignore(i => i.LBracket)
                .Ignore(i => i.RBracket)
                .Ignore(i => i.Semicolon)
                .Ignore(i => i.IsCapitalized)
                .Ignore(i => i.PreSpace)
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
                .Ignore(i => i.PrefixAmp).Ignore(i => i.AddAmp);
            });


            modelBuilder.Entity<TextWordsHistory>(p =>
            {
                p.ToTable("textwordshistory");
                p.HasKey(k => k.id);
                p.Ignore(i => i.AddSpace)
                .Ignore(i => i.IsAllCaps)
                .Ignore(i => i.IsFootNote)
                .Ignore(i => i.AddDot)
                .Ignore(i => i.Semicolon)
                .Ignore(i => i.AddComma)
                .Ignore(i => i.IsHeader)
                .Ignore(i => i.LParentThesis)
                .Ignore(i => i.RParentThesis)
                .Ignore(i => i.LBracket)
                .Ignore(i => i.RBracket)
                .Ignore(i => i.IsCapitalized)
                .Ignore(i => i.PreSpace)
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
                .Ignore(i => i.AddAmp).Ignore(i => i.PrefixAmp);
            });
		
		}

        #endregion
      
       


    }
}
