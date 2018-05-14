using Peshitta.Data.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Linq;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;


//#pragma warning disable 219
namespace peshitta.nl.DB
{



    //[DataContract(Namespace = "http://adccure.nl")]
    public sealed class VerseInfo
    {
        public string Book { get; set; }
        public string BookEnglish { get; set; }
        public int Verse { get; set; }
        public int Chapter { get; set; }
        public int TextId { get; set; }
    }
    // used to group on
    public sealed class AlineaText
    {
        public int BookChapterAlineaId { get; set; }
        public int Alineaid { get; set; }
        public IEnumerable<Text> Texts { get; set; }
    }
    public partial class Text
    {
        //denormalize
        public int langid { get; set; }
        public string LanguageCode
        {
            get
            {
                return bookedition.translang(langid);
            }
        }

        public string Content { get; set; }
        public string Remarks { get; set; }
        public string Header { get; set; }
        public string Synonyms { get; set; }
    }
    public sealed class BookChapter
    {
        public int chapter { get; set; }
        public Collection<BookChaptersJoined> BookChaptersJoinedCollection { get; set; }
    }

    public partial class bookedition
    {
        internal static string translang(int langid)
        {
            switch (langid)
            {
                case 90:
                    return "syc";
                case 19:
                    return "nl";
                default:
                    return "en";
            }
        }
        public string LanguageCode
        {
            get
            {
                return translang(this.langid);
            }
        }
    }
    public sealed partial class word
    {
        private string _cache;
        /// <summary>
        /// yeah!
        /// </summary>
        public string word1
        {

            get
            {
                if (this.wordX == null || this.wordX.Length == 0) return "";
                if (_cache == null)
                {
                    _cache = Encoding.Unicode.GetString(this.wordX.ToArray());
                }
                return _cache;
            }
            set
            {
                this.wordX = new Binary(Encoding.Unicode.GetBytes(value));
                this._cache = value;
            }
        }
    }

  
    public sealed class BookChaptersJoined
    {
        public int bookchapterAlineaId { get; set; }
        public int alineaId { get; set; }
    }
    //public partial class word
    //{
    //    private List<word> _seotags;
    //    partial void OnLoaded()
    //    {

    //    }
    //    public List<word> SeoTags
    //    {
    //        set
    //        {
    //            _seotags = value;
    //        }
    //        get
    //        {
    //            if (_seotags == null)
    //            {
    //                _seotags = new List<word>();
    //            }
    //            return _seotags;
    //        }
    //    }
    //}
    /// <summary>
    /// Summary description for db
    /// </summary>
    public sealed class BookDal : IDisposable
    {
        private static readonly char[] splitters = new[] { ' ', '(', ')', ';', ':','#', ',', '.','‘', '“',
                                                            '”', '&', '\'', '-', '"', '[', ']', '?', '!', '<', '>',
                                                            '=', '/', '*', '\r', '\n', '„', '—', '%', '…', '·', '´'};
        private readonly Entities dcd;
        private static ILookup<short, word> wordsCache;
        //private static List<textword_tiny> textwordCache;
        private static ILookup<short, IEnumerable<short>> synonymCache;
        private static readonly object twlocker = new object();
        internal static readonly IDictionary<string, ILookup<string, int>> BookToIdByTitle = new Dictionary<string, ILookup<string, int>>();
        internal static readonly IDictionary<string, ILookup<string, int>> BookToIdByAbr = new Dictionary<string, ILookup<string, int>>();
        private static readonly object wordsCacheLocker = new object();
   

        /// <summary>
        /// needed to avoid Compiled queries across DataContexts with different LoadOptions not supported
        /// </summary>
        private static readonly DataLoadOptions Prefetch = (new Func<DataLoadOptions>(() =>
        {
            DataLoadOptions dl = new DataLoadOptions();

            //dl.LoadWith<WordTag>(t => t.word1);
            return dl;
        }))();


        public BookDal()
        {
            dcd = new Entities();
            dcd.Connection.Open();
            dcd.LoadOptions = Prefetch;

        }

        public void Dispose()
        {
            if (dcd.Connection.State == ConnectionState.Open)
                dcd.Connection.Close();
            //dcd.Connection.Dispose();
            dcd.Dispose();
        }
        public All SerializeDB(params string[] pubCodes)
        {
            var retVal = new All
            {
              PublicationCodes = dcd.PublicationCodes.Select(s => new Peshitta.Data.Models. PublicationCode {Code = s.publicationCode, Name = s.Name, Searchable = s.searchable }),
              Languages = dcd.Languages.Select(s =>  new Peshitta.Data.Models.Language { CultureCode = s.CultureCode, FontName = s.FontName, Langid = s.Langid, Language1 = s.Language1}),
                BookChapterAlineas = dcd.bookchapteralineas.ToDictionary(i => new BookChapterAlineaKey( i.bookchapteralineaid, i.Alineaid),
                    k => new BookChapterAlinea
                    {
                        AlineaId = k.Alineaid,
                        BookchapterAlineaId = k.bookchapteralineaid,
                        BookchapterId = k.bookchapterid,
                        Comments = k.comments
                    } ),
                BookChapters = dcd.bookchapters.ToDictionary(i => i.bookchapterid, k => new
                       Peshitta.Data.Models.BookChapter
                {
                    bookchapterid = k.bookchapterid,
                    bookid = k.bookid,
                    chapter = k.chapter,
                    comments = k.comments
                }),
                
                BookEditions = dcd.bookeditions.Where(p => pubCodes.Contains(p.publishercode)).ToDictionary(i => i.bookEditionid, k => new BookEdition
                {
                    active = k.active,
                    Author = k.Author,
                    bookEditionid = k.bookEditionid,
                    bookOrder = k.bookOrder,
                    bookid = k.bookid,
                    Copyright = k.Copyright,
                    description = k.description,
                    EnglishTitle = k.EnglishTitle,
                    isbn = k.isbn,
                    keywords = k.keywords?.Split(' ', ','),
                    langid = k.langid,
                    PressDate = k.PressDate,
                    publishercode = k.publishercode,
                    robots = k.robots,
                    subject = k.subject,
                    title = k.title,
                    Version = k.Version,
                    year = k.year
                }),
                Words = dcd.words.ToDictionary(i => (int)i.id, k => new words
                {
                    id = k.id,
                    IsNumber = k.IsNumber.BoolToNullable(),
                    IsSymbol = k.IsSymbol.BoolToNullable(),
                    LangId = k.LangId,
                    number = k.number,
                    word = k.word1
                }),
                Books = dcd.books.ToDictionary(i => i.bookid, k => new Peshitta.Data.Models.Book
                {
                    bookid = k.bookid,
                    abbreviation = k.abbreviation,
                    Title = k.Title
                })
            };
            var tws = dcd.textwords.ToArray();
            var twsh = dcd.textwordsHistories.ToArray();
            foreach (var uniqPub in retVal.BookEditions.Select(s => s.Value.publishercode).Distinct())
            {
          
                var bookeditions = dcd.bookeditions.Where(w => w.publishercode == uniqPub).Select(s => s.bookEditionid).ToArray();
                var textIds = dcd.Texts.Where(w => bookeditions.Contains(w.bookeditionid)).Select(s => s.textid).ToArray();
                var p = new Publication()
                {
                    TextWords = tws.Where(w => textIds.Contains(w.textid)).ToLookup(i => i.textid, k => new TextWords
                    {
                        AddAmp = default(bool?),
                        AddBang = k.AddBang.BoolToNullable(),
                        AddColon = k.AddColon.BoolToNullable(),
                        AddComma = k.AddComma.BoolToNullable(),
                        AddDot = k.AddDot.BoolToNullable(),
                        AddEqual = k.AddEqual.BoolToNullable(),
                        AddGT = k.AddGT.BoolToNullable(),
                        AddHyphenMin = k.AddHyphenMin.BoolToNullable(),
                        AddLT = k.AddLT.BoolToNullable(),
                        AddSlash = k.AddSlash.BoolToNullable(),
                        AddSlashAfter = k.AddSlashAfter.BoolToNullable(),
                        AddSpace = k.AddSpace.BoolToNullable(),
                        textid = k.textid,
                        wordid = k.wordid,
                        id = k.id,
                        IsAllCaps = k.IsAllCaps.BoolToNullable(),
                        IsCapitalized = k.IsCapitalized.BoolToNullable(),
                        IsFootNote = k.IsFootNote.BoolToNullable(),
                        IsHeader = k.IsHeader.BoolToNullable(),
                        LBracket = k.LBracket.BoolToNullable(),
                        LDQuote = k.LDQuote.BoolToNullable(),
                        LSQuote = k.LSQuote.BoolToNullable(),
                        LParentThesis = k.LParentThesis.BoolToNullable(),
                        QMark = k.QMark.BoolToNullable(),
                        RBracket = k.RBracket.BoolToNullable(),
                        RDQuote = k.RDQuote.BoolToNullable(),
                        RParentThesis = k.RParentThesis.BoolToNullable(),
                        RSQuote = k.RSQuote.BoolToNullable(),
                    }),
                    HistoryDates = twsh.Where(w => textIds.Contains(w.textid)).Select(s => new HistoryKey(s.textid, s.ArchiveDate)).Distinct().ToLookup(i => i.id, k => k.ArchiveDate),
                    TextWordsHistory = twsh.Where(w => textIds.Contains(w.textid)).ToLookup(i =>
                        new HistoryKey(i.textid, i.ArchiveDate),
                    k => new TextWordsHistory
                    {
                        AddAmp = default(bool?),
                        id = k.id,
                        textid = k.textid,
                        wordid = k.wordid,
                        ArchiveDate = k.ArchiveDate,
                        AddBang = k.AddBang.BoolToNullable(),
                        AddColon = k.AddColon.BoolToNullable(),
                        AddComma = k.AddComma.BoolToNullable(),
                        AddDot = k.AddDot.BoolToNullable(),
                        AddEqual = k.AddEqual.BoolToNullable(),
                        AddGT = k.AddGT.BoolToNullable(),
                        AddHyphenMin = k.AddHyphenMin.BoolToNullable(),
                        AddLT = k.AddLT.BoolToNullable(),
                        AddSlash = k.AddSlash.BoolToNullable(),
                        AddSlashAfter = k.AddSlashAfter.BoolToNullable(),
                        AddSpace = k.AddSpace.BoolToNullable(),
                        IsAllCaps = k.IsAllCaps.BoolToNullable(),
                        IsCapitalized = k.IsCapitalized.BoolToNullable(),
                        IsFootNote = k.IsFootNote.BoolToNullable(),
                        IsHeader = k.IsHeader.BoolToNullable(),
                        LBracket = k.LBracket.BoolToNullable(),
                        LDQuote = k.LDQuote.BoolToNullable(),
                        LSQuote = k.LSQuote.BoolToNullable(),
                        LParentThesis = k.LParentThesis.BoolToNullable(),
                        QMark = k.QMark.BoolToNullable(),
                        RBracket = k.RBracket.BoolToNullable(),
                        RDQuote = k.RDQuote.BoolToNullable(),
                        RParentThesis = k.RParentThesis.BoolToNullable(),
                        RSQuote = k.RSQuote.BoolToNullable()
                    }),

                    Texts = dcd.Texts.Where(w => bookeditions.Contains(w.bookeditionid)).ToDictionary(i => i.textid,
                            k => new Peshitta.Data.Models.Text
                            {
                                TextId = k.textid,
                                Alineaid = k.Alineaid,
                                langid = (short)k.bookedition.langid,
                                BookChapterAlineaid = k.BookChapterAlineaid,
                                bookeditionid = k.bookeditionid,
                                timestamp = k.timestamp
                            })
                };
                retVal.Pubs.Add(uniqPub, p);
            }
           
            return retVal;
        }
        
        public static void UpdateWordsCache(BookDal dcd)
        {

            lock (wordsCacheLocker)
            {
                wordsCache = dcd.GetWordsCache;
                synonymCache = dcd.GetSynonymCache;
                BookToIdByTitle["AB"] = dcd.GetBooksCache("AB");
                BookToIdByAbr["AB"] = dcd.GetBooksCacheAbr("AB");
            }
        }
        public ILookup<string, int> GetBooksCache(string pubid)
        {

            return dcd.bookeditions.Where(a => a.publishercode == pubid && a.active).OrderBy(o => o.bookOrder)
                .Select(s => new {s.bookid, s.title }).ToLookup(l => l.title, l => l.bookid);

        }
        public ILookup<string, int> GetBooksCacheAbr(string pubid)
        {

            return dcd.bookeditions.Where(a => a.publishercode == pubid && a.active).OrderBy(o => o.bookOrder)
                .Select(s => new {s.bookid, abreviation = s.book.abbreviation }).ToLookup(l => l.abreviation, l => l.bookid);

        }
        public ILookup<short, IEnumerable<short>> GetSynonymCache
        {
            get
            {//TODO: ILookup<short, List<word>
                return dcd.Synonyms.GroupBy(g => g.wordid, g => g.synonym_wordid, (wordid, listofwords) => new { w = wordid, s = listofwords }).ToLookup(els => els.w, key => key.s);
            }
        }
        //private List<textword_tiny> GetTWCache
        //{
        //    get
        //    {
        //        //todo time delay of 4 hours
        //        if (textwordCache == null)
        //        {
        //            UpdateTextwordCache(this);
        //        }
        //        return textwordCache;
        //    }
        //}
        public ILookup<short, word> GetWordsCache
        {
            get
            {

                ILookup<short, word> cache = dcd.words.ToLookup(a => a.id, p => p);
                //
                return cache;
            }
        }
        public sealed class VerseHistory
        {
            public int textid { get; set; }
            public DateTime ArchiveDate { get; set; }

        }
        public sealed class TextHistory
        {
            public TextHistory()
            {
                this.t = new Text();
            }
            public DateTime ArchiveDate { get; set; }
            public string Content { get; set; }
            internal Text t;
        }

        private static readonly Func<Entities, int, IEnumerable<IGrouping<DateTime, textwordsHistory>>>
            VerseHistoryByArchive = CompiledQuery.Compile((Entities db, int pTextId) =>
                db.textwordsHistories.
                Where(p => p.textid == pTextId).
                OrderBy(i => i.id).
                GroupBy(g => g.ArchiveDate).
                AsEnumerable());
        public IEnumerable<int> getTextIdsByBookIdsAndAlineas(IEnumerable<int> BookEditions, IEnumerable<int> gotos, out DateTime maxTs, out string eTag)
        {
            var bookeds = dcd.bookeditions.Where(b => BookEditions.Contains(b.bookEditionid)).ToList();
            var calcMaxTs = dcd.Texts.Where(i => gotos.Contains(i.BookChapterAlineaid) && bookeds.Contains(i.bookedition)).
                Select(t => new temp() { textid = t.textid, timestamp = t.timestamp }).ToList();
            eTag = tsFromTempRange(calcMaxTs, out maxTs);
            return calcMaxTs.Select(t => t.textid).AsEnumerable();
        }
        //public IEnumerable<tDifference> getDifference
        //{
        //    get
        //    {
        //        return dcd.tDifferences.AsEnumerable();
        //    }
        //}
        private static readonly Func<Entities, int, DateTime>
           CompareTimeStampCache = CompiledQuery.Compile((Entities db, int pTextId) =>
               db.Texts.SingleOrDefault(p => p.textid == pTextId).timestamp);

        public int CompareTimeStamp(int pTextId, DateTime pWithThisTimeStamp)
        {
            DateTime tsHere = CompareTimeStampCache(dcd, pTextId);

            return tsHere.CompareTo(pWithThisTimeStamp);
        }
        private static readonly Func<Entities, string, PublicationCode>
           PublicationCache = CompiledQuery.Compile((Entities db, string pPublisherCode) =>
               db.PublicationCodes.SingleOrDefault(p => p.publicationCode == pPublisherCode));
        public PublicationCode getPublicationByPublisherCode(string pPublisherCode)
        {
            return PublicationCache(dcd, pPublisherCode);
        }

        private static readonly Func<Entities, int, bookedition>
            BookEditionCache = CompiledQuery.Compile((Entities db, int pBookEditionId) =>
                db.bookeditions.SingleOrDefault(p => p.bookEditionid == pBookEditionId));

        public bookedition getBookEdition(int pBookEditionId)
        {
            return BookEditionCache(dcd, pBookEditionId);

        }
        //public int getBookIdFromBookEdition(int bookeditionid)
        //{
        //    var query = dcd.bookeditions.Where(p => p.bookEditionid == bookeditionid)
        //                    .Select(a => a.bookid).SingleOrDefault();

        //    return query == 0? 1 : query;
        //}
        private static readonly System.Func<Entities, int, int, int>
            BookEditionIdFromBook = CompiledQuery.Compile((Entities db, int bookId, int languageId) =>
                db.bookeditions.
                        Where(b => b.langid == languageId && b.bookid == bookId).
                        Select(b => b.bookEditionid).Single());


        public int getBookEditionIdFromBook(int bookId, int languageId)
        {
            return BookEditionIdFromBook(dcd, bookId, languageId);
        }
        public void BeginTransaction()
        {
            dcd.Transaction = dcd.Connection.BeginTransaction();
        }
        public void RollBack()
        {
            dcd.Transaction.Rollback();
        }
        public void Commit()
        {
            dcd.Transaction.Commit();
        }

        private static readonly Func<Entities, int, IQueryable<int>>
            ChaptersBookMenu = CompiledQuery.Compile((Entities db, int pBookEditionId) =>
                    db.bookchapters.Where(bookch => bookch.bookid == BookEditionIdToBookId(db, pBookEditionId)).
                                                        Select(ch => ch.chapter));
        public IEnumerable<int> GetChaptersBookMenu(int bookeditionId)
        {
            return ChaptersBookMenu(dcd, bookeditionId).AsEnumerable();
        }

        private static readonly Func<Entities, int, IEnumerable<BookChapter>>
            ChaptersByBookId = CompiledQuery.Compile((Entities db, int bookId) =>
                                    db.bookchapters.Where(p => p.bookid == bookId).
                                    Select(bc => new BookChapter
                                    {
                                        chapter = bc.chapter,
                                        BookChaptersJoinedCollection = new Collection<BookChaptersJoined>(bc.bookchapteralineas.
                                Select(a => new BookChaptersJoined
                                {
                                    alineaId = a.Alineaid,
                                    bookchapterAlineaId = a.bookchapteralineaid
                                }).ToArray())
                                    }));

        /// <summary>
        /// usefull for searching
        /// </summary>
        /// <param name="bookId"></param>
        /// <returns></returns>
        public IEnumerable<BookChapter> getChaptersByBookId(int bookId)
        {
            return ChaptersByBookId(dcd, bookId).AsEnumerable();
        }
        sealed class temp
        {
            public int textid;
            public DateTime timestamp;
        }
        /// <summary>
        /// same as Text minus database attributes
        /// plus problems with anonymous compiled query
        /// </summary>
        sealed class TextPseudo
        {

            internal int textid;
            internal int BookChapterAlineaid;
            internal int Alineaid;
            internal int bookeditionid;
            //internal string Content;
            //internal string Remarks;
            internal DateTime timestamp;
            //internal string Header;
            internal IEnumerable<textword> textwords;
            internal List<word> synonym;
            internal short langid;

        }
        private static readonly Func<Entities, int, int, IQueryable<temp>>
            getVerseStamp = CompiledQuery.Compile((Entities dcd, int pBookEditionId, int pVerse) =>

                dcd.Texts.Where(t => t.bookeditionid == pBookEditionId
                && t.BookChapterAlineaid == pVerse && t.bookchapteralinea.bookchapter.bookid == BookEditionIdToBookId(dcd, pBookEditionId)
                ).Select(r => new temp() { textid = r.textid, timestamp = r.timestamp }
                          ));

        private static readonly Func<Entities, int, int, IQueryable<temp>>
            getChapterStamp = CompiledQuery.Compile((Entities dcd, int pBookEditionId, int pChapter) =>

                dcd.Texts.Where(t => t.bookeditionid == pBookEditionId
                && t.bookchapteralinea.bookchapter.chapter == pChapter &&
                t.bookchapteralinea.bookchapter.bookid == BookEditionIdToBookId(dcd, pBookEditionId)
                                    )
                     .Select(r => new temp() { textid = r.textid, timestamp = r.timestamp }
                          ));

        private static readonly Func<Entities, DateTime, IQueryable<temp>>
            getIdTimeStampFromDate = CompiledQuery.Compile((Entities dcd, DateTime FromWhen) =>
                dcd.Texts.Where(t => t.timestamp >= FromWhen).
                    Select(r => new temp() { textid = r.textid, timestamp = r.timestamp }
                        ));

        private static string tsFromTempRange(IEnumerable<temp> calcFromRange, out DateTime maxTs)
        {
            maxTs = DateTime.MinValue;
            if (calcFromRange.Any())
            {

                maxTs = calcFromRange.Max(ts => ts.timestamp);
                //using a byte array and BitConverter.GetBytes(..) is and writing an int32 and a int64 1000000 times, takes 156/172 ms.
                // using the memorytstream took 56/62 ms.
                //specifying or precalculating the memory size, did not make any difference!
                // calculate exact length No records * (size(int32) + sizeof(int64))
                using (var mem = new MemoryStream(calcFromRange.Count() * (sizeof(int) + sizeof(long))))
                {
                    using (var memStream = new BinaryWriter(mem))
                    {
                        foreach (var b in calcFromRange)
                        {
                            memStream.Write(b.textid);
                            memStream.Write(b.timestamp.ToBinary());
                        }

                        var MD5Enc = MD5CryptoServiceProvider.Create();
                        mem.Position = 0;
                        return BitConverter.ToString(MD5Enc.ComputeHash(mem)).Replace("-", "");
                    }
                }
            }
            else
            {
                maxTs = DateTime.Today;
                return string.Empty;

            }
        }

        /// <summary>
        /// Returns the Max of timestamp for a range of verses within a chapter of a book
        /// this must be used to make search engines happy and minimize our network traffic 
        /// (304 HTTP status instead of full page, is hardly bytes)
        /// calculates a unique hash code using the primary key of Text (id) and the lastmodified field.
        /// </summary>
        /// <param name="bookEditionId"></param>
        /// <param name="chapter"></param>
        /// <returns>The last modification timestamp</returns>
        /// <param name="pBookEditionId"></param>
        /// <param name="pChapter"></param>
        /// <param name="maxTs">Output maximum timestamp</param>
        /// <returns></returns>
        public string getVersesByBookEditionEtag(IEnumerable<int> pBookEditionId, int pChapter, int GoToVerse, out DateTime maxTs)
        {
            maxTs = DateTime.MinValue;
            string returnValue = maxTs.ToBinary().ToString();
            var calcFromRange = new List<temp>();
            foreach (int i in pBookEditionId)
            {
                calcFromRange.AddRange(GoToVerse == 0 ?
                    getChapterStamp.Invoke(dcd, i, pChapter).ToList() :
                    getVerseStamp.Invoke(dcd, i, GoToVerse).ToList());


            }
            string max = tsFromTempRange(calcFromRange, out DateTime t);
            if (t > maxTs)
            {
                maxTs = t;
                returnValue = max;
            }
            return returnValue;

        }

        public string bookTitle(int pTextId)
        {
            Text t = GetVerseById(pTextId);
            var chpt = t.bookchapteralinea.bookchapter;
            int chapter = chpt.chapter;
            var title = t.bookedition.title;
            return string.Format("{0} {1}:{2}", title, chapter, t.Alineaid);

        }

        public string getVersesByBookEditionMaxTs(int pBookEditionId, out DateTime maxTs)
        {
            var calcFromRange = dcd.Texts.Where(t => t.bookeditionid == pBookEditionId).
                            Select(r => new temp()
                            { textid = r.textid, timestamp = r.timestamp }).ToList();

            return tsFromTempRange(calcFromRange, out maxTs);
        }

        //TODO: int GoTo should be int[] GoTos to support a range of verses 
        private static readonly Func<Entities, int, int, int, IQueryable<TextPseudo>>
            VersesBookEditionByGotos = CompiledQuery.Compile((Entities dcd, int bookEditionId, int GoTo, int bookId) =>
                    dcd.Texts.Where(t => t.bookeditionid == bookEditionId
                            && t.BookChapterAlineaid == GoTo
                                    && t.bookchapteralinea.bookchapter.bookid == bookId
                            ).
                            OrderBy(o => o.textid).
                            Select(x => new TextPseudo
                            {
                                textid = x.textid,
                                Alineaid = x.Alineaid,
                                timestamp = x.timestamp,
                                bookeditionid = x.bookeditionid,
                                BookChapterAlineaid = x.BookChapterAlineaid,
                                textwords = x.textwords,
                            }));
        private static readonly Func<Entities, int, int, int, IQueryable<TextPseudo>>
            VersesBookEdition = CompiledQuery.Compile((Entities dcd, int bookEditionId, int chapter, int bookId) =>
                    dcd.Texts.Where(t => t.bookeditionid == bookEditionId
                            && t.bookchapteralinea.bookchapter.chapter == chapter
                                    && t.bookchapteralinea.bookchapter.bookid == bookId
                            ).
                            OrderBy(o => o.textid).
                            Select(x => new TextPseudo
                            {
                                textid = x.textid,
                                Alineaid = x.Alineaid,
                                timestamp = x.timestamp,
                                bookeditionid = x.bookeditionid,
                                BookChapterAlineaid = x.BookChapterAlineaid,
                                textwords = x.textwords,
                                // we keep it empty
                                langid = x.bookedition.langid
                            }));
        /// <summary>
        /// retrieves the bookid from a bookeditionid
        /// </summary>
        private static readonly Func<Entities, int, int>
            BookEditionIdToBookId = CompiledQuery.Compile((Entities db, int pBookEditionId) =>
                db.bookeditions.First(be => be.bookEditionid == pBookEditionId).bookid);

        /// <summary>
        /// retrieves the bookid from a bookeditionid
        /// </summary>
        private static readonly Func<Entities, string, int>
            BookPubToBookId = CompiledQuery.Compile((Entities db, string pub) =>
                db.bookeditions.First(be => be.publishercode == pub).bookid);
        /// <summary>
        /// uses textwords and word to and composes the text in this way.
        /// </summary>
        /// <param name="bookEditionId"></param>
        /// <param name="chapter"></param>
        /// <returns></returns>
        public IEnumerable<AlineaText> getVersesByBookEditionDc(IEnumerable<int> bookEditionId, int chapter, IEnumerable<int> GoTos)
        {
            var retVal = new List<Text>();
            if (wordsCache == null)
            {
                UpdateWordsCache(this);
            }
            //TODO 
            foreach (int i in bookEditionId)
            {
                int bookId = BookEditionIdToBookId(dcd, i);
                var preChoice = !GoTos.Any() ? VersesBookEdition(dcd, i, chapter, bookId) :
                    VersesBookEditionByGotos(dcd, i, GoTos.First(), bookId);

                foreach (var tbd in preChoice)
                {
                    Text t = new Text()
                    {
                        textid = tbd.textid,
                        BookChapterAlineaid = tbd.BookChapterAlineaid,
                        Alineaid = tbd.Alineaid,
                        timestamp = tbd.timestamp,
                        bookeditionid = tbd.bookeditionid,
                        langid = tbd.langid
                    };
                    retVal.Add(t);
                    Decompress(t, tbd.textwords);
                }
            }

            foreach (var alTexts in retVal.GroupBy(a => string.Format("{0:D9}{1:D3}", a.BookChapterAlineaid, a.Alineaid)))
            {
                string t = alTexts.Key;
                yield return new AlineaText()
                {
                    BookChapterAlineaId = int.Parse(t.Substring(0, 9)),
                    Alineaid = int.Parse(t.Substring(9, 3)),
                    Texts = alTexts.ToList()
                };
            }

        }
        /// <summary>
        /// reads a whole book using decompression
        /// </summary>
        /// <param name="bookEditionId"></param>
        /// <returns></returns>
        public IEnumerable<AlineaText> getVersesByBookEditionDc(IEnumerable<int> bookEditionId)
        {
            var retVal = new List<Text>();
            if (wordsCache == null) 
            {
                UpdateWordsCache(this);
            }
            foreach (int i in bookEditionId)
            {

                foreach (var tbd in dcd.Texts.Where(t => t.bookeditionid == i && t.Alineaid > 0).
                            OrderBy(o => o.textid).
                            Select(z => new TextPseudo
                            {
                                textid = z.textid,
                                Alineaid = z.Alineaid,
                                timestamp = z.timestamp,
                                bookeditionid = z.bookeditionid,
                                BookChapterAlineaid = z.BookChapterAlineaid,
                                textwords = z.textwords
                            }).AsEnumerable())
                {
                    var t = new Text()
                    {
                        textid = tbd.textid,
                        BookChapterAlineaid = tbd.BookChapterAlineaid,
                        Alineaid = tbd.Alineaid,
                        timestamp = tbd.timestamp,
                        bookeditionid = tbd.bookeditionid
                    };
                    retVal.Add(t);
                    Decompress(t, tbd.textwords);
                }
            }
          
            var restVal = retVal.GroupBy(a => string.Format("{0:D9}{1:D3}", a.BookChapterAlineaid, a.Alineaid)).
                Select(alTexts => 
                        new AlineaText()
                        {
                            BookChapterAlineaId = int.Parse(alTexts.Key.Substring(0, 9)),
                            Alineaid = int.Parse(alTexts.Key.Substring(9, 3)),
                            Texts = alTexts.ToList()
                        });
            return restVal;

        }
        /*
         * Retrieves a single verse
         * */
        private static readonly Func<Entities, int, TextPseudo>
            SingleVerse = CompiledQuery.Compile((Entities db, int pTextId) =>
                db.Texts.Where(p => p.textid == pTextId).
                Select(t => new TextPseudo()
                {
                    textid = t.textid,
                    Alineaid = t.Alineaid,
                    timestamp = t.timestamp,
                    BookChapterAlineaid = t.BookChapterAlineaid,
                    bookeditionid = t.bookeditionid,
                    textwords = t.textwords
                }).SingleOrDefault());


        public Text DecompressVerse(int pTextId)
        {
            if (wordsCache == null) 
            {
                UpdateWordsCache(this);
            }
            var query = SingleVerse(dcd, pTextId);
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
                bookeditionid = query.bookeditionid
            };
            Decompress(retVal, query.textwords);
            return retVal;
        }
        public IEnumerable<PublicationCode> PublicationCodes
        {
            get { return dcd.PublicationCodes.Where(pc => pc.searchable == true).AsEnumerable(); }
        }
        //public void CompressBook(int bookEditionId)
        //{
        //    if (dcd.Connection.State == System.Data.ConnectionState.Closed)
        //    {
        //        dcd.Connection.Open();
        //    }
        //    var verses = getVersesByBookEdition(bookEditionId);
        //    DbTransaction dbt = null;
        //    try
        //    {
        //        dbt = dcd.Connection.BeginTransaction();
        //        dcd.Transaction = dbt;

        //        foreach (Text t in verses)
        //        {
        //            CompressVerse(t);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        dbt.Rollback();
        //        throw ex;
        //    }

        //    dbt.Commit();
        //}
        /// <summary>
        /// returns a list of textids found
        /// </summary>
        /// <param name="words"></param>
        /// <param name="bookEditionId"></param>
        /// <param name="exact"></param>
        /// <returns></returns>
        //public List<int> FindRange(IList<word> words, IList<bookedition> bookEditionId, bool exact, int maxResults)
        //{
        //      /*
        //     * issues zoeken op woordreeks
        //     * negeren leestekens zoals komma's en punten
        //     * Zoeken in versen, of zoeken op woordreeksen die twee versen zou overlappen?
        //     *  indien exacte woordreeks
        //     *      zoek op eerste woord. pak het woord erna, negeer symbolen en HTML. Is het volgende woord gelijk aan het gezochte?
        //     *      ja? Dan door tot laatste woord. 
        //     *      nee? begin weer bij het eerste woord en pak de woordreeks 'met skip'.
        //     *      Eerste gevonden + laatste gevonden woord, vormt reeks.
        //     *  indien op 'of' reeks.
        //     *  indien op betekenis gerelateerd
        //     * */
        //    int startPos = 0;
        //    int findTokens = 5; //max 5 we are going to cache to minimize datbase trips
        //    int currentWord = 0;
        //    int curFindWord = 0;

        //    int findMaxCount = words.Count;
        //    List<textword> result = new List<textword>();
        //    //List<textword>[] foundList = new List<textword>[findMaxCount];
        //    var synonyms = dcd.Synonyms.Where(sm => words.Contains(sm.word1)).Select(sm2 => sm2.word).ToList().Concat(words).ToList();

        //    // algorithm;
        //    // eg We input Matthew 1:1-10;4:1-4
        //    //  facts There is one requested book, 6 numbers; two ranges (1-10, 1-4), two chapters
        //    if (exact)
        //    {

        //        //List<textword> query = new List<textword>();
        //            //List<int> bookEds = bookEditionId.Select(p => p.bookEditionid).ToList();
        //            //foreach (bookedition be in bookEditionId)
        //            //{
        //                //query.AddRange(dcd.textwords.Where(p => words.Contains(p.word) &&
        //                //    bookEditionId.Contains(p.Text.bookedition)));
        //            //}
        // //       var    query = dcd.textwords.Where(p => words.Contains(p.word) &&
        //   //                 bookEditionId.Contains(p.Text.bookedition)).GroupBy(g => g.textid).ToList();
        //        //var queryWordList = dcd.textwords.Select(w => w.word);
        //        //var query3 = dcd.Texts.Where(t => bookEditionId.Contains(t.bookedition)).
        //        //    Select(s => s.textwords).Where(tw => words.Contains(tw.First().word)).ToList();

        //        //var doit = GetTWCache
        //        //    .GroupBy(p => p.textid, p => p.wordid, (tid, wordsinverses) => new { textids = tid, ws = wordsinverses })
        //        //    .Where(p => p.ws.Intersect(words.Select(w => w.id)).Count() >= words.Count()).Select(v => v.textids);

        //        //.Where(z => z.bookeditionid == bookEditionId[0].bookEditionid)
        //        var doitx = dcd.Texts.
        //            Where(filtPub => bookEditionId.Contains(filtPub.bookedition))
        //            .SelectMany(tw => tw.textwords).Select(p => new { p.textid, p.wordid }).                    
        //            GroupBy(p => p.textid, p => p.wordid, (tid, words2) => new { textids = tid, ws = words2 }).
        //            Where( p => p.ws.Distinct().Where(w => synonyms.Select(x => x.id).Contains(w)).Count() >= words.Count);

        //        return doitx.Select(s => s.textids).Take(maxResults).ToList();
        //        //var query2 = dcd.textwords.GroupBy(g => g.textid).Where(p => words.Contains(p.Key) &&
        //          //          bookEditionId.Contains(p.Text.bookedition)).GroupBy(g => g.textid).ToList();

        //        //foundList[currentWord] = query;
        //        //todo: this could also find results on wordranges like matthew 28:20 and marc 1:1 

        //        //int idx = 0, previdx = -1;
        //        //do
        //        //{
        //        //    foreach (word wrd in words)
        //        //    {

        //        //        int distanceRequired = 1; // exact search
        //        //        int currentResultLen = result.Count;
        //        //        idx = query.FindIndex(previdx + 1, p => p.wordid == wrd.id);
        //        //        if (idx > previdx + distanceRequired) // 
        //        //        {
        //        //            //we'll remove the missed range
        //        //            result.RemoveRange(previdx, result.Count - previdx);
        //        //            break;
        //        //        }
        //        //        else if (idx >= 0)
        //        //        {
        //        //            result.Add(query[idx]);
        //        //            previdx = idx;
        //        //        }
        //        //        // -1 exit
        //        //        else
        //        //        {                         
        //        //            break;

        //        //        }
        //        //    }
        //        //}
        //        //while (idx >= 0);
        //    }
        //    else
        //    {
        //    }

        //    return null;
        //}
        ///// <summary>
        /// compresses a boolean search term
        /// </summary>
        /// <param name="term"></param>
        /// <param name="exact">if true, will match exact sentence</param>
        /// <returns>a list of found words which also include (of course) a textid</returns>
        public IEnumerable<word> CompressTerm(string term)
        {
            int startAt = 0;
            int foundSplits = 0;

            // take the content text first than the footnote            
            if (!string.IsNullOrEmpty(term))
            {
                int lineLen2 = term.Length;
                string theWord = null;
                for (;;)
                {
                    foundSplits = term.IndexOfAny(splitters, startAt);
                    if (foundSplits < 0 && lineLen2 > 0)
                    {
                        theWord = term.Substring(startAt);
                        if (theWord.Length > 0)
                        {
                            var foundWord = FindWord(theWord);
                            if (foundWord != null)
                            {
                                yield return foundWord;
                            }
                        }
                        //exit the for loop
                        break;
                    }

                    int wordLen = foundSplits - startAt;
                    theWord = term.Substring(startAt, wordLen);
                    char foundChar = term[foundSplits];

                    if (wordLen > 0)
                    {
                        char ch = term[startAt + wordLen];
                        //compressing space, comma and dot delivers a compressionrate of +/- 20%                       
                        bool addComma = ch == ',';
                        bool addDot = ch == '.';
                        bool addSpace = ch == ' ';
                        // is there any space afther the dot or the comma?
                        if (!addSpace && (ch == ',' || ch == '.'))
                        {
                            addSpace = startAt + wordLen + 1 < lineLen2 ? term[startAt + wordLen + 1] == ' ' : false;
                        }
                        var foundWord = FindWord(theWord);
                        if (foundWord != null)
                        {
                            yield return foundWord;
                        }
                    }
                    startAt = foundSplits + 1;
                }

            }
        }

        /*
         * situations that we want to catch and deal with specially
         * 1) Hyphens - (search feature: this happens when we want a text-range like Matthew 1:2-10)
         * 2) Colons : (search feature: this happens when we want a chapter)
         * 3) Parenthesis ()  (render: some words or sentences are surrounded by ( or ))
         * 4) brackets [] (render: some words or sentences are surrounded by [ or ] )
         * 5) Comma's ,  (render: western languages just have comma's)
         * 6) Dots . (render: sentences end with a dot)
         * 7) Double Quotes " (render: Western Languages enquote a sentence when some one speaks)
         * special occasions:
         * 1) Comma + space , (render: I said, how do you do?)
         * 2) dot + space . (render: It's ok. Next?)
         * 3) parenthesis + dot + space(render: That's what they said (or thought). He anwsered)
         * 4) Parenthesis dot + space
         * 
         * We solve this like this:
         * ) and ] have the most left rendering preference
         * . and , next
         * then space
         * */
        /// <summary>
        /// Compresses one single verse
        /// </summary>
        /// <param name="t"></param>
        /// <param name="pNewContent">the text</param>
        /// <param name="pNewRemark">appends a footer below the text</param>
        /// <param name="pHeaderText">inserts a header above the text</param>
        /// <returns>true if new words were added (so refresh wordcache)</returns>
        public bool CompressVerse(Text t,
                string pNewContent,
                string pNewRemark,
                string pHeaderText)
        {
            //string[] verseWords = content.Split(new[] { ' ' }, StringSplitOptions.None);
            //this.dcd.Transaction = dcd.Connection.BeginTransaction();
            bool didAdd = false;
            if (t == null) return didAdd;
            int textid = t.textid;
            dcd.SubmitChanges();
            //copy history
            DateTime now = DateTime.Now;
            dcd.textwordsHistories.InsertAllOnSubmit(t.textwords.OrderBy(i => i.id).Select(tw =>
                    new textwordsHistory()
                    {
                        wordid = tw.wordid,
                        textid = tw.textid,
                        AddComma = tw.AddComma,
                        AddDot = tw.AddDot,
                        AddSpace = tw.AddSpace,
                        ArchiveDate = now,
                        IsAllCaps = tw.IsAllCaps,
                        IsCapitalized = tw.IsCapitalized,
                        IsFootNote = tw.IsFootNote,
                        IsHeader = tw.IsHeader,
                        AddColon = tw.AddColon,
                        AddHyphenMin = tw.AddHyphenMin,
                        Semicolon = tw.Semicolon,
                        LBracket = tw.LBracket,
                        PreSpace = tw.PreSpace,
                        RBracket = tw.RBracket,
                        RParentThesis = tw.RParentThesis,
                        LParentThesis = tw.LParentThesis,
                        LDQuote = tw.LDQuote,
                        RDQuote = tw.RDQuote,
                        RSQuote = tw.RSQuote,
                        LSQuote = tw.LSQuote,
                        AddSlash = tw.AddSlash,
                        AddGT = tw.AddGT,
                        AddLT = tw.AddLT,
                        AddBang = tw.AddBang,
                        AddSlashAfter = tw.AddSlashAfter,
                        QMark = tw.QMark,
                        AddEqual = tw.AddEqual

                    })
                   );

            // this includes footer, header and body
            dcd.textwords.DeleteAllOnSubmit(t.textwords);
            dcd.SubmitChanges();

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
                for (;;)
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
                        char ch = foundChar; //[startAt + wordLen];
                        //var checkForSpecialWords = dcd.TranslateToWords.FirstOrDefault(special => special.word == theWord);
                        //if (checkForSpecialWords != null)
                        //{
                        //    // this instance must be eaten, not translated.
                        //    if (checkForSpecialWords.wordid == null)
                        //    {
                        //        if (checkForSpecialWords.specialCharacters != null)
                        //        {
                        //            while (checkForSpecialWords.specialCharacters.IndexOf(ch) >= 0)
                        //            {
                        //                ch = content[++foundSplits];
                        //            }
                        //        }
                        //        //startAt = foundSplits;
                        //        theWord = "";
                        //        goto q;
                        //    }
                        //    //TODO: fix langid

                        //    theWord = dcd.words.SingleOrDefault(a => a.id == checkForSpecialWords.wordid && a.LangId == 19 && a.IsNumber == false).word1;
                        //    if (checkForSpecialWords.CapitalizeTargetWord)
                        //    {
                        //        theWord = char.ToUpper(theWord[0]) + theWord.Substring(1);
                        //    }
                        //    if (checkForSpecialWords.specialCharacters != null)
                        //    {
                        //        while (checkForSpecialWords.specialCharacters.IndexOf(ch) >= 0)
                        //        {
                        //            ch = content[++foundSplits];
                        //        }
                        //    }
                        //}
                        q:
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
                        if (CompressRange(t, theWord, addSpace, addDot, addComma, x == 1, x == 2,
                            addColon, addSemiColon, addHyphen, addLBracket, addRBracket, addRParenthesis,
                            addLParenthesis, addLSQuote, addRSQuote, addLDQuote, addRDQuote, addLT, addGT, addSlash, addBang,
                            preSpace, addQMark, addSlashAfter, addEqual, addAmp))
                        {
                            didAdd = true;

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

                        if (foundChar > '\0' && CompressRange(t, new string(foundChar, 1), addSpace, addDot, addComma, x == 1, x == 2,
                            false, false,
                            false, false, false, false, false, false, addRSQuote, false, addRDQuote, false, false, false, false, false, false, false, false, false))
                        {
                            didAdd = true;

                        }
                        addRDQuote = addRSQuote = false;
                        foundSplits++;
                    }
                    startAt = foundSplits;
                }
            }
            dcd.SubmitChanges();
            //this.dcd.Transaction.Commit();
            return didAdd;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="textid"></param>
        /// <param name="w">The single word to be converted to a number</param>
        private word FindWord(string w)
        {
            IsCapitalized(w, out bool capitalized, out bool allCaps);
            if (capitalized || allCaps)
            {
                w = w.ToLower();
            }
            // numbers can become huge, and thus, waste space!
            bool isNumber = int.TryParse(w, out int number);
            var foundWord = isNumber ? dcd.words.SingleOrDefault(a => a.number == number) : dcd.words.SingleOrDefault(a => a.word1 == w);
            return foundWord;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="textid"></param>
        /// <param name="w">The single word to be converted to a number</param>
        private bool CompressRange(Text t, string w,
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
            bool pAddAmp
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
            int? foundid = null;
            short? langid = 19;
            dcd.FindWord(new Binary(Encoding.Unicode.GetBytes(w)), number, ref foundid, langid);

            word foundWord = null;
            if (foundid != null)
            {
                foundWord = dcd.words.Single(a => a.id == foundid.Value);
            }

            if (foundWord == null)
            {
                foundWord = new word()
                {
                    IsNumber = isNumber,
                    //TODO: make language 
                    LangId = 19
                };
                if (isNumber)
                {
                    foundWord.number = number;
                }
                else
                {
                    foundWord.word1 = w;
                }
                dcd.words.InsertOnSubmit(foundWord);
                dcd.SubmitChanges();
                didAdd = true;
            }
            var wordid = foundWord.id;
            var tw = new textword()
            {
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
            t.textwords.Add(tw);
            return didAdd;
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
       
        public static void Decompress(Text t, IEnumerable<textword> tws) 
        {

            var sb = new StringBuilder(512);
            // loop from 0 to 2 for respective text, footer, header decompression
            for (int cx = 2; cx >= 0; cx--)
            {
                sb.Length = 0;
                IEnumerable<textword> texts = null;
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
                    var listOfSynonyms = synonymCache[tw.wordid];
                    //create a string formed like: "word: syn1, [syn2], ...
                    if (listOfSynonyms != null)
                    {
                        t.Synonyms = wordsCache[tw.wordid].First().word1 + ':';
                        t.Synonyms += string.Join(",", wordsCache[tw.wordid].Select(w => w.word1));
                    }
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
                            sb.Append(wrd.word1.ToUpper());
                            // sb.Append(UnPackTags(wrd.tWordTags));
                        }
                        else if (tw.IsCapitalized)
                        {
                            string w = wrd.word1;
                            sb.Append(char.ToUpper(w[0]));
                            sb.Append(w, 1, w.Length - 1);
                            //sb.Append(UnPackTags(wrd.tWordTags));
                        }
                        else
                        {
                            sb.Append(wrd.word1);
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
        public IEnumerable<Text> getVersesByBookEdition(IEnumerable<int> bookEditionId, int chapter)
        {


            return bookEditionId.SelectMany(i => dcd.Texts.Where(t => t.bookeditionid == i
                     && t.bookchapteralinea.bookchapter.chapter == chapter &&
                         t.bookchapteralinea.bookchapter.bookid == BookEditionIdToBookId(dcd, i)
                             // ADDING  the bookid in the where clause, optimizes by 1000%
                             ).
                     OrderBy(o => o.textid)).ToArray();


        }

        public IEnumerable<Text> getVersesByBookEdition(int bookEditionId)
        {
            var query = dcd.Texts.Where(t => t.bookeditionid == bookEditionId).
                  OrderBy(o => o.textid);
            return query.AsEnumerable();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pTextId"></param>
        /// <param name="langId">assumes 19 at the moment</param>
        /// <returns></returns>
        public string GetVerseToolTip(int pTextId, int langId)
        {
            var find = dcd.Texts.SingleOrDefault(p => p.textid == pTextId);
            if (find != null)
            {
                return string.Format("{0} {1}:{2}", find.bookedition.title, find.bookchapteralinea.bookchapter.chapter, find.Alineaid);

            }
            return string.Empty;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pTextId"></param>
        /// <param name="langId">assumes 19 at the moment</param>
        /// <returns></returns>
        public VerseInfo GetVerseInfo(int pTextId, int langId)
        {
            var find = dcd.Texts.SingleOrDefault(p => p.textid == pTextId);
            if (find != null)
            {
                return new VerseInfo() { Book = find.bookedition.title, Chapter = find.bookchapteralinea.bookchapter.chapter, Verse = find.Alineaid, TextId = pTextId, BookEnglish = find.bookedition.EnglishTitle };
                //string.Format("{0} {1}:{2}", find.bookedition.title, find.bookchapteralinea.bookchapter.chapter, find.Alineaid);

            }
            return null;
        }
        public string GetBookEnglishTitle(int bookeditionid)
        {
            var find = dcd.bookeditions.SingleOrDefault(p => p.bookEditionid == bookeditionid);
            return find != null ? find.EnglishTitle : string.Empty;
        }
        internal Text GetVerseById(int pTextId)
        {
            return dcd.Texts.SingleOrDefault(p => p.textid == pTextId);

        }

        internal void SubmitChanges()
        {
            dcd.SubmitChanges();
        }

        /// <summary>
        /// returns the list containing all titles eg. for a book index
        /// </summary>
        /// <param name="pBookEditionId"></param>
        /// <returns></returns>
        public IEnumerable<bookedition> BookEditionsByPublishCode(int pBookEditionId)
        {
            return dcd.bookeditions.OrderBy(o => o.bookOrder).
                Where(w => w.bookEditionid == pBookEditionId).AsEnumerable();

        }
        /// <summary>
        /// retrieves the next book, eg. after matthew comes mark
        /// </summary>
        /// <param name="pPublisherCode"></param>
        /// <param name="pLanguage"></param>
        /// <returns></returns>
        //public bookedition GetNextBookEdition(bookedition currentBookEdition)
        //{
        //    var retVal = dcd.bookeditions.Where(p => p.publishercode == currentBookEdition.publishercode
        //            && p.active && p.bookOrder > currentBookEdition.bookOrder)
        //            .OrderBy(o => o.bookOrder).FirstOrDefault();
        //    return retVal;
        //}

        /// <summary>
        ///// retrieves the start book, eg. after matthew comes mark
        ///// </summary>
        ///// <param name="pPublisherCode"></param>
        ///// <param name="pLanguage"></param>
        ///// <returns></returns>
        //public bookedition GetStartBookEdition(bookedition currentBookEdition)
        //{
        //    var retVal = dcd.bookeditions.Where(p => p.publishercode == currentBookEdition.publishercode
        //            && p.active)
        //            .OrderBy(o => o.bookOrder).FirstOrDefault();
        //    return retVal;
        //}
        /// <summary>
        /// retrieves the previous book, eg. after matthew comes mark
        /// </summary>
        /// <param name="pPublisherCode"></param>
        /// <param name="pLanguage"></param>
        /// <returns></returns>
        //public bookedition GetPreviousBookEdition(bookedition currentBookEdition)
        //{

        //    var retVal = dcd.bookeditions.Where(p => p.publishercode == currentBookEdition.publishercode
        //            && p.active && p.bookOrder < currentBookEdition.bookOrder)
        //            .OrderByDescending(o => o.bookOrder).FirstOrDefault();
        //    return retVal;
        //}
        /// <summary>
        /// returns a list of bookedition ordered by bookOrder
        /// </summary>
        /// <param name="pPublisherCode"></param>
        /// <param name="pLanguage"></param>
        /// <returns></returns>
        public IEnumerable<bookedition> BookEditionsByPublishCode(string pPublisherCode)
        {
            var retVal = dcd.bookeditions.Where(p => p.publishercode == pPublisherCode
                    && p.active).OrderBy(o => o.bookOrder);
            return retVal.AsEnumerable();
        }

        public IEnumerable<bookedition> BookEditionsByIds(IEnumerable<int> ids)
        {
            var retVal = dcd.bookeditions.Where(w => ids.Contains(w.bookEditionid));
            return retVal.AsEnumerable();
        }
    }
}