
using System;

namespace Peshitta.Infrastructure.Models
{

    public class Text
    {
        public override bool Equals(object obj)
        {
            if (obj is BookChapterAlineaKey bca)
            {
                return bca.bookchapteralineaid == this.BookChapterAlineaid && bca.Alineaid == this.Alineaid;
            }
            else if (obj is Text txt)
            {
                return txt.TextId == this.TextId;
            }
            return false;
        }
        public override int GetHashCode()
        {
            return TextId;
        }


        public int TextId { get; set; }
        public int BookChapterAlineaid { get; set; }

        public int Alineaid { get; set; }

        public int bookeditionid { get; set; }

        public DateTime timestamp { get; set; }

        public short langid { get; set; }

    }


}
