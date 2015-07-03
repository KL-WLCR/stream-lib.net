using System.IO;

namespace StreamLib.Utils
{
    public static class Bits
    {
        public static uint[] GetBits(byte[] bytes)
        {
            int bitSize = bytes.Length / 4;
            uint[] bits = new uint[bitSize];
            using (var ms = new MemoryStream(bytes))
            using (var br = new BinaryReader(ms))
            {
                for (int i = 0; i < bitSize; i++)
                    bits[i] = br.ReadUInt32();
            }
            return bits;
        }
    }
}