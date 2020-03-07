using System.Collections.Generic;

namespace Peshitta.Infrastructure.Models
{
    public sealed class AlineaText
    {
        public int BookChapterAlineaId { get; set; }
        public int Alineaid { get; set; }
        public IEnumerable<TextExpanded> Texts { get; set; }
       
    }
}
