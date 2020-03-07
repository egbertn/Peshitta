﻿using System;

namespace Peshitta.Infrastructure.Models
{
    public class HistoryKey
    {
        public readonly int id;
        public readonly DateTime ArchiveDate;

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
            var byts = new byte[12];
            var bn = ArchiveDate.ToBinary();
            fixed (byte* ptr = byts)
            {
                *(int*)ptr = id;
                var newPtr = ptr + sizeof(int);
                *(long*)newPtr = bn;
            }
            
            // return d.GetHashCode();
          
            int hash;
            fixed (byte* ptr2 = byts)
            {
                var result = Utils.HashData.Hash(ptr2, 12, (byte*)&hash, 4);
                return hash;
            }
        }

    }
}
