using System.IO;

namespace StreamLib.Utils
{
    public static class Bits
    {
        public static uint[] GetBits(byte[] mBytes)
        {
            int bitSize = mBytes.Length/4;
            uint[] bits = new uint[bitSize];
            using (var ms = new MemoryStream(mBytes))
            using (var br = new BinaryReader(ms))
            {
                for (int i = 0; i < bitSize; i++)
                    bits[i] = br.ReadUInt32();
            }
            return bits;
        }

        /**
         * This method might be better described as
         * "byte array to int array" or "data input to int array"
        */
        //public static int[] GetBits(DataInput dataIn, int byteLength)
        //{
        //    int bitSize = byteLength/4;
        //    int[] bits = new int[bitSize];
        //    for (int i = 0; i < bitSize; i++)
        //    {
        //        bits[i] = dataIn.readInt();
        //    }
        //    return bits;
        //}
    }
}