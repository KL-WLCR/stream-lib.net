namespace StreamLib.Utils
{
    internal static class UInt64
    {
        /// <summary>
        /// Returns the number of zero bits preceding the highest-order
        /// ("leftmost") one-bit in the two's complement binary representation
        /// of the specified <paramref name="value"/>. Returns 64 if the specified value
        /// has no one-bits in its two's complement representation,
        /// in other words if it is equal to zero.
        /// </summary>
        public static uint NumberOfLeadingZeros(ulong value) {
            if (value == 0) return 64;
            uint n = 1;
            uint x = (uint)(value >> 32);
            if (x == 0) { n += 32; x = (uint)value; }
            if (x >> 16 == 0) { n += 16; x <<= 16; }
            if (x >> 24 == 0) { n +=  8; x <<=  8; }
            if (x >> 28 == 0) { n +=  4; x <<=  4; }
            if (x >> 30 == 0) { n +=  2; x <<=  2; }
            n -= x >> 31;
            return n;
        }
    }
}