using StreamLib.Cardinality;
using System;
using ChunkedArray = StreamLib.Utils.ChunkedArray<uint>;
using ChunkedByteArray = StreamLib.Utils.ChunkedArray<byte>;

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

        public static ChunkedArray GetBits(ChunkedByteArray bytes, int offset = 0)
        {
            var position = offset;
            var blocks = ( bytes.Length - offset ) / 4 ;
            var result = new ChunkedArray(blocks);

            for (int curBlock = 0; curBlock < blocks; ++curBlock)
            {
                result[curBlock] = BitConverter.ToUInt32(bytes.GetPart(position, 4), 0);
                position += 4;
            }

            return result;
        }

    }
}