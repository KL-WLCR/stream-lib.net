using System;
using NUnit.Framework;
using StreamLib.Cardinality;

namespace StreamLib.Tests.Cardinality
{
    [TestFixture]
    public class RegisterSetTests
    {
        static uint Pow(int num, int power)
        {
            return (uint)Math.Pow(num, power);
        }

        [Test]
        public void GetAndSet()
        {
            var rs = new RegisterSet(Pow(2, 4));
            rs.Set(0, 11);
            Assert.That(rs.Get(0), Is.EqualTo(11));
        }

        [Test]
        public void GetAndSet_AllPositions()
        {
            var rs = new RegisterSet(Pow(2, 4));
            for (uint i = 0; i < Pow(2, 4); ++i)
            {
                rs.Set(i, i % 31);
                Assert.That(rs.Get(i), Is.EqualTo(i % 31));
            }
        }

        [Test]
        public void GetAndSet_WithSmallBits()
        {
            var rs = new RegisterSet(6);
            rs.Set(0, 11);
            Assert.That(rs.Get(0), Is.EqualTo(11));
        }

        [Test]
        public void Merge()
        {
            const int count = 32;
            var rand = new Random(2);

            var rs = new RegisterSet(count);
            var rss = new RegisterSet[5];

            for (var i = 0; i < rss.Length; ++i)
            {
                rss[i] = new RegisterSet(count);
                for (uint pos = 0; pos < rs.Count; ++pos)
                {
                    var val = (uint)rand.Next(10);
                    rs.UpdateIfGreater(pos, val);
                    rss[i].Set(pos, val);
                }
            }

            var merged = new RegisterSet(count);
            foreach (var t in rss)
                merged.Merge(t);

            for (uint pos = 0; pos < rs.Count; pos++)
                Assert.That(merged.Get(pos), Is.EqualTo(rs.Get(pos)));
        }

        [Test]
        public void MergeUsingUpdate()
        {
            const int count = 32;
            var rand = new Random(2);
            var rs = new RegisterSet(count);
            var rss = new RegisterSet[5];

            for (var i = 0; i < rss.Length; ++i)
            {
                rss[i] = new RegisterSet(count);
                for (uint pos = 0; pos < rs.Count; ++pos)
                {
                    var val = (uint)rand.Next(10);
                    rs.UpdateIfGreater(pos, val);
                    rss[i].Set(pos, val);
                }
            }

            var merged = new RegisterSet(count);
            foreach (var t in rss)
            {
                for (uint pos = 0; pos < rs.Count; pos++)
                {
                    merged.UpdateIfGreater(pos, t.Get(pos));
                }
            }

            for (uint pos = 0; pos < rs.Count; pos++)
                Assert.That(merged.Get(pos), Is.EqualTo(rs.Get(pos)));
        }
    }
}