using StreamLib.Cardinality;

using ChunkedArray = StreamLib.Utils.ChunkedArray<uint>;

namespace StreamLib.Utils
{
    internal static class HashCode
    {
        // todo possible optimization with unsafe implementation
        public static int ForArray(uint[] array)
        {
            unchecked
            {
                uint result = 0;
                foreach (var v in array)
                    result = (result * 31) ^ v;
                return (int)result;
            }
        }

        // todo possible optimization with unsafe implementation
        public static int ForArray(ChunkedArray array)
        {
            unchecked
            {
                uint result = 0;

                for (var i=0;i<array.Length;++i)
                {
                    result = (result * 31) ^ array[i];
                }

                return (int)result;
            }
        }
    }
}