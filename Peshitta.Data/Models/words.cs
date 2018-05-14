using Newtonsoft.Json;
using Peshitta.Data.Attributes;

namespace Peshitta.Data.Models
{
    [JsonObject]
    [Cache]
    public class words
    {
        public override bool Equals(object obj)
        {
            if (obj is words wrds)
            {
                return wrds.id == this.id;
            }
            return false;
        }
        public override int GetHashCode()
        {
            return id;
        }
        public int id { get; set; }
        public string word { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsSymbol { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public short LangId { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsNumber { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? number { get; set; }
    }
}
