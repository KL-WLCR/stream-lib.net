using System;
using System.Runtime.CompilerServices;

namespace StreamLib.Utils.Streams
{
    namespace System.IO
    {
        // this class copy of MemoryStream with write-only ability and highly optimized:
        //   * no any contract checking
        //   * not derived from Stream since .net not inline virtual methods
        internal class WriteOnlyMemoryStream : IDisposable
        {
            const int ArrayMaxByteArrayLength = 0x7FFFFFC7;

            readonly byte[] _tmpBuffer = new byte[4];
            byte[] _buffer;    // either allocated internally or externally
            int _position;     // read/write head
            int _length;       // number of bytes within the memory stream
            int _capacity;     // length of usable portion of buffer for stream
            // Note that _capacity == _buffer.Length for non-user-provided byte[]'s

            public WriteOnlyMemoryStream()
                : this(0)
            {
            }

            public WriteOnlyMemoryStream(int capacity)
            {
                _buffer = new byte[capacity];
                _capacity = capacity;
            }

            public WriteOnlyMemoryStream(byte[] buffer)
            {
                if (buffer == null) throw new ArgumentNullException("buffer");
                _buffer = buffer;
                _length = _capacity = buffer.Length;
            }

            public void Dispose()
            {
                _buffer = null;
            }

            public byte[] GetBuffer()
            {
                return _buffer;
            }

            public byte[] ToArray()
            {
                byte[] copy = new byte[_length];
                Array.Copy(_buffer, 0, copy, 0, _length);
                return copy;
            }

            // Gets & sets the capacity (number of bytes allocated) for this stream.
            // The capacity cannot be set to a value less than the current length
            // of the stream.
            public int Capacity
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get
                {
                    return _capacity;
                }
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                set
                {
                    // Only update the capacity if the value is different than the current capacity
                    if (value != _capacity)
                    {
                        if (value > 0)
                        {
                            byte[] newBuffer = new byte[value];
                            if (_length > 0) Array.Copy(_buffer, 0, newBuffer, 0, _length);
                            _buffer = newBuffer;
                        }
                        else
                        {
                            _buffer = null;
                        }
                        _capacity = value;
                    }
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Write(byte[] buffer, int offset, int count)
            {
                int i = _position + count;
                if (i > _length)
                {
                    bool mustZero = _position > _length;
                    if (i > _capacity)
                    {
                        bool allocatedNewArray = EnsureCapacity(i);
                        if (allocatedNewArray)
                            mustZero = false;
                    }
                    if (mustZero)
                        Array.Clear(_buffer, _length, i - _length);
                    _length = i;
                }
                if ((count <= 8) && (buffer != _buffer))
                {
                    int byteCount = count;
                    while (--byteCount >= 0)
                        _buffer[_position + byteCount] = buffer[offset + byteCount];
                }
                else
                    Array.Copy(buffer, offset, _buffer, _position, count);
                _position = i;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void WriteByte(byte value)
            {
                if (_position >= _length)
                {
                    int newLength = _position + 1;
                    bool mustZero = _position > _length;
                    if (newLength >= _capacity)
                    {
                        bool allocatedNewArray = EnsureCapacity(newLength);
                        if (allocatedNewArray)
                            mustZero = false;
                    }
                    if (mustZero)
                        Array.Clear(_buffer, _length, _position - _length);
                    _length = newLength;
                }
                _buffer[_position++] = value;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void WriteUInt(uint value)
            {
                _tmpBuffer[0] = (byte)value;
                _tmpBuffer[1] = (byte)(value >> 8);
                _tmpBuffer[2] = (byte)(value >> 16);
                _tmpBuffer[3] = (byte)(value >> 24);
                Write(_tmpBuffer, 0, 4);
            }

            // returns a bool saying whether we allocated a new array
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            bool EnsureCapacity(int value)
            {
                if (value > _capacity)
                {
                    int newCapacity = value;
                    if (newCapacity < 256)
                        newCapacity = 256;
                    // We are ok with this overflowing since the next statement will deal
                    // with the cases where _capacity*2 overflows.
                    if (newCapacity < _capacity * 2)
                        newCapacity = _capacity * 2;
                    // We want to expand the array up to Array.MaxArrayLengthOneDimensional
                    // And we want to give the user the value that they asked for
                    if ((uint)(_capacity * 2) > ArrayMaxByteArrayLength)
                        newCapacity = value > ArrayMaxByteArrayLength ? value : ArrayMaxByteArrayLength;

                    Capacity = newCapacity;
                    return true;
                }
                return false;
            }
        }
    }
}