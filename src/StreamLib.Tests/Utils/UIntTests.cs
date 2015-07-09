using System;
using System.Linq;
using FsCheck;
using NUnit.Framework;
using UInt32 = StreamLib.Utils.UInt32;
using UInt64 = StreamLib.Utils.UInt64;

namespace StreamLib.Tests.Utils
{
    [TestFixture]
    public class UIntTests
    {
        [Test]
        public void UInt32_NumberOfLeadingZeros()
        {
            Prop.ForAll<uint>(num =>
            {
                var actual = UInt32.NumberOfLeadingZeros(num);
                var expected = Convert.ToString(num, 2).PadLeft(32, '0').TakeWhile(c => c == '0').Count();
                return actual == expected;
            }).QuickCheckThrowOnFailure();
        }

        [Test]
        public void UInt32_BitCount()
        {
            Prop.ForAll<uint>(num =>
            {
                var actual = UInt32.BitCount(num);
                var expected = Convert.ToString(num, 2).PadLeft(32, '0').Count(c => c == '1');
                return actual == expected;
            }).QuickCheckThrowOnFailure();
        }

        [Test]
        public void UInt64_NumberOfLeadingZeros()
        {
            Prop.ForAll<ulong>(num =>
            {
                var actual = UInt64.NumberOfLeadingZeros(num);
                var expected = Convert.ToString((long)num, 2).PadLeft(64, '0').TakeWhile(c => c == '0').Count();
                return actual == expected;
            }).QuickCheckThrowOnFailure();
        }
    }
}
