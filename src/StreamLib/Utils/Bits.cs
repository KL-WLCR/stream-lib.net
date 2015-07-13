namespace StreamLib.Utils
{
    public static class Bits
    {
        public static uint[] GetBits(byte[] bytes)
        {
            var blocks = bytes.Length / 4;
            var result = new uint[blocks];

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