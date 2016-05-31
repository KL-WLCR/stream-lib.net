using StreamLib.Cardinality;

using ChunkedArray = StreamLib.Utils.ChunkedArray<uint>;

namespace StreamLib.Utils
{
    public static class Bits
    {
        public static ChunkedArray GetBits(byte[] bytes)
        {
            var blocks = bytes.Length / 4;
            var result = new ChunkedArray (blocks);

            unsafe
            {
                fixed (byte* fix = &bytes[0])
                {
                    int curBlock = 0;
                    uint* curUint = (uint*)fix;
                    while (curBlock < blocks)
                    {
                        result[curBlock] = *curUint;
                        curUint++;
                        curBlock++;
                    }
                }
            }
            return result;
        }
    }
}