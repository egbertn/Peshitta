using Newtonsoft.Json;
using System;

namespace Peshitta.Data.Models
{
    public class TextWordsHistory : TextWords
    {
        public DateTime ArchiveDate { get; set; }
    }
    
    public class TextWords
    {

        public override bool Equals(object obj)
        {           
            if (obj is TextWords tw)
            {
                return tw.id == this.id;
            }
            return false;
        }
        public override int GetHashCode()
        {
            return id;
        }
        public int id { get; set; }
        public int textid { get; set; }
        public int wordid { get; set; }
     

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsCapitalized { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? AddSpace { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsAllCaps { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsFootNote { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? AddDot { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? AddComma { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsHeader { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? LParentThesis { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? RParentThesis { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? LBracket { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? RBracket { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? LAngle { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? Rangle { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? AddColon { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? AddHyphenMin { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? RDQuote { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? LDQuote { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? RSQuote { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? LSQuote { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? AddLT { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? AddGT { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? AddSlash { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? AddBang { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? QMark { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? AddSlashAfter { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? AddEqual { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? AddAmp { get; set; }
    }
}
