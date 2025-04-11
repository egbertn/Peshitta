using System;

namespace Peshitta.Infrastructure.Models
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


        public bool? IsCapitalized { get; set; }
        public bool? AddSpace { get; set; }
        public bool? IsAllCaps { get; set; }
        public bool? IsFootNote { get; set; }
        public bool? AddDot { get; set; }
        public bool? AddComma { get; set; }
        public bool? IsHeader { get; set; }
        public bool? LParentThesis { get; set; }
        public bool? RParentThesis { get; set; }
        public bool? LBracket { get; set; }
        public bool? RBracket { get; set; }
        public bool? LAngle { get; set; }
        public bool? Rangle { get; set; }
        public bool? AddColon { get; set; }
        public bool? AddHyphenMin { get; set; }
        public bool? RDQuote { get; set; }
        public bool? LDQuote { get; set; }
        public bool? RSQuote { get; set; }
        public bool? LSQuote { get; set; }
        public bool? AddLT { get; set; }
        public bool? AddGT { get; set; }
        public bool? AddSlash { get; set; }
        public bool? AddBang { get; set; }
        public bool? QMark { get; set; }
        public bool? AddSlashAfter { get; set; }
        public bool? AddEqual { get; set; }
        public bool? AddAmp { get; set; }
    }
}
