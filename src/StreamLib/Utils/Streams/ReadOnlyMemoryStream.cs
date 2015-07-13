using System;
using System.Runtime.CompilerServices;

namespace StreamLib.Utils.Streams
{
    // this class copy of MemoryStream with read-only ability and highly optimized:
    //   * no any contract checking
    //   * not derived from Stream since .net not inline virtual methods
    internal class ReadOnlyMemoryStream : IDisposable
    {
        byte[] _buffer;
        int _position;
        readonly int _length;

        public ReadOnlyMemoryStream(byte[] buffer)
        {
            _buffer = buffer;
            _length = buffer.Length;
        }

        public void Dispose()
        {
            _buffer = null;
        }

        public int Read(byte[] buffer, int offset, int count)
        {
            int n = _length - _position;
            if (n > count) n = count;
            if (n <= 0)
                return 0;

            if (n <= 8)
            {
                int byteCount = n;
                while (--byteCount >= 0)
                    buffer[offset + byteCount] = _buffer[_position + byteCount];
            }
            else
                Array.Copy(_buffer, _position, buffer, offset, n);

            _position += n;
            return n;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int ReadByte()
        {
            if (_position >= _length) return -1;
            return _buffer[_position++];
        }

        public int ReadInt32()
        {
            int pos = (_position += 4); // use temp to avoid a race condition
            return _buffer[pos - 4] | _buffer[pos - 3] << 8 | _buffer[pos - 2] << 16 | _buffer[pos - 1] << 24;
        }
    }
}