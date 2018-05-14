using System;
using System.Runtime.InteropServices;

namespace Peshitta.Data.Models
{
    public class HistoryKey
    {
        public readonly int id;
        public readonly DateTime ArchiveDate;

        [DllImport("Shlwapi.dll")]
        internal unsafe static extern uint HashData(void* pbData, int cbData,
                        void* piet,
                        int outputLen);



        public HistoryKey(int textid, DateTime archivedate)
        {
            id = textid;
            ArchiveDate = archivedate;
        }
        public override bool Equals(object obj)
        {
            if (obj is HistoryKey key)
            {
                return this.ArchiveDate == key.ArchiveDate && this.id == key.id;
            }
            return false;
        }
        public unsafe override int GetHashCode()
        {
            //we just use the decimal since that is 16 bytes from which we will use 12 bytes
            decimal d;
            var p = ArchiveDate.ToBinary();
            int* ptr = (int*)&d;
            *(long*)ptr = p;
            ptr += sizeof(long);
            *ptr = id;
            ptr -= sizeof(long);

            // return d.GetHashCode();
            int piet;

            var result = HashData(&d, 12, &piet, sizeof(int));
            if (result != 0)
            {
                return 0;
            }    //bt.GetHashCode does not work correctly
            return piet;
        }

    }
}
