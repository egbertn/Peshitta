using System;
using Peshitta.Infrastructure.Utils;

namespace Peshitta.Infrastructure.Models
{
    public class HistoryKey(int textid, DateTime archivedate)
    {
        public readonly int id = textid;
        public readonly DateTime ArchiveDate = archivedate;

        public override bool Equals(object obj)
        {
            if (obj is HistoryKey key)
            {
                return this.ArchiveDate == key.ArchiveDate && this.id == key.id;
            }
            return false;
        }
        public override int GetHashCode()
        {
            //we just use the decimal since that is 16 bytes from which we will use 12 bytes
            Span<byte> byts = new byte[sizeof(int)];
            Span<byte> bytes = stackalloc byte[sizeof(long)];
            var bn = ArchiveDate.ToBinary();
            BitConverter.TryWriteBytes(bytes, bn);
            return bytes.GetStableHashCode();
        }

    }
}
