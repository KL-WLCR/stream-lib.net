using System;
using Murmur;

namespace StreamLib.Tests
{
    public static class Murmur128Extensions
    {
        public static uint ComputeHash32(this Murmur32 murmur, byte[] data)
        {
            var hash32 = murmur.ComputeHash(data);
            return BitConverter.ToUInt32(hash32, 0);
        }

        public static ulong ComputeHash64(this Murmur128 murmur, byte[] data)
        {
            var hash128 = murmur.ComputeHash(data);
            var hash64 = BitConverter.ToUInt64(hash128, 0); // take first 64 bits of 128 bit hash
            return hash64;
        }
    }

    public static class RandomExtensions
    {
        public static long NextLong(this Random rnd)
        {
            var buf = new byte[8];
            rnd.NextBytes(buf);
            return BitConverter.ToInt64(buf, 0);
        }

        public static long NextLong(this Random rnd, long minInlusive, long maxExclusive)
        {
            var buf = new byte[8];
            rnd.NextBytes(buf);
            var longRand = BitConverter.ToInt64(buf, 0);
            return (Math.Abs(longRand % (maxExclusive - minInlusive)) + minInlusive);
        }
    }
}