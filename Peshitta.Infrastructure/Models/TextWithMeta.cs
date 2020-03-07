using System;
using System.Collections.Generic;

namespace Peshitta.Infrastructure.Models
{
    public class TextMeta
    {

        public int textid { get; set; }
        public int bca { get; set; }
        public int aid { get; set; }
        public int beid { get; set; }
        public DateTimeOffset ts { get; set; }
        public int ch { get; set; }
    }

    public class BookEditionMeta
    {
        public int beid { get; set; }
        public int bookid { get; set; }
        public  string pc { get; set; }
        public  int langid { get; set; }
        public  string title { get; set; }
        public  string entitle { get; set; }
        public  string descr { get; set; }
        public  int bo { get; set; }
        public  string abr { get; set; }
    }
    public class TextWithMeta
    {
        public IEnumerable<TextMeta> Text { get; set; }
        public IEnumerable<BookEditionMeta> BookEditions { get; set; }
    }
}