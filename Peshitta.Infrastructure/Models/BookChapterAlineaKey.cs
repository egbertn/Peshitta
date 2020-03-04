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
        public override int GetHashCode()
        {
            var bytesCount = Encoding.UTF8.GetByteCount(Word);
            var bytes = new byte[bytesCount+4];

            Array.Copy(Encoding.UTF8.GetBytes(Word), 0, bytes, 0, bytesCount);
            Array.Copy(BitConverter.GetBytes(langid), 0, bytes, bytesCount, 4);
            var dest = new byte[4];
            Utils.HashData.Hash(bytes, ref dest);
            return BitConverter.ToInt32(dest, 0);
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
                ulong p = (((ulong)bookchapteralineaid << shiftit)) | (ulong)Alineaid;
                int outp;
                var bits = BitConverter.GetBytes(p);
                var dest = new byte[4];
                Utils.HashData.Hash(bits, ref dest);
                outp = BitConverter.ToInt32(dest, 0);
                return outp;
                //p.GetHashCode() does not deliver unique values
            }
        }

        public bool Equals(BookChapterAlineaKey x, BookChapterAlineaKey y)
        {
            throw new NotImplementedException();
        }

        public int GetHashCode(BookChapterAlineaKey obj)
        {
            throw new NotImplementedException();
        }

     

        public readonly int bookchapteralineaid;
        public readonly int Alineaid;
    }
}
