using System.IO;
using System.Linq;
using FsCheck;
using NUnit.Framework;
using StreamLib.Utils;
using StreamLib.Cardinality;

using ChunkedArray = StreamLib.Utils.ChunkedArray<uint>;
using System.Collections.Generic;

namespace StreamLib.Tests.Utils
{
    [TestFixture]
    public class ChunkedArrayTests
    {
        [Test]
        public void ChunkArrayEnumeration()
        {
            var tst = new ChunkedArray(9, 5);

            uint i = 0;

            for (i = 0; i < 9; ++ i)
            {
                tst[i] = i;
            }

            i = 0;
            foreach (var t in tst)
            {
                Assert.That(t, Is.EqualTo (i));

                ++ i;
            }

            i = 0;
            foreach (var t in tst)
            {
                Assert.That(t, Is.EqualTo(i));

                ++i;
            }
        }

        class Comparer : IComparer<uint>
        {
            public int Compare(uint left, uint right)
            {
                if (left == right) return 0;
                if (left < right) return -1;
                if (right < left) return 1;
                return 0;
            }
        }

        static readonly IComparer<uint> comparer = new Comparer();

        [Test]
        public void ChunkArraySorting()
        {
            var tst = new ChunkedArray(25, 5);
            var tst_sorted = new ChunkedArray(25, 5);

            uint i = 0;

            for (i = 0; i < 25; ++i)
            {
                tst[i] = 24 - i;
            }

            tst_sorted = tst.Sort(comparer);

            i = 0;
            foreach (var t in tst_sorted)
            {
                Assert.That(t, Is.EqualTo(i));

                ++i;
            }

        }

        [Test]
        public void ChunkArraySortingEqualsElements()
        {
            var tst = new ChunkedArray(25, 5);
            var tst_sorted = new ChunkedArray(25, 5);

            uint i = 0;

            for (i = 0; i < 25; ++i)
            {
                tst[i] = 0;
            }

            tst_sorted = tst.Sort(comparer);

            i = 0;
            foreach (var t in tst_sorted)
            {
                Assert.That(t, Is.EqualTo(i));
            }

        }

    }
}

