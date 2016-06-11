using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using StreamLib.Utils;

using ChunkedByteArray = StreamLib.Utils.ChunkedArray<byte>;

namespace StreamLib.Utils.Streams
{
    public class ReadOnlyChunkedArrayStream : Stream
    {
        ChunkedByteArray _buffer;

        int _position;

        public ReadOnlyChunkedArrayStream(ChunkedByteArray buffer)
        {
            _position = 0;
            _buffer = buffer;
        }

        public override bool CanRead
        {
            get { return true; }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        protected override void Dispose(bool disposing)
        {
        }

        public override void Flush()
        {
            _position = 0;
        }

        public override long Length
        {
            get
            {
                return _buffer.Length;
            }
        }

        public override long Position
        {
            get
            {
                return (long)_position;
            }
            set
            {
                throw new Exception("not implemented");
            }
        }

        public override int Read([In, Out] byte[] buffer, int offset, int count)
        {
            int len = 0;
            for ( var i = 0; i < count; ++i )
            {
                if (_position == _buffer.Length)
                    break;

                buffer[offset + i] = _buffer[_position];
                ++_position;
                ++len;
            }

            return len;
        }

        public override int ReadByte()
        {
            if (_position == _buffer.Length)
                return -1;
            else
                return (int)_buffer[_position++];
        }
        
        public override long Seek(long offset, SeekOrigin loc)
        {
            throw new Exception("not implemented");
        }

        public override void SetLength(long value)
        {
            throw new Exception("not implemented");
        }

        public virtual byte[] ToArray()
        {
            throw new Exception("not implemented");
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new Exception("not implemented");
        }

        public override void WriteByte(byte value)
        {
            throw new Exception("not implemented");
        }

        public virtual void WriteTo(Stream stream)
        {
            throw new Exception("not implemented");
        }


    }
}
