using System;
using System.Text;

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
        public unsafe override int GetHashCode()
        {
            var bytesCount = Encoding.UTF8.GetByteCount(Word);
            var bytes = new byte[bytesCount+4];

            Array.Copy(Encoding.UTF8.GetBytes(Word), 0, bytes, 0, bytesCount);
            Array.Copy(BitConverter.GetBytes(langid), 0, bytes, bytesCount, 4);
            int hashCode;
            fixed (byte* src = bytes)
            {
                Utils.HashData.Hash(src, bytesCount + 4, (byte*)&hashCode, 4);
            }

            return hashCode;
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
                ulong p = ((uint)bookchapteralineaid << shiftit) | (uint)Alineaid;
                int outp;
                Utils.HashData.Hash((byte*)&p,8, (byte*)&outp, 4);

                return outp;
                //p.GetHashCode() does not deliver unique values
            }
        }



        public readonly int bookchapteralineaid;
        public readonly int Alineaid;
    }
}
