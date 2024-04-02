namespace peshitta.nl;

public static class HashHelper
{
    public static int GetStableHashCode(this Span<byte> data)
    {   //FNV-1a algorithm
        unchecked
        {
            const int p = 16777619;
            int hash = (int)2166136261;
            var l = data.Length;
            for (int i = 0; i < l; i++)
                hash = (hash ^ data[i]) * p;

            hash += hash << 13;
            hash ^= hash >> 7;
            hash += hash << 3;
            hash ^= hash >> 17;
            hash += hash << 5;
            return hash;
        }
    }
}