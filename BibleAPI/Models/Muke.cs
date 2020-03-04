using System;

namespace peshitta.nl.Api
{
    public class TimestampParams
    {
        public int[] pTextId { get; set; }
        public DateTimeOffset[] pTimeStamp { get; set; }
    }
    public class VerseTemp
    {
        public int pTextId { get; set; }
        public DateTimeOffset pTimeStamp { get; set; }
        public string pContent { get; set; }
        public string pFootNoteText { get; set; }
        public string pHeaderText { get; set; }
    }
}