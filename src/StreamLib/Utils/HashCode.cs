namespace StreamLib.Utils
{
    internal static class HashCode
    {
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