 namespace StreamLib.Utils
{
    internal static class UInt32
    {
        /**
         * Returns the number of zero bits preceding the highest-order
         * ("leftmost") one-bit in the two's complement binary representation
         * of the specified {@code int} value.  Returns 32 if the
         * specified value has no one-bits in its two's complement representation,
         * in other words if it is equal to zero.
         *
         * <p>Note that this method is closely related to the logarithm base 2.
         * For all positive {@code int} values x:
         * <ul>
         * <li>floor(log<sub>2</sub>(x)) = {@code 31 - numberOfLeadingZeros(x)}
         * <li>ceil(log<sub>2</sub>(x)) = {@code 32 - numberOfLeadingZeros(x - 1)}
         * </ul>
         *
         * @param i the value whose number of leading zeros is to be computed
         * @return the number of zero bits preceding the highest-order
         *     ("leftmost") one-bit in the two's complement binary representation
         *     of the specified {@code int} value, or 32 if the value
         *     is equal to zero.
         * @since 1.5
         */
        public static uint NumberOfLeadingZeros(uint i) {
            if (i == 0) return 32;
            //var ui = (uint) i;
            uint n = 1;
            if (i >> 16 == 0) { n += 16; i <<= 16; } // todo was >>>
            if (i >> 24 == 0) { n +=  8; i <<=  8; } // todo was >>>
            if (i >> 28 == 0) { n +=  4; i <<=  4; } // todo was >>>
            if (i >> 30 == 0) { n +=  2; i <<=  2; } // todo was >>>
            n -= i >> 31;                            // todo was >>>
            return n;
        }
    }
}