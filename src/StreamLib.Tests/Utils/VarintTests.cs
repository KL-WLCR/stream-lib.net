using FsCheck;
using NUnit.Framework;
using StreamLib.Utils;
using StreamLib.Utils.Streams;
using StreamLib.Utils.Streams.System.IO;

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
                using (var wms = new WriteOnlyMemoryStream())
                {
                    Varint.WriteUInt32(num, wms);
                    var varintBytes = wms.ToArray();

                    using (var ms = new ReadOnlyMemoryStream(varintBytes))
                    {
                        var readed = Varint.ReadUInt32(ms);
                        return readed == num;
                    }
                }
            }).QuickCheckThrowOnFailure();
        }
    }
}