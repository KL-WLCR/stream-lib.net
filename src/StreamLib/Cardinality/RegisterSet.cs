namespace StreamLib.Cardinality
{
    public class RegisterSet
    {
        const uint Log2BitsPerWord = 6;
        const int RegisterSize = 5;

        public readonly uint Count;

        /// <summary>
        /// Readonly internal representation, do not modify it.
        /// </summary>
        public readonly uint[] M;

        public RegisterSet(uint count, uint[] initialValues = null)
        {
            Count = count;
            M = initialValues ?? new uint[GetSizeForCount(count)];
        }

        public override string ToString()
        {
            return string.Format("{{ Count = {0}, M.len = {1}}}", Count, M.Length);
        }

        public static uint GetSizeForCount(uint count)
        {
            uint bits = count / Log2BitsPerWord;
            if (bits == 0) return 1;
            if (bits % 32 == 0) return bits;
            return bits + 1;
        }

        public void Set(uint position, uint value)
        {
            uint bucketPos = position / Log2BitsPerWord;
            int shift = (int)(RegisterSize * (position - (bucketPos * Log2BitsPerWord)));
            M[bucketPos] = (M[bucketPos] & (uint)~(0x1f << shift)) | (value << shift);
        }

        public uint Get(uint position)
        {
            uint bucketPos = position / Log2BitsPerWord;
            int shift = (int)(RegisterSize * (position - (bucketPos * Log2BitsPerWord)));
            return (M[bucketPos] & (uint)(0x1f << shift)) >> shift;
        }

        public bool UpdateIfGreater(uint position, uint value)
        {
            uint bucket = position / Log2BitsPerWord;
            int shift = (int)(RegisterSize * (position - (bucket * Log2BitsPerWord)));
            int mask = 0x1F << shift;

            // use long to avoid sign issues with the left-most shift
            long curVal = M[bucket] & mask;
            long newVal = value << shift;
            if (curVal < newVal)
            {
                M[bucket] = (uint)((M[bucket] & ~mask) | newVal);
                return true;
            }
            return false;
        }

        public void Merge(RegisterSet other)
        {
            for (var bucket = 0; bucket < M.Length; ++bucket)
            {
                uint word = 0;
                for (var j = 0; j < Log2BitsPerWord; ++j)
                {
                    uint mask = 0x1Fu << (RegisterSize * j);
                    uint thisVal = M[bucket] & mask;
                    uint otherVal = other.M[bucket] & mask;
                    word |= (thisVal < otherVal) ? otherVal : thisVal;
                }
                M[bucket] = word;
            }
        }
    }
}