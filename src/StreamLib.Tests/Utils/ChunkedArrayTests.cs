﻿using System.IO;
using System.Linq;
using FsCheck;
using NUnit.Framework;
using StreamLib.Utils;
using StreamLib.Cardinality;

using ChunkedArray = StreamLib.Utils.ChunkedArray<uint>;
using ArrayPool = StreamLib.Utils.ArrayPool<uint>;
using System.Collections.Generic;

namespace StreamLib.Tests.Utils
{
    [TestFixture]
    public class ChunkedArrayTests
    {
        [Test]
        public void ChunkArrayEnumeration()
        {
            var tst = new ChunkedArray(9);

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

            tst.SetSize(7);
            i = 0;
            foreach (var t in tst)
            {
                Assert.That(t, Is.EqualTo(i));

                ++i;
            }

            Assert.That(i, Is.EqualTo(7));

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
            var tst = new ChunkedArray(17000);

            uint i = 0;

            for (i = 0; i < 17000; ++i)
            {
                tst[i] = 16999 - i;
            }

            ChunkedArray.Sort(tst, 0, 17000, comparer);

            i = 0;
            foreach (var t in tst)
            {
                Assert.That(t, Is.EqualTo(i));

                ++i;
            }

        }

        [Test]
        public void ChunkArraySortingPooling()
        {
            var pool = new ArrayPool(17000, 8);

            var tst = new ChunkedArray(17000, pool);

            uint i = 0;

            for (i = 0; i < 17000; ++i)
            {
                tst[i] = 16999 - i;
            }

            ChunkedArray.Sort(tst, 0, 17000, comparer);

            i = 0;
            foreach (var t in tst)
            {
                Assert.That(t, Is.EqualTo(i));

                ++i;
            }

            tst.Dispose();
        }

    }
}

