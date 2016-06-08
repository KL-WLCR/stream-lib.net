using System;
using System.Runtime.CompilerServices;
using StreamLib.Utils.Streams;
using StreamLib.Utils.Streams.System.IO;

using ChunkedByteArray = StreamLib.Utils.ChunkedArray<byte>;

namespace StreamLib.Utils
{
    internal static class Varint
    {
        static readonly Exception Truncated = new Exception("While parsing bytes the input ended unexpectedly in the middle of a field.");
        static readonly Exception Malformed = new Exception("Stream encountered a malformed varint");

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint ReadUInt32(ReadOnlyMemoryStream input)
        {
            int result = 0;
            int offset = 0;
            for (; offset < 32; offset += 7)
            {
                int b = input.ReadByte();
                if (b == -1) throw Truncated;

                result |= (b & 0x7f) << offset;
                if ((b & 0x80) == 0)
                    return (uint)result;
            }
            // keep reading up to 64 bits
            for (; offset < 64; offset += 7)
            {
                int b = input.ReadByte();
                if (b == -1) throw Truncated;
                if ((b & 0x80) == 0)
                    return (uint)result;
            }
            throw Malformed;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint ReadUInt32(ChunkedByteArray input, ref int position)
        {
            int result = 0;
            int offset = 0;
            for (; offset < 32; offset += 7)
            {
                int b = input[position++]; //input.ReadByte();
                if (b == -1) throw Truncated;

                result |= (b & 0x7f) << offset;
                if ((b & 0x80) == 0)
                    return (uint)result;
            }
            // keep reading up to 64 bits
            for (; offset < 64; offset += 7)
            {
                int b = input[position++]; //input.ReadByte();
                if (b == -1) throw Truncated;
                if ((b & 0x80) == 0)
                    return (uint)result;
            }
            throw Malformed;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteUInt32(uint value, WriteOnlyMemoryStream output)
        {
            while ((value & 0xFFFFFF80) != 0)
            {
                output.WriteByte((byte)((value & 0x7F) | 0x80));
                value >>= 7;
            }
            output.WriteByte((byte)(value & 0x7F));
        }
    }
}