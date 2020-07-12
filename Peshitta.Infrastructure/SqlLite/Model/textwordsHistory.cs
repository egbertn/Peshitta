namespace Peshitta.Infrastructure.Sqlite.Model
{
    [System.Diagnostics.DebuggerDisplay("{words}, id={id} textid={textid}, wordid={wordid}")]
    public class TextWordsHistory
    {
        public TextWordsHistory() 
        {
        }

        public TextWordsHistory(TextWords tw, System.DateTimeOffset now, int newId)
        {
            id = newId;
            wordid = tw.wordid;
            words = tw.words;
            ArchiveDate = now;
            textid = tw.textid;
            AddComma = tw.AddComma;
            AddDot = tw.AddDot;
            AddSpace = tw.AddSpace;
            IsAllCaps = tw.IsAllCaps;
            IsCapitalized = tw.IsCapitalized;
            IsFootNote = tw.IsFootNote;
            IsHeader = tw.IsHeader;
            AddColon = tw.AddColon;
            AddHyphenMin = tw.AddHyphenMin;
            Semicolon = tw.Semicolon;
            LBracket = tw.LBracket;
            PreSpace = tw.PreSpace;
            RBracket = tw.RBracket;
            RParentThesis = tw.RParentThesis;
            LParentThesis = tw.LParentThesis;
            LDQuote = tw.LDQuote;
            RDQuote = tw.RDQuote;
            RSQuote = tw.RSQuote;
            LSQuote = tw.LSQuote;
            AddSlash = tw.AddSlash;
            AddGT = tw.AddGT;
            AddLT = tw.AddLT;
            AddBang = tw.AddBang;
            AddSlashAfter = tw.AddSlashAfter;
            QMark = tw.QMark;
            AddEqual = tw.AddEqual;
            AddAmp = tw.AddAmp;
        }
    
        public TextWords ToTw()
        {
            return new TextWords
            {
                wordid = this.wordid,

                textid = this.textid,
                AddComma = this.AddComma,
                AddDot = this.AddDot,
                AddSpace = this.AddSpace,
                IsAllCaps = this.IsAllCaps,
                IsCapitalized = this.IsCapitalized,
                IsFootNote = this.IsFootNote,
                IsHeader = this.IsHeader,
                AddColon = this.AddColon,
                AddHyphenMin = this.AddHyphenMin,
                Semicolon = this.Semicolon,
                LBracket = this.LBracket,
                PreSpace = this.PreSpace,
                RBracket = this.RBracket,
                RParentThesis = this.RParentThesis,
                LParentThesis = this.LParentThesis,
                LDQuote = this.LDQuote,
                RDQuote = this.RDQuote,
                RSQuote = this.RSQuote,
                LSQuote = this.LSQuote,
                AddSlash = this.AddSlash,
                AddGT = this.AddGT,
                AddLT = this.AddLT,
                AddBang = this.AddBang,
                AddSlashAfter = this.AddSlashAfter,
                QMark = this.QMark,
                AddEqual = this.AddEqual,

                AddAmp = this.AddAmp
            };
           
        }
        private bool IsBitSet(byte pos)
        {
            return (flags & (1 << pos)) != 0;
        }
        private void SetBit(byte position, bool value)
        {
            if (value)
            {
                flags |= (1L << position);
            }
            else
            {
                flags &= ~(1L << position);
            }
        }

        public int id { get; set; }
        public int textid { get; set; }
        public int wordid { get; set; }
        public long flags { get; set; }

        public bool IsCapitalized { get { return IsBitSet(1); } set { SetBit(1, value); } }
        public bool AddSpace { get { return IsBitSet(2); } set { SetBit(2, value); } }
        public bool IsAllCaps { get { return IsBitSet(3); } set { SetBit(3, value); } }
        public bool IsFootNote { get { return IsBitSet(4); } set { SetBit(4, value); } }
        public bool AddDot { get { return IsBitSet(5); } set { SetBit(5, value); } }
        public bool AddComma { get { return IsBitSet(6); } set { SetBit(6, value); } }
        public bool IsHeader { get { return IsBitSet(7); } set { SetBit(7, value); } }
        public bool LParentThesis { get { return IsBitSet(8); } set { SetBit(8, value); } }
        public bool RParentThesis { get { return IsBitSet(9); } set { SetBit(9, value); } }
        public bool LBracket { get { return IsBitSet(10); } set { SetBit(10, value); } }
        public bool RBracket { get { return IsBitSet(11); } set { SetBit(11, value); } }

        public bool Semicolon { get { return IsBitSet(12); } set { SetBit(12, value); } }
        public bool PreSpace { get { return IsBitSet(13); } set { SetBit(13, value); } }
        public bool AddColon { get { return IsBitSet(14); } set { SetBit(14, value); } }
        public bool AddHyphenMin { get { return IsBitSet(15); } set { SetBit(15, value); } }
        public bool RDQuote { get { return IsBitSet(16); } set { SetBit(16, value); } }
        public bool LDQuote { get { return IsBitSet(17); } set { SetBit(17, value); } }
        public bool RSQuote { get { return IsBitSet(18); } set { SetBit(18, value); } }
        public bool LSQuote { get { return IsBitSet(19); } set { SetBit(19, value); } }
        public bool AddLT { get { return IsBitSet(20); } set { SetBit(20, value); } }
        public bool AddGT { get { return IsBitSet(21); } set { SetBit(21, value); } }
        public bool AddSlash { get { return IsBitSet(22); } set { SetBit(22, value); } }
        public bool AddBang { get { return IsBitSet(23); } set { SetBit(23, value); } }
        public bool QMark { get { return IsBitSet(24); } set { SetBit(24, value); } }
        public bool AddSlashAfter { get { return IsBitSet(25); } set { SetBit(25, value); } }
        public bool AddEqual { get { return IsBitSet(26); } set { SetBit(26, value); } }
        public bool PrefixAmp { get { return IsBitSet(27); } set { SetBit(27, value); } }
        public bool AddAmp { get { return IsBitSet(28); } set { SetBit(28, value); } }
        public Text Text { get; set; }
        public words words { get; set; }

        public System.DateTimeOffset ArchiveDate { get; set; }

      
	}
}
