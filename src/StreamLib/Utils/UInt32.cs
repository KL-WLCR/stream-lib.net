 namespace StreamLib.Utils
{
    internal static class UInt32
    {
        /// <summary>
        /// Returns the number of zero bits preceding the highest-order
        /// ("leftmost") one-bit in the two's complement binary representation
        /// of the specified <paramref name="i"/> value. Returns 32 if the
        /// specified value has no one-bits in its two's complement representation,
        /// in other words if it is equal to zero.
        /// </summary>
        public static uint NumberOfLeadingZeros(uint i)
        {
            if (i == 0) return 32;
            //var ui = (uint) i;
            uint n = 1;
            if (i >> 16 == 0) { n += 16; i <<= 16; }
            if (i >> 24 == 0) { n +=  8; i <<=  8; }
            if (i >> 28 == 0) { n +=  4; i <<=  4; }
            if (i >> 30 == 0) { n +=  2; i <<=  2; }
            n -= i >> 31;
            return n;
        }

        /// <summary>
        /// Returns the number of one-bits in the two's complement binary
        /// representation of the specified value.  This function is
        /// sometimes referred to as the population count.
        /// </summary>
        /// <param name="i">the value whose bits are to be counted</param>
        /// <returns>the number of one-bits in the two's complement binary representation of the specified value</returns>
        public static uint BitCount(uint i)
        {
            i = i - ((i >> 1) & 0x55555555);
            i = (i & 0x33333333) + ((i >> 2) & 0x33333333);
            i = (i + (i >> 4)) & 0x0f0f0f0f;
            i = i + (i >> 8);
            i = i + (i >> 16);
            return i & 0x3f;
        }
    }
}