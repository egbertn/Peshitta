using System;
using System.Security.Cryptography;
using System.Text;
using Peshitta.Infrastructure.Utils;

namespace Peshitta.Infrastructure.Models
{
    public class WordLanguageKey
    {
        public WordLanguageKey(string word, int langid)
        {
            Word = word;
            this.langid = langid;
        }
        public string Word { get; }
        public int langid { get; }
        public override bool Equals(object obj)
        {
            return this.GetHashCode() == obj.GetHashCode();
        }
        public override int GetHashCode()
        {
            var bytesCount = Encoding.UTF8.GetByteCount(Word);
            Span<byte> bytes = stackalloc byte[bytesCount+4];
            Encoding.UTF8.GetBytes(Word, bytes);
            return bytes.GetStableHashCode();
        }
    }

    public class BookChapterAlineaKey
    {

        public BookChapterAlineaKey(int bca, int alinea)
        {
            this.bookchapteralineaid = bca;
            this.Alineaid = alinea;

        }
        public override bool Equals(object obj)
        {
            if (obj is BookChapterAlineaKey bca)
            {
                return obj.GetHashCode() == this.GetHashCode();
            };
            return false;
        }
        public unsafe override int GetHashCode()
        {
            unchecked
            {
                byte shiftit = 32;
                ulong p = ((ulong)bookchapteralineaid << shiftit) | (uint)Alineaid;
                return HashCode.Combine(p);
            }
        }

        public readonly int bookchapteralineaid;
        public readonly int Alineaid;
    }
}
