using System.IO;
using FsCheck;
using NUnit.Framework;
using StreamLib.Utils;

namespace StreamLib.Tests.Utils
{
    [TestFixture]
    public class VarintTests
    {
        [Test]
        public void UInt_WrittenAsVarint_IsSameUInt_When_ReadedBack()
        {
            Prop.ForAll<uint>(num =>
            {
                using (var ms = new MemoryStream())
                {
                    Varint.WriteUInt32(num, ms);
                    ms.Seek(0, SeekOrigin.Begin);
                    var readed = Varint.ReadUInt32(ms);
                    return readed == num;
                }
            }).QuickCheckThrowOnFailure();
        }
    }
}