using System;
using System.Text;
using Murmur;

namespace StreamLib.Tests.Cardinality
{
    public class TestBase
    {
        protected static readonly Random Rnd = new Random();

        protected static uint Hash32(byte[] value)
        {
            var hashalg = MurmurHash.Create32(managed: false);
            return hashalg.ComputeHash32(value);
        }

        protected static uint Hash32(int value)
        {
            return Hash32(BitConverter.GetBytes(value));
        }

        protected static uint Hash32(string value)
        {
            return Hash32(Encoding.UTF8.GetBytes(value));
        }

        protected static ulong Hash64(byte[] value)
        {
            var hashalg = MurmurHash.Create128(managed: false);
            return hashalg.ComputeHash64(value);
        }

        protected static ulong Hash64(string value)
        {
            return Hash64(Encoding.UTF8.GetBytes(value));
        }

        protected static ulong Hash64(int value)
        {
            return Hash64(BitConverter.GetBytes(value));
        }

        protected static ulong Hash64(double value)
        {
            return Hash64(BitConverter.GetBytes(value));
        }

    }
}