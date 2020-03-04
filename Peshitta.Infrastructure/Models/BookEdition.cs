using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Peshitta.Infrastructure.Models
{
    public class BookEdition
    {
        public override bool Equals(object obj)
        {
            if (obj is BookEdition be)
            {
                return be.bookEditionid == this.bookEditionid;
            }
            return false;
        }
        public override int GetHashCode()
        {
            return bookEditionid;
        }
        public int bookEditionid { get; set; }
        public int bookid { get; set; }
        public string publishercode { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? year { get; set; }
        public string isbn { get; set; }
        public short langid { get; set; }
        public string Copyright { get; set; }
        public string title { get; set; }
        public string EnglishTitle { get; set; }
        public string Author { get; set; }
        public IEnumerable<string> keywords { get; set; }
        public string description { get; set; }
        public string robots { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? PressDate { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public float? Version { get; set; }
        public string subject { get; set; }
        public string nativeAbbreviation { get; set; }
        public int bookOrder { get; set; }
        public bool active { get; set; }
    }
}
