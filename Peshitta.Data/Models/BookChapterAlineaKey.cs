using System;


namespace Peshitta.Data.Models
{
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
                ulong p = (((ulong)bookchapteralineaid << 32)) | (ulong)Alineaid;
                int outp;
                HistoryKey.HashData(&p, sizeof(long), &outp, sizeof(int));
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
