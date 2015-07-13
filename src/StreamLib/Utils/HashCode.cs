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
    }
}