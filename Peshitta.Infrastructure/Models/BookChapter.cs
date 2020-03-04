using Newtonsoft.Json;
using System;
namespace Peshitta.Infrastructure.Models
{
    public class BookChapter
    {
        public override bool Equals(object obj)
        {
           if (obj is BookChapter bc)
            {
                return bc.bookchapterid == this.bookchapterid;
            }
            return false;
        }
        public override int GetHashCode()
        {
            return bookchapterid;
        }
        public int bookchapterid { get; set; }
        public int bookid { get; set; }
        public int chapter { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string comments { get; set; }

        public DateTime TimeStamp { get; set; }
    }
}
