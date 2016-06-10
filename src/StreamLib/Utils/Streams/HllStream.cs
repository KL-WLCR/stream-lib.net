using StreamLib.Cardinality;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace StreamLib.Utils.Streams
{

    public class HllStream : Stream
    {
        HyperLogLogPlus _hll;
        long _position;
        long _length;
        IEnumerator<byte> _serializer;

        public HllStream(HyperLogLogPlus hll)
        {
            _position = 0;
            _hll = hll;
            _length = _hll.GetBinSize();
            _serializer = _hll.GetSerializer();
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
            _serializer.Reset();
        }

        public override long Length
        {
            get
            {
                return _length;
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
            int till = offset + count;

            for (var i = offset; i < till; ++i)
            {
                if (_serializer.MoveNext())
                {
                    buffer[i] = _serializer.Current;
                    ++len;
                    ++_position;
                }
                else
                {
                    break;
                }

            }

            return len;
        }

        public override int ReadByte()
        {
            if (_serializer.MoveNext())
            {
                return (int)_serializer.Current;
            }

            return -1;
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
