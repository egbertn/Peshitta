using Microsoft.EntityFrameworkCore;
using Peshitta.Infrastructure.Sqlite.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Peshitta.Infrastructure.Sqlite
{
    public class DbSqlContext : DbContext
	{
        private static readonly char[] splitters = new[] { ' ', '(', ')', ';', ':','#', ',', '.','‘', '“',
                                                            '”', '&', '\'', '-', '"', '[', ']', '?', '!', '<', '>',
                                                            '=', '/', '*', '\r', '\n', '„', '—', '%', '…', '·', '´'};

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
        private static ILookup<int, words> wordsCache;
        private static IDictionary<Models.WordLanguageKey, int> idCache;
        private static readonly object locker = new object();
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Book>(p =>
            {
                p.ToTable("book");
                p.HasKey(k => k.bookid);
                p.Property(k => k.bookid).ValueGeneratedNever();
                p.Property(prop => prop.Title).IsRequired().HasMaxLength(50);
                p.Property(prop => prop.abbrevation).IsRequired().HasMaxLength(8);
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
                p.HasOne(o => o.bookchapteralinea).WithMany(m => m.Texts).HasForeignKey(f => new { f.BookChapterAlineaid, f.Alineaid });
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
                p.Property(k => k.id);
              //  p.HasIndex(i => new { i.textid, i.ArchiveDate }).HasName("ix_archive");
                p.Property(prop => prop.ArchiveDate).HasColumnType("bigint").HasConversion(from => from.ToUnixTimeMilliseconds() , to => DateTimeOffset.FromUnixTimeMilliseconds(to) );
                p.HasOne(o => o.words).WithMany(m => m.textwordsHistory);
                p.HasOne(o => o.Text).WithMany(m => m.TextWordsHistories).HasForeignKey(f => f.textid);               
            });
           

            modelBuilder.Entity<words>(p =>
            {
                p.ToTable("words");
                p.HasKey(k => k.id);
                p.HasIndex(k => new { k.word, k.LangId }).HasName("idx_words");
                p.Property(prop => prop.id).ValueGeneratedOnAdd().IsRequired();               
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
        private static int CountChar(string st, char c)
        {
            var r = st.Length;
            var cnt = 0;
            while(r-- != 0)
            {
                if (st[r] == c) cnt++;
            }
            return cnt;
        }
        public ILookup<int, words> GetWordsCache()
        {   // no async stuff since we cannot do a lock + await

            var cache = this.Words.ToLookup(a => a.id.Value, p => p);
            //
            return cache;
            
        }
        private void UpdateWordsCache()
        {

            lock (locker)
            {
                wordsCache = GetWordsCache();
                //idCache = this.Words.Where(w => !w.IsNumber && !string.IsNullOrEmpty(w.word)).ToDictionary(w => new Models.WordLanguageKey( w.word, w.LangId), i => i.id.Value);
            }
        }
       
        public async Task<IEnumerable<Text>> GetVerseHistory(int textid)
        {
            if (wordsCache == null)
            {
                UpdateWordsCache();
            }
            var query = await this.Text.Where(w => w.textid == textid).
                Include(i => i.TextWordsHistories).FirstOrDefaultAsync();

            var cntHistories = query.TextWordsHistories.
                GroupBy(g =>  g.ArchiveDate ).Select(m => m.Key);
            var historyText = new List<Text>();
            foreach(var dt in cntHistories)
            {               
                if (query == null)
                {
                    return null;
                }
                var retVal = new Text()
                {
                    textid = query.textid,
                    BookChapterAlineaid = query.BookChapterAlineaid,
                    Alineaid = query.Alineaid,
                    timestamp = dt,
                    bookeditionid = query.bookeditionid
                };
                Decompress(retVal, query.TextWordsHistories.Where(w => w.ArchiveDate == dt).Select(s => s.ToTw()));
                historyText.Add(retVal);
            }
           
            return historyText;
        }
        public async Task<Text> DecompressVerse(int pTextId)
        {
            if (wordsCache == null)
            {
                UpdateWordsCache();
            }
            var query = await this.Text.Where(w => w.textid == pTextId).
                Select(s => new { 
                    s.textid, 
                    s.BookChapterAlineaid, 
                    s.Alineaid,
                    s.timestamp, 
                    s.bookeditionid, 
                    s.bookedition,
                    TextWords = s.TextWords.OrderBy(o => o.id).AsEnumerable() }).FirstOrDefaultAsync();
            if (query == null)
            {
                return null;
            }
            Text retVal = new Text()
            {
                textid = query.textid,
                BookChapterAlineaid = query.BookChapterAlineaid,
                Alineaid = query.Alineaid,
                timestamp = query.timestamp,
                bookeditionid = query.bookeditionid,
                bookedition = query.bookedition
            };
            Decompress(retVal, query.TextWords);
            return retVal;
        }
        public static void Decompress(Text t, IEnumerable<TextWords> tws)
        {

            var sb = new StringBuilder(512);
            // loop from 0 to 2 for respective text, footer, header decompression
            for (int cx = 2; cx >= 0; cx--)
            {
                sb.Length = 0;
                IEnumerable<TextWords> texts = null;
                if (cx == 0)
                {
                    texts = tws.Where(f => f.IsFootNote == false && f.IsHeader == false);
                }
                else if (cx == 1)
                {
                    texts = tws.Where(f => f.IsFootNote == true);
                }
                else if (cx == 2)
                {
                    texts = tws.Where(f => f.IsHeader == true);
                }

                foreach (var tw in texts)
                {

                    var wrd = wordsCache[tw.wordid].FirstOrDefault();
                   
                    bool qmarkDone = false;
                    bool LDQuoteDone = false, RDQuoteDone = false;
                    bool eqDone = false;
                    if (wrd != null)
                    {

                        if (tw.PrefixAmp && tw.Semicolon) //&amp; encoding
                        {
                            sb.Append('&');
                        }
                        if (tw.PreSpace)
                        {
                            sb.Append(' ');
                        }
                        if (tw.LParentThesis)
                        {
                            sb.Append('(');
                        }
                        else if (tw.LBracket)
                        {
                            sb.Append('[');
                        }
                        if (tw.AddLT)
                        {
                            sb.Append('<');
                            if (tw.AddSlash)
                                sb.Append('/');
                        }
                        if (tw.LSQuote)
                        {
                            sb.Append('\'');
                        }
                        if (tw.LDQuote && !LDQuoteDone)
                        {
                            sb.Append('"');
                        }

                        if (wrd.IsNumber)
                        {
                            sb.Append(wrd.number.Value);
                        }
                        else if (tw.IsAllCaps)
                        {
                            sb.Append(wrd.word.ToUpper());
                            // sb.Append(UnPackTags(wrd.tWordTags));
                        }
                        else if (tw.IsCapitalized)
                        {
                            string w = wrd.word;
                            sb.Append(char.ToUpper(w[0]));
                            sb.Append(w, 1, w.Length - 1);
                            //sb.Append(UnPackTags(wrd.tWordTags));
                        }
                        else
                        {
                            sb.Append(wrd.word);
                            //sb.Append(UnPackTags(wrd.tWordTags));
                        }

                        if (tw.AddEqual && tw.QMark)
                        {
                            eqDone = true;
                            sb.Append('=');
                            if (tw.RDQuote)
                            {
                                sb.Append('"');
                                RDQuoteDone = true;
                            }
                            else if (tw.LDQuote)
                            {
                                sb.Append('"');
                                LDQuoteDone = true;
                            }
                            if (tw.QMark)
                            {
                                sb.Append('?');
                                qmarkDone = true;
                            }
                        }
                        if (tw.AddBang)
                        {
                            sb.Append('!');
                        }
                        if (tw.QMark && !qmarkDone)
                        {
                            sb.Append('?');
                            qmarkDone = true;
                        }
                        if (tw.RSQuote)
                        {
                            sb.Append('\'');
                        }
                        else if (tw.AddDot)
                        {
                            sb.Append('.');
                        }
                        if (tw.AddColon)
                        {
                            sb.Append(':');
                        }
                        else if (tw.Semicolon)
                        {
                            sb.Append(';');
                        }
                        if (tw.AddEqual && !eqDone)
                        {
                            sb.Append('=');
                        }

                        if (tw.RDQuote && !RDQuoteDone)
                        {
                            sb.Append('"');
                        }
                        if (tw.AddSlashAfter)
                        {
                            sb.Append('/');
                        }
                        if (tw.RBracket)
                        {
                            sb.Append(']');
                        }
                        else if (tw.AddGT)
                        {
                            sb.Append('>');
                        }
                        else if (tw.RParentThesis)
                        {
                            sb.Append(')');
                        }

                        if (tw.AddComma)
                        {
                            sb.Append(',');
                        }
                        if (tw.AddSpace)
                        {
                            sb.Append(' ');
                        }
                        if (tw.AddHyphenMin)
                        {
                            sb.Append('-');
                        }
                        if (tw.PrefixAmp && !tw.Semicolon)
                        {
                            sb.Append('&'); // for &p=1&c=10 etc.
                        }

                    }
                }

                if (cx == 0)
                {
                    t.Content = sb.ToString();
                }
                else if (cx == 1)
                {
                    if (sb.Length > 0)
                    {
                        t.Remarks = sb.ToString();
                    }
                }
                else if (cx == 2)
                {
                    if (sb.Length > 0)
                    {
                        t.Header = sb.ToString();
                    }
                }
            };
        }
        
        public async Task<IEnumerable<Publication>> PublicationCodes()
        {           
            var result = await  Publications.Where(pc => pc.Searchable).ToArrayAsync();
            return result;           
        }

        public async Task<IList<DateTimeOffset>> GetVerseTimeStamps(IEnumerable<int> textids)
        {
            var result = await Text.Where(w => textids.Contains(w.textid)).Select(s => s.timestamp).ToArrayAsync();
            return result;
        }
        public async Task< bool> CompressVerse(int  textId,
            DateTimeOffset newTimeStamp,
                string pNewContent,
                string pNewRemark,
                string pHeaderText)
        {
           
            //string[] verseWords = content.Split(new[] { ' ' }, StringSplitOptions.None);
            //this.dcd.Transaction = dcd.Connection.BeginTransaction();
            var trans = await this.Database.BeginTransactionAsync();
            var t = await this.Text.                
                Include(i => i.TextWords).
                Include(i => i.bookedition).
                Include(i => i.bookchapteralinea).Where(w => w.textid == textId).FirstOrDefaultAsync();
          
            bool didAdd = false;
            if (t == null) return didAdd;
          
            
            //copy history
            var now = DateTimeOffset.Now;
            //tricky do not do direct edits
            var textWordsHistories = this.TextwordsHistory.AsNoTracking();
            var maxHistoryId = await textWordsHistories.AnyAsync() ? await textWordsHistories.MaxAsync(a => a.id) : 1;

            foreach (var twh in t.TextWords.OrderBy(i => i.id).Select(tw =>
                     new TextWordsHistory(tw, now, ++maxHistoryId)
                  ).AsEnumerable())
            {
                
                t.TextWordsHistories.Add(twh);
                
            }
            this.AddRange(t.TextWordsHistories);

            // this includes footer, header and body
            this.RemoveRange(t.TextWords);
            t.TextWords.Clear();
            var textWords = this.TextWords.AsNoTracking();
            var maxid = await textWords.AnyAsync() ? await textWords.MaxAsync(a => a.id) : 1;

            for (int x = 0; x < 3; x++)
            {
                int startAt = 0;
                int foundSplits = 0;

                // take the content text first than the footnote

                string content = null;

                switch (x)
                {
                    case 0:
                        content = pNewContent;
                        break;
                    case 1:
                        content = pNewRemark;                   
                        break;
                    case 2:
                        content = pHeaderText;
                        break;
                }
                if (string.IsNullOrEmpty(content))
                {
                    continue;
                }
                int lineLen2 = content.Length;
                string theWord = null;

                bool addLBracket = false; //ch == '[';
                bool preSpace = false;
                bool addRBracket = false; //]
                bool doAddLiteral = false;
                bool addRParenthesis = false, addLParenthesis = false;
                bool addComma = false;
                bool addDot = false;
                bool addSpace = false;
                bool addHyphen = false;
                bool addEqual = false;
                bool addColon = false, addSemiColon = false;
                bool addRSQuote = false, addLSQuote = false; // '
                bool addAmp = false;
                bool addRDQuote = false, addLDQuote = false; // "
                bool addLT = false, addGT = false; // < and >
                bool addBang = false, addQMark = false;
                bool addSlash = false, addSlashAfter = false; // / for </b> for instance
                for (; ; )
                {
                    foundSplits = content.IndexOfAny(splitters, startAt);
                    // this normally happens at the very last word of a line
                    char foundChar = '\0';

                    // if a line ends without any splitter, we must take the remainer!
                    if (foundSplits < 0 && startAt < lineLen2)
                    {
                        foundSplits = lineLen2;
                    }

                    if (foundSplits < 0) break;
                    else if (foundSplits < lineLen2)
                        foundChar = content[foundSplits]; // end of line

                    int wordLen = foundSplits - startAt;
                    theWord = content.Substring(startAt, wordLen);
                    // this is the symbol which caused a split on the word

                    //there is a symbol? 
                    // deal with ( [ ' and "
                    // this section deals with symbols before any word such as ( [ - and space
                    if (wordLen == 0)
                    {
                        char ch = foundChar;
                        preSpace = ch == ' ';
                        addHyphen = ch == '-';
                        addColon = ch == ':';
                        addLBracket = ch == '[';
                        addLParenthesis = ch == '(';
                        addLSQuote = ch == '\'';
                        addBang = ch == '!';
                        addQMark = ch == '?';
                        addLDQuote = ch == '"';
                        addEqual = ch == '=';
                        addAmp = ch == '&'; //e.g. &amp; or &quote;
                        addLT = ch == '<';

                        if (addLBracket || addLParenthesis || addEqual && (startAt + 1 < lineLen2))
                        {
                            addLDQuote = content[startAt + 1] == '"';

                            if (addLDQuote && (startAt + 1 < lineLen2)) // fix for ="?booked=10" etc...
                            {
                                addQMark = content[startAt + 1] == '?';
                                if (addQMark)
                                {
                                    startAt++;
                                }
                            }
                            addLSQuote = content[startAt + 1] == '\'';
                            if (addLDQuote || addLSQuote)
                            {
                                startAt++;
                            }
                        }
                        if (addLT && startAt + 1 < lineLen2)
                        {
                            addSlash = content[startAt + 1] == '/';
                            if (addSlash) startAt++;
                        }
                        else if (startAt > lineLen2)
                        {
                            break;
                        }
                        if (addHyphen)
                        {
                            addSpace = content[startAt + 1] == ' ';
                            if (addSpace)
                            {
                                startAt++;
                            }

                        }
                        //only if we can map a char but there is no word right after (as in aller-snelste)
                        if (preSpace || addHyphen || addColon || addLBracket || addLParenthesis ||
                            addLSQuote ||
                            addLDQuote ||
                            addLT)
                        {


                            if (addHyphen)
                            {
                                //ugly workaround
                                //if the - is in the middle of a word like this
                                // He - not knowing that
                                foundChar = '-';
                                addHyphen = false;
                                doAddLiteral = true;
                            }
                            else
                            {
                                //our prefetch succeeded. Look further for / or a complete word (as in [hello] or </b)
                                startAt++;
                                continue;
                            }
                        }
                        else
                        {
                            doAddLiteral = true;
                        }
                    }
                    // this section deals with symbols after a word, such as ., ? ! : and ; etc.
                    else if (wordLen > 0)
                    {
                        char ch = foundChar; 
                   
                        //compressing space, comma and dot delivers a compressionrate of +/- 20%                       
                        addComma = ch == ',';
                        addDot = ch == '.';
                        if (!addSpace) addSpace = ch == ' ';
                        addHyphen = ch == '-';
                        if (!addColon) addColon = ch == ':';
                        addRBracket = ch == ']';
                        addRParenthesis = ch == ')';
                        addRSQuote = ch == '\'';
                        addSlashAfter = ch == '/';
                        addRDQuote = ch == '"';
                        addGT = ch == '>';
                        addBang = ch == '!';
                        addQMark = ch == '?';
                        addSemiColon = ch == ';';
                        addEqual = ch == '=';
                        addAmp = ch == '&';
                        //addAmp = ch == '&'; //for bl=1&p=1 etc
                        bool mappedSymbol = addSemiColon || addHyphen || addBang || addRBracket || addRParenthesis || addColon ||
                              addComma || addRBracket || addDot || addGT || addRDQuote || addRSQuote || addQMark || addSlashAfter || addEqual || addAmp;

                        if (mappedSymbol || addSpace)
                            foundSplits++;


                        if (startAt + wordLen + 1 < lineLen2)
                        {
                            int lookahead = 1;
                            // we allow !" + space, ." + space, and !' + space, .' + space
                            if (addSlashAfter)
                            {
                                addGT = content[startAt + wordLen + lookahead] == '>';
                                if (addGT)
                                {
                                    lookahead++;
                                    foundSplits++;
                                }
                            }
                            if (addRSQuote) //'if ' then look for "
                            {
                                addRDQuote = content[startAt + wordLen + lookahead] == '"';
                                if (addRDQuote)
                                {
                                    lookahead++;
                                    foundSplits++;
                                }
                            }
                            if (startAt + wordLen + lookahead < lineLen2 && (addBang || addDot || addQMark || addEqual || addAmp))
                            {
                                if (!addRDQuote) addRDQuote = content[startAt + wordLen + lookahead] == '"';
                                if (!addRSQuote) addRSQuote = content[startAt + wordLen + lookahead] == '\'';

                                addRBracket = content[startAt + wordLen + lookahead] == ']';
                                addRParenthesis = content[startAt + wordLen + lookahead] == ')';

                                if (addRSQuote || addRDQuote || addRParenthesis || addRBracket)
                                {
                                    foundSplits++;
                                    lookahead++;
                                    mappedSymbol = true;
                                }
                                if (!addQMark && addRDQuote && (startAt + wordLen + lookahead < lineLen2)) // fix for ="?booked=10" etc...
                                {
                                    addQMark = content[startAt + wordLen + lookahead] == '?';
                                    if (addQMark)
                                    {
                                        foundSplits++;
                                        lookahead++;
                                        mappedSymbol = true;
                                    }
                                }
                                if (startAt + wordLen + lookahead < lineLen2 && (addRSQuote || addRDQuote))
                                {
                                    if (!addRDQuote)
                                    {
                                        addRDQuote = content[startAt + wordLen + lookahead] == '"'; // we have !'"
                                        if (addRDQuote)
                                        {
                                            foundSplits++;
                                            lookahead++;
                                        }
                                    }
                                    if (startAt + wordLen + lookahead + 1 < lineLen2)
                                    {
                                        addRBracket = content[startAt + wordLen + lookahead] == ']'; // we have !'"]
                                        if (!addGT) addGT = content[startAt + wordLen + lookahead] == '>'; // we have '">
                                        addRParenthesis = content[startAt + wordLen + lookahead] == ')';
                                        if (addRBracket || addRParenthesis || addGT)
                                        {
                                            lookahead++;
                                            foundSplits++;
                                            mappedSymbol = true;
                                        }
                                    }
                                }
                            }
                            if (mappedSymbol && !addAmp && startAt + wordLen + lookahead + 1 < lineLen2)
                            {

                                addAmp = content[startAt + wordLen + lookahead] == '&';
                                if (addAmp)
                                {
                                    lookahead++;
                                    foundSplits++;
                                }

                            }
                            if (mappedSymbol && !addSpace && startAt + wordLen + lookahead + 1 < lineLen2)
                            {

                                addSpace = content[startAt + wordLen + lookahead] == ' ';
                                if (addSpace)
                                {
                                    lookahead++;
                                    foundSplits++;
                                }

                            }
                            else if (addRSQuote || addRDQuote)
                            {
                                // we could have '"
                                if (addRSQuote)
                                {
                                    if (startAt + wordLen + lookahead + 1 < lineLen2)
                                    {
                                        addRDQuote = content[startAt + wordLen + lookahead] == '"';
                                        if (addRDQuote)
                                        {
                                            lookahead++;
                                            foundSplits++;
                                        }
                                    }
                                }
                                if (startAt + wordLen + lookahead + 1 < lineLen2)
                                {
                                    addComma = content[startAt + wordLen + lookahead] == ',';
                                    if (addComma)
                                    {
                                        lookahead++;
                                        foundSplits++;
                                    }
                                }
                                if (startAt + wordLen + lookahead + 1 < lineLen2)
                                {
                                    addSpace = content[startAt + wordLen + lookahead] == ' ';
                                    if (addSpace)
                                    {
                                        foundSplits++;
                                    }
                                }
                            }
                            // [: or (; ????
                            else if (addRBracket || addRParenthesis)
                            {
                                addColon = content[startAt + wordLen + lookahead] == ':';
                                if (addColon)
                                {
                                    mappedSymbol = true;
                                    foundSplits++;
                                }
                            }

                            //we allow symbol + space                                               
                            //if (mappedSymbol && !addSpace)
                            //{
                            //    if (startAt + wordLen + lookahead > lineLen2)
                            //    {
                            //        addSpace = content[startAt + wordLen + lookahead] == ' ';
                            //        if (addSpace)
                            //            foundSplits++;
                            //    }
                            //}
                            //we allow space + symbol
                            if (!addHyphen && addSpace)
                            {
                                addHyphen = content[startAt + wordLen + 1] == '-';
                                if (addHyphen)
                                {
                                    foundSplits++;
                                }
                            }
                        }
                        if ((maxid = await CompressRange(t, theWord, addSpace, addDot, addComma, x == 1, x == 2,
                            addColon, addSemiColon, addHyphen, addLBracket, addRBracket, addRParenthesis,
                            addLParenthesis, addLSQuote, addRSQuote, addLDQuote, addRDQuote, addLT, addGT, addSlash, addBang,
                            preSpace, addQMark, addSlashAfter, addEqual, addAmp, t.bookedition.langid, maxid)) > 0)
                        {
                            didAdd = true;

                        }
                        if (false)
                        {

                        }
                        addAmp = preSpace = addEqual = addRDQuote = addDot = addRSQuote = addRBracket =
                         addRParenthesis = addBang = addSlash =
                         addHyphen = addSemiColon = addColon =
                         addLBracket = addLParenthesis =
                         addLSQuote = addLDQuote = addSlashAfter =
                         addSpace = addGT = addLT = false;
                    }

                    //non mapped symbols go here!
                    // if the symbol also is followed by a space or dot it will be 'compressed'
                    if (doAddLiteral)
                    {
                        doAddLiteral = false;
                        char testChar = '0';

                        if (foundSplits + 1 < lineLen2)
                        {
                            testChar = content[foundSplits + 1];
                            addDot = testChar == '.';
                            addComma = testChar == ',';
                            //addGT = testChar == '>';
                            //addHyphen = testChar == '-';
                            //addRBracket = testChar == ']';
                            //addRDQuote = testChar == '"';
                            //addRSQuote = testChar == '\'';
                            //addLParenthesis = testChar == ')';
                            //addColon = testChar == ':';
                            if (!addSpace)
                            {
                                addSpace = testChar == ' ';
                            }
                            // look ahead for the case of ' or "[space]
                            // now this will be fine
                            //eg. 'Abraham', a child of God. You see, ' will be seperately encoded. 
                            // However, ' is followed by a comma and a space. The ' needs two bits set on, the comma and the space.
                            // deal with <span title="Jezus">Jeshua</span>, blah
                            if (foundSplits + 2 < lineLen2)
                            {
                                //if (addComma || addGT || addHyphen || addRBracket || addLParenthesis || addColon || addDot || addRSQuote || addRDQuote)
                                if (!addSpace)
                                {
                                    char testChar2 = content[foundSplits + 2];
                                    addSpace = testChar2 == ' ';
                                    if (addSpace)
                                        foundSplits++;
                                }

                                addRSQuote = content[foundSplits + 2] == '\'';
                                addRDQuote = content[foundSplits + 2] == '"';
                                if (addRSQuote || addRDQuote)
                                {
                                    foundSplits++;
                                }

                            }
                        }

                        if (foundChar > '\0' && 
                            (maxid = await CompressRange(t, 
                            new string(foundChar, 1), addSpace, addDot, addComma, x == 1, x == 2,
                            false, false,
                            false, false, false, false, false, false, 
                            addRSQuote, false, addRDQuote, false, false, false, false, 
                            false, false, false, false, false, t.bookedition.langid, maxid))> 0)
                        {
                            didAdd = true;

                        }
                        addRDQuote = addRSQuote = false;
                        foundSplits++;
                    }
                    startAt = foundSplits;
                }
            }
            t.timestamp = newTimeStamp;
           this.Update(t);
            await this.SaveChangesAsync();
        //  await this.Database.ExecuteSqlRawAsync("UPDATE text SET timestamp={1} WHERE textid={0}", textId, new DateTimeOffset(newTimeStamp).ToUnixTimeSeconds());
            await trans.CommitAsync();


            await trans.DisposeAsync();
            return didAdd;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="textid"></param>
        /// <param name="w">The single word to be converted to a number</param>
        private  async Task<words> FindWord(string w, int langId)
        {
            IsCapitalized(w, out bool capitalized, out bool allCaps);
            if (capitalized || allCaps)
            {
                w = w.ToLowerInvariant();
            }
            // numbers can become huge, and thus, waste space!
            bool isNumber = int.TryParse(w, out int number);
            if (idCache == null)
            {
                UpdateWordsCache();
            }
            var key = new Models.WordLanguageKey(w, langId);
            if (idCache.ContainsKey(key))
            {
                return new words { id = idCache[key], word = w };
            }

            var foundWord = isNumber ? await this.Words.SingleOrDefaultAsync(a => a.number == number && a.LangId == langId) :
                await this.Words.FromSqlRaw("SELECT * FROM words WHERE word = {0} AND LangId = {1} LIMIT 1", w, langId).FirstOrDefaultAsync();
            return foundWord;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="textid"></param>
        /// <param name="w">The single word to be converted to a number</param>
        /// <returns> the new maxid from textwords</returns>
        private async Task<int> CompressRange(Text t, string w,
            bool pAddSpace,
            bool addDot,
            bool addComma,
            bool pIsFootNote,
            bool pIsHeader,
            bool isColon,
            bool bAddSemiColon,
            bool pIsHyphen,
            bool paddLBracket,
            bool ppaddRBracket,
            bool paddRParenthesis,
            bool paddLParenthesis,
            bool paddLSQuote,
            bool paddRSQuote,
            bool paddLDQuote,
            bool paddRDQuote,
            bool paddLT,
            bool paddGT,
            bool paddSlash,
            bool paddBang, bool addPreSpace, bool pAddQMark, bool pAddSlashAfter, bool pAddEqual,
            bool pAddAmp, int langid, int maxid
            )
        {
            //short wordid = 0;
            // we only find case sensitve searches
            bool didAdd = false;
            IsCapitalized(w, out bool capitalized, out bool allCaps);
            if (capitalized || allCaps)
            {
                w = w.ToLower();
            }
            int number = 0;
            // numbers can become huge, and thus, waste space!
            bool isNumber = int.TryParse(w, out number);
            var foundWord = await FindWord(w, langid);
            
            if (foundWord == null)
            {
                foundWord = new words()
                {
                    IsNumber = isNumber,
                    //TODO: make language 
                    LangId =(short) langid
                };
                if (isNumber)
                {
                    foundWord.number = number;
                }
                else
                {
                    foundWord.word = w;
                }
                 this.Words.Add(foundWord);
                 await this.SaveChangesAsync();
                didAdd = true;
            }
            var wordid = foundWord.id.Value;
            var tw = new TextWords()
            {
                id = ++maxid,
                textid = t.textid,
                IsAllCaps = allCaps,
                AddSpace = pAddSpace,
                IsFootNote = pIsFootNote,
                IsHeader = pIsHeader,
                wordid = wordid,
                IsCapitalized = capitalized,
                AddComma = addComma,
                AddDot = addDot,
                AddColon = isColon,
                AddHyphenMin = pIsHyphen,
                LSQuote = paddLSQuote,
                RSQuote = paddRSQuote,
                RDQuote = paddRDQuote,
                LDQuote = paddLDQuote,
                RParentThesis = paddRParenthesis,
                LParentThesis = paddLParenthesis,
                AddGT = paddGT,
                AddLT = paddLT,
                AddSlash = paddSlash,
                AddBang = paddBang,
                LBracket = paddLBracket,
                Semicolon = bAddSemiColon,
                RBracket = ppaddRBracket,
                PreSpace = addPreSpace,
                QMark = pAddQMark,
                AddSlashAfter = pAddSlashAfter,
                AddEqual = pAddEqual,
                PrefixAmp = pAddAmp


            };
            //tw.word = foundWord;            
            t.TextWords.Add(tw);
            return maxid;
        }
        private static void IsCapitalized(string word, out bool pIsCapitalized, out bool pIsAllCaps)
        {
            pIsCapitalized = false;
            pIsAllCaps = true;
            if (!string.IsNullOrEmpty(word))
            {
                int wordLen = word.Length;
                for (int x = 0; x < wordLen; x++)
                {
                    if (char.IsUpper(word[x]))
                    {
                        //switch of. This is not a word like God or 'In' or 'Jahweh' or 'And' but it might have been God or LORD
                        if (x > 0)
                        {
                            pIsCapitalized = false;
                        }
                        else if (x == 0)
                        {
                            pIsCapitalized = true;
                        }
                    }
                    else
                    {
                        pIsAllCaps = false;
                    }
                }
                if (pIsAllCaps && wordLen == 1)
                {
                    pIsAllCaps = false; //capitalized is enough
                }
            }
        }


    }
}
