using System;
using System.Collections.Generic;
using System.Linq;

namespace Peshitta.Data.Models
{
    /// <summary>
    /// contains a specific publication of a book, such as translation
    /// </summary>
    public class Publication
    {
        /// <summary>
        /// publishercode and full book search
        /// </summary>
        public IEnumerable<string> FullTextSearch { get; set; }
        public ILookup<int, TextWords> TextWords { get; set; }
        public IReadOnlyDictionary<int, Text> Texts { get; set; }
        /// <summary>
        /// to enumerate history id's
        /// </summary>
        public ILookup<int, DateTime> HistoryDates { get; set; }
        public ILookup<HistoryKey, TextWordsHistory> TextWordsHistory { get; set; }

    }
    public class All
    {
        public All()
        {
            Pubs = new Dictionary<string, Publication>(2);
        }
        public IEnumerable<PublicationCode> PublicationCodes { get; set; }

        public IEnumerable<Language> Languages { get; set; }

        public IReadOnlyDictionary<int, Book> Books { get; set; }
        public IReadOnlyDictionary<int, BookChapter> BookChapters { get; set; }

        public IReadOnlyDictionary<BookChapterAlineaKey, BookChapterAlinea> BookChapterAlineas {get;set;}

        public IReadOnlyDictionary<int, BookEdition> BookEditions { get; set; }

       
      
        public IReadOnlyDictionary<int, words> Words { get; set; }
        /// <summary>
        /// contains the specific edition of the publication. Editable/read write
        /// </summary>
        public IDictionary<string, Publication> Pubs { get;  }
    }
}
