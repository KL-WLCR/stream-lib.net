using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using StreamLib.Cardinality;

namespace StreamLib.Tests.Cardinality
{
    [TestFixture]
    public class HyperLogLogPlusTests : TestBase
    {
        [Test]
        [Ignore("https://github.com/addthis/stream-lib/issues/91")]
        public void Offer()
        {
            var hll = new HyperLogLogPlus(14,25);

            Assert.True(hll.OfferHashed(Hash64("123")));
            Assert.True(hll.OfferHashed(Hash64("ABC")));

            Assert.False(hll.OfferHashed(Hash64("123")));
            Assert.False(hll.OfferHashed(Hash64("ABC")));
        }

        [Test]
        public void Equals()
        {
            var hll1 = new HyperLogLogPlus(5, 25);
            var hll2 = new HyperLogLogPlus(5, 25);

            hll1.OfferHashed(Hash64("A"));
            hll2.OfferHashed(Hash64("A"));
            Assert.That(hll1, Is.EqualTo(hll2));

            hll2.OfferHashed(Hash64("B"));
            hll2.OfferHashed(Hash64("C"));
            hll2.OfferHashed(Hash64("D"));
            Assert.That(hll1, Is.Not.EqualTo(hll2));

            var hll3 = new HyperLogLogPlus(5, 25);
            for (int i = 0; i < 50000; i++)
                hll3.OfferHashed(Hash64("" + i));
            Assert.That(hll3, Is.Not.EqualTo(hll1));
        }

        [Test]
        public void ComputeCount()
        {
            const int count = 70000;
            var hll = new HyperLogLogPlus(14, 25);

            for (int i = 0; i < count; i++)
                hll.OfferHashed(Hash64("i" + i));

            long estimate = hll.Cardinality();
            double se = count*(1.04/Math.Sqrt(Math.Pow(2, 14)));
            const long expectedCardinality = count;

            Console.WriteLine("Expect estimate: {0} is between {1} and {2}", estimate, expectedCardinality - (3*se),
                expectedCardinality + (3*se));

            Assert.That(estimate, Is.GreaterThanOrEqualTo(expectedCardinality - (3*se)));
            Assert.That(estimate, Is.LessThanOrEqualTo(expectedCardinality + (3*se)));
        }

        [Test]
        public void SmallCardinalityRepeatedInsert()
        {
            var hll = new HyperLogLogPlus(14, 25);
            const int count = 15000;
            const int maxAttempts = 200;

            for (var i = 0; i < count; i++)
            {
                var n = Rnd.Next(maxAttempts) + 1;
                for (var j = 0; j < n; j++)
                {
                    hll.OfferHashed(Hash64("i" + i));
                }
            }
            long estimate = hll.Cardinality();
            double se = count*(1.04/Math.Sqrt(Math.Pow(2, 14)));
            const long expectedCardinality = count;

            Console.WriteLine("Expect estimate: {0} is between {1} and {2}", estimate, expectedCardinality - (3*se),
                expectedCardinality + (3*se));

            Assert.That(estimate, Is.GreaterThanOrEqualTo(expectedCardinality - (3*se)));
            Assert.That(estimate, Is.LessThanOrEqualTo(expectedCardinality + (3*se)));
        }

        [Test]
        public void HighCardinality()
        {
            var sw = Stopwatch.StartNew();

            var hll = new HyperLogLogPlus(18, 25);
            const int size = (int)10e6;

            for (int i = 0; i < size; ++i)
            {
                var buf = new byte[8];
                Rnd.NextBytes(buf);
                hll.OfferHashed(Hash64(buf));
            }

            Console.WriteLine("expected: {0}, estimate: {1}, time: {2}", size, hll.Cardinality(), sw.Elapsed);
            long estimate = hll.Cardinality();
            double err = Math.Abs(estimate - size)/(double) size;
            Console.WriteLine("Percentage error: " + err);
            Assert.That(err, Is.LessThan(0.1));
        }

        [Test]
        public void Serialization_Normal()
        {
            var hll = new HyperLogLogPlus(5, 25);
            for (var i = 0; i < 100000; ++i)
                hll.OfferHashed(Hash64(i));
            Console.WriteLine(hll.Cardinality());

            var hll2 = HyperLogLogPlus.FromBytes(hll.ToBytes());
            Assert.That(hll2.Cardinality(), Is.EqualTo(hll.Cardinality()));
        }

        [Test]
        public void Serialization_Sparse()
        {
            var hll = new HyperLogLogPlus(14, 25);
            hll.OfferHashed(Hash64("a"));
            hll.OfferHashed(Hash64("b"));
            hll.OfferHashed(Hash64("c"));
            hll.OfferHashed(Hash64("d"));
            hll.OfferHashed(Hash64("e"));

            var hll2 = HyperLogLogPlus.FromBytes(hll.ToBytes());
            Assert.That(hll2.Cardinality(), Is.EqualTo(hll.Cardinality()));
        }

        [Test]
        public void SortEncodedSet()
        {
            //var testSet = new uint[]
            //{
            //    655403,
            //    655416,
            //    655425
            //};

            var testSet = new TempSet(3);

            testSet[0] = 655403;
            testSet[1] = 655416;
            testSet[2] = 655425;

            var sorted = HyperLogLogPlus.SortEncodedSet(testSet, 3);
            Assert.That(sorted._M[0], Is.EqualTo(new[] {655403, 655425, 655416}));
        }

        [Test]
        public void MergeSelf_ForceNormal()
        {
            int[] cardinalities = {0, 1, 10, 100, 1000, 10000, 100000, 1000000};
            foreach (var cardinality in cardinalities)
            {
                for (uint j = 4; j < 24; ++j)
                {
                    Console.WriteLine("p=" + j);
                    var hll = new HyperLogLogPlus(j, 0);
                    for (var l = 0; l < cardinality; l++)
                        hll.OfferHashed(Hash64(Rnd.Next()));

                    Console.WriteLine("hllcardinality={0} cardinality={1}", hll.Cardinality(), cardinality);

                    var deserialized = HyperLogLogPlus.FromBytes(hll.ToBytes());
                    Assert.That(deserialized.Cardinality(), Is.EqualTo(hll.Cardinality()));
                    var merged = hll.Merge(deserialized);
                    Console.WriteLine(merged.Cardinality() + " : " + hll.Cardinality());
                    Assert.That(merged.Cardinality(), Is.EqualTo(hll.Cardinality()));
                }
            }
        }

        [Test]
        public void MergeSelf()
        {
            int[] cardinalities = {0, 1, 10, 100, 1000, 10000, 100000};
            uint[] ps = {4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20};
            uint[] sps = {16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32};

            foreach (int cardinality in cardinalities)
            {
                for (int j = 0; j < ps.Length; j++)
                {
                    foreach (var sp in sps)
                    {
                        if (sp < ps[j])
                            continue;
                        var hll = new HyperLogLogPlus(ps[j], sp);
                        for (int l = 0; l < cardinality; l++)
                            hll.OfferHashed(Hash64(Rnd.Next()));

                        var deserialized = HyperLogLogPlus.FromBytes(hll.ToBytes());
                        Console.WriteLine(ps[j] + "-" + sp + ": " + cardinality + " -> " + hll.Cardinality());
                        Assert.That(deserialized.Cardinality(), Is.EqualTo(hll.Cardinality()));
                        var merged = hll.Merge(deserialized);
                        Assert.That(merged.Cardinality(), Is.EqualTo(hll.Cardinality()));
                    }
                }
            }
        }

        [Test]
        public void One()
        {
            var one = new HyperLogLogPlus(8, 25);
            one.OfferHashed(Hash64("a"));
            Assert.That(one.Cardinality(), Is.EqualTo(1));
        }

        [Test]
        public void SparseSpace()
        {
            var hllp = new HyperLogLogPlus(14, 14);
            for (int i = 0; i < 10000; ++i)
                hllp.OfferHashed(Hash64(i));
            Console.WriteLine("Size: {0}", hllp.ToBytes().Length);
        }

        [Test]
        public void Merge_Sparse()
        {
            const int numToMerge = 4;
            const int bits = 18;
            const int cardinality = 4000;

            var hlls = new HyperLogLogPlus[numToMerge];
            var baseline = new HyperLogLogPlus(bits, 25);
            for (var i = 0; i < numToMerge; ++i)
            {
                hlls[i] = new HyperLogLogPlus(bits, 25);
                for (int j = 0; j < cardinality; ++j)
                {
                    double val = Rnd.NextDouble();
                    hlls[i].OfferHashed(Hash64(val));
                    baseline.OfferHashed(Hash64(val));
                }
            }

            const long expectedCardinality = numToMerge * cardinality;
            HyperLogLogPlus hll = hlls[0];
            hlls = hlls.Skip(1).ToArray();
            long mergedEstimate = hll.Merge(hlls).Cardinality();
            double se = expectedCardinality * (1.04 / Math.Sqrt(Math.Pow(2, bits)));

            Console.WriteLine("Expect estimate: {0} is between {1} and {2}", mergedEstimate, expectedCardinality - (3 * se), expectedCardinality + (3 * se));
            double err = Math.Abs(mergedEstimate - expectedCardinality) / (double) expectedCardinality;
            Console.WriteLine("Percentage error " + err);
            Assert.That(err, Is.LessThan(0.1));
            Assert.That(mergedEstimate, Is.InRange(expectedCardinality - (3 * se), expectedCardinality + (3 * se)));
        }

        [Test]
        public void Merge_Normal()
        {
            const int numToMerge = 4;
            const int bits = 18;
            const int cardinality = 5000;

            var hlls = new HyperLogLogPlus[numToMerge];
            var baseline = new HyperLogLogPlus(bits, 25);
            for (int i = 0; i < numToMerge; i++)
            {
                hlls[i] = new HyperLogLogPlus(bits, 25);
                for (int j = 0; j < cardinality; j++)
                {
                    double val = Rnd.NextDouble();
                    hlls[i].OfferHashed(Hash64(val));
                    baseline.OfferHashed(Hash64(val));
                }
            }

            const long expectedCardinality = numToMerge * cardinality;
            var hll = hlls[0];
            hlls = hlls.Skip(1).ToArray();
            long mergedEstimate = hll.Merge(hlls).Cardinality();
            double se = expectedCardinality * (1.04 / Math.Sqrt(Math.Pow(2, bits)));

            Console.WriteLine("Expect estimate: {0} is between {1} and {2}", mergedEstimate, expectedCardinality - (3 * se), expectedCardinality + (3 * se));
            Assert.That(mergedEstimate, Is.InRange(expectedCardinality - (3 * se), expectedCardinality + (3 * se)));
        }

        [Test]
        public void Merge_ManySparse()
        {
            const int numToMerge = 20;
            const int bits = 18;
            const int cardinality = 10000;

            var hlls = new HyperLogLogPlus[numToMerge];
            var baseline = new HyperLogLogPlus(bits, 25);
            for (int i = 0; i < numToMerge; i++)
            {
                hlls[i] = new HyperLogLogPlus(bits, 25);
                for (int j = 0; j < cardinality; j++)
                {
                    double val = Rnd.NextDouble();
                    hlls[i].OfferHashed(Hash64(val));
                    baseline.OfferHashed(Hash64(val));
                }
            }

            const long expectedCardinality = numToMerge * cardinality;
            var hll = hlls[0];
            hlls = hlls.Skip(1).ToArray();
            long mergedEstimate = hll.Merge(hlls).Cardinality();
            double se = expectedCardinality * (1.04 / Math.Sqrt(Math.Pow(2, bits)));

            Console.WriteLine("Expect estimate: {0} is between {1} and {2}", mergedEstimate, expectedCardinality - (3 * se), expectedCardinality + (3 * se));
            Assert.That(mergedEstimate, Is.InRange(expectedCardinality - (3 * se), expectedCardinality + (3 * se)));
        }

        [Test]
        public void Merge_SparseIntersection()
        {
            var a = new HyperLogLogPlus(11, 16);
            var b = new HyperLogLogPlus(11, 16);

            // Note that only one element, 41, is shared amongst the two sets,
            // and so the number of total unique elements is 14.
            int[] aInput = {12, 13, 22, 34, 38, 40, 41, 46, 49};
            int[] bInput = {2, 6, 19, 29, 41, 48};

            var testSet = new HashSet<int>();
            foreach (int i in aInput)
            {
                testSet.Add(i);
                a.OfferHashed(Hash64(i));
            }

            foreach (int i in bInput)
            {
                testSet.Add(i);
                b.OfferHashed(Hash64(i));
            }

            Assert.That(testSet.Count, Is.EqualTo(14));
            Assert.That(a.Cardinality(), Is.EqualTo(9));
            Assert.That(b.Cardinality(), Is.EqualTo(6));

            a.AddAll(b);
            Assert.That(a.Cardinality(), Is.EqualTo(14));
        }
    }
}
