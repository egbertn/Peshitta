using Newtonsoft.Json;
using System;
namespace Peshitta.Infrastructure.Models
{
   
    public class BookChapterAlinea
    {
        public override int GetHashCode()
        {
            unchecked
            {
                var p = ((ulong)BookchapterAlineaId) << 32 | (ulong)AlineaId;
                return p.GetHashCode();
            }
        }

        public override bool Equals(object obj)
        {         
           if (obj is BookChapterAlinea bca)
            {
                return bca.BookchapterAlineaId == BookchapterAlineaId && bca.AlineaId == AlineaId;
            }
           else if (obj is BookChapter bc)
            {
                return bc.bookchapterid == this.BookchapterId;
            }
            return false;
        }
        public int BookchapterAlineaId { get; set; }
        public int BookchapterId { get; set; }
        public int AlineaId { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Comments { get; set; }
        public DateTime TimeStamp { get; set; }
    }
}
