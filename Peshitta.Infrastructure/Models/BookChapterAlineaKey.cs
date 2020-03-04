using System;


namespace Peshitta.Infrastructure.Models
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
