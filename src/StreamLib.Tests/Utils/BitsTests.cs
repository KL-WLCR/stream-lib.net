using System.IO;
using System.Linq;
using FsCheck;
using NUnit.Framework;
using StreamLib.Utils;
using StreamLib.Cardinality;

using ChunkedArray = StreamLib.Utils.ChunkedArray<uint>;
using ChunkedByteArray = StreamLib.Utils.ChunkedArray<byte>;

namespace StreamLib.Tests.Utils
{
    [TestFixture]
    public class BitsTests
    {
        static ChunkedArray SlowGetBits(byte[] bytes)
        {
            int bitSize = bytes.Length / 4;
            ChunkedArray bits = new ChunkedArray (bitSize);
            using (var ms = new MemoryStream(bytes))
            using (var br = new BinaryReader(ms))
            {
                for (int i = 0; i < bitSize; i++)
                    bits[i] = br.ReadUInt32();
            }
            return bits;
        }

        [Test]
        public void GetBits()
        {
            var inputGen = Arb.Generate<byte[]>().Where(ar => ar.Length > 0 && ar.Length % 4 == 0);
            Prop.ForAll(
                Arb.From(inputGen),
                bytes =>  Bits.GetBits(bytes).SequenceEqual(SlowGetBits(bytes)))
            .QuickCheckThrowOnFailure();
        }

        [Test]
        public void GetBitsChunked()
        {
            var inputGen = Arb.Generate<byte[]>().Where(ar => ar.Length > 0 && ar.Length % 4 == 0);
            Prop.ForAll(
                Arb.From(inputGen),
                bytes => Bits.GetBits(ChunkedByteArray.CreateFromArray(bytes)).SequenceEqual(SlowGetBits(bytes)))
            .QuickCheckThrowOnFailure();
        }
    }
}