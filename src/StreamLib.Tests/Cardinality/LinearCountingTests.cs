using System;
using System.Linq;
using FsCheck;
using NUnit.Framework;
using StreamLib.Cardinality;

namespace StreamLib.Tests.Cardinality
{
    [TestFixture]
    public class LinearCountingTests : TestBase
    {
        [Test]
        public void OfferReturnsWasModifiedInternalBytes()
        {
            Prop.ForAll<int>(num =>
            {
                var lc = new LinearCounter(4);
                var hash = Hash32(num);
                Assert.True(lc.OfferHashed(hash), "first offer");
                Assert.False(lc.OfferHashed(hash), "second offer");
            }).QuickCheck();
        }

        [Test]
        public void LinearCounterFullfilled_CardinalityReturnMaxValue()
        {
            var lc = new LinearCounter(1);
            while (lc.UnsetBits > 0)
                lc.OfferHashed(Hash32(Rnd.Next()));
            Assert.That(lc.Cardinality(), Is.EqualTo(ulong.MaxValue), "Cardinality");
        }

        [Test]
        public void ArbitraryStdErrorSize()
        {
            // some sanity check with 1% error
            Assert.That(LinearCounter.CreateWithError(0.01, 100).Bitmap.Length, Is.EqualTo(630));
            Assert.That(LinearCounter.CreateWithError(0.01, 3375).Bitmap.Length, Is.EqualTo(759));

            // checking for 10% error (values from original paper)
            Assert.That(LinearCounter.CreateWithError(0.1, 100).Bitmap.Length, Is.EqualTo(10));
            Assert.That(LinearCounter.CreateWithError(0.1, 1000).Bitmap.Length, Is.EqualTo(34));
            Assert.That(LinearCounter.CreateWithError(0.1, 10000).Bitmap.Length, Is.EqualTo(214));
            Assert.That(LinearCounter.CreateWithError(0.1, 100000).Bitmap.Length, Is.EqualTo(1593));
            Assert.That(LinearCounter.CreateWithError(0.1, 1000000).Bitmap.Length, Is.EqualTo(12610));
            Assert.That(LinearCounter.CreateWithError(0.1, 10000000).Bitmap.Length, Is.EqualTo(103977));
            Assert.That(LinearCounter.CreateWithError(0.1, 100000000).Bitmap.Length, Is.EqualTo(882720));
        }

        [Test]
        public void Serialization()
        {
            var lc = new LinearCounter(4);
            lc.OfferHashed(Hash32("a"));
            lc.OfferHashed(Hash32("b"));
            lc.OfferHashed(Hash32("c"));
            lc.OfferHashed(Hash32("d"));
            lc.OfferHashed(Hash32("e"));

            var lc2 = new LinearCounter(lc.Bitmap);

            Assert.That(lc2.Bitmap, Is.EqualTo(lc.Bitmap), "Bitmap");
            Assert.That(lc2.UnsetBits, Is.EqualTo(lc.UnsetBits), "UnsetBits");
            Assert.That(lc2.Cardinality(), Is.EqualTo(lc.Cardinality()), "Cardinality");
        }

        [Test]
        public void Merge()
        {
            const int size = 65536;
            const int numToMerge = 5;
            const int cardinality = 1000;
            const uint expectedCardinalityOfMerged = numToMerge * cardinality;

            var lcs = new LinearCounter[numToMerge];
            var baseline = new LinearCounter(size);

            for (var i = 0; i < numToMerge; ++i)
            {
                lcs[i] = new LinearCounter(size);
                for (var j = 0; j < cardinality; ++j)
                {
                    int val = Rnd.Next();
                    lcs[i].OfferHashed(Hash32(val));
                    baseline.OfferHashed(Hash32(val));
                }
            }

            ulong mergeAllEstimate = LinearCounter.MergeAll(lcs).Cardinality();
            double mergeAllError = Math.Abs((double)mergeAllEstimate - expectedCardinalityOfMerged) / expectedCardinalityOfMerged;
            Assert.That(mergeAllError, Is.EqualTo(0.01).Within(0.01));

            var mergeWithEstimate = lcs[0].MergeWith(lcs.Skip(1).ToArray()).Cardinality();
            var mergeWithError = Math.Abs((double)mergeWithEstimate - expectedCardinalityOfMerged) / expectedCardinalityOfMerged;
            Assert.That(mergeWithEstimate, Is.EqualTo(mergeAllEstimate));
            Assert.That(mergeWithError, Is.EqualTo(mergeAllError));

            ulong baselineEstimate = baseline.Cardinality();
            Assert.That(mergeWithEstimate, Is.EqualTo(baselineEstimate));
        }
    }
}