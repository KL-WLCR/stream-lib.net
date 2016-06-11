using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace StreamLib.Utils
{
    /// ChunkedArray is wrapper over jagged array. Used to avoid allocation in LOH large arrays.
    public class ChunkedArray<T> : IEnumerable<T>, IDisposable
        where T : IComparable
    {
        public const int _maxWidth = 8192;
        const int _columnStartBit = 13;
        const int _rowPositionBitMask = 0x1FFF;

        int _rows;
        int _lastArraySize;
        int _allocatedRows;
        int _sizeOfLastAllocatedRow;
        int _length;
        int _capacity;
        bool _isPossibleResize;

        private T[][] _buffer;
        private int[] _poolBufferId;

        ChunkPool<T> _pool;

        public ChunkedArray(int size, ChunkPool<T> pool = null, bool isCleanupRented = false)
        {
            _pool = pool;
            _capacity = size;
            _length = size;
            _allocatedRows = 0;

            if (size > 0)
            {
                _isPossibleResize = false;
                _rows = (size / _maxWidth) + 1;
                _lastArraySize = size - (_rows - 1) * _maxWidth;
                if (_lastArraySize == 0)
                {
                    --_rows;
                    _lastArraySize = _maxWidth;
                }

                _buffer = new T[_rows][];
                _poolBufferId = new int[_rows];
            }
            else
            {
                _isPossibleResize = true;
                _rows = 0;
                _lastArraySize = 0;
            }

            if (_rows > 0)
            {
                if (pool != null)
                {
                    for (var i = 0; i < _rows; ++i)
                    {
                        var bufferId = 0;
                        _buffer[i] = pool.Rent(out bufferId);
                        _poolBufferId[i] = bufferId;

                        if (isCleanupRented)
                        { 

                            FastArrayClear(_buffer[i], default(T), i == _rows -1 ? _lastArraySize : _maxWidth);
                        }

                        ++_allocatedRows;
                    }
                    _sizeOfLastAllocatedRow = _lastArraySize;
                }
                else
                {
                    for (var i = 0; i < _rows - 1; ++i)
                    {
                        _buffer[i] = new T[_maxWidth];
                    }

                    _buffer[_rows - 1] = new T[_lastArraySize];
                }
            }
        }

        private void FastArrayClear(T[] array, T value, int freeSize)
        {
            int block = 32, index = 0;
            int length = Math.Min(block, freeSize);

            while (index < length)
            {
                array[index++] = value;
            }

            while (index < freeSize)
            {
                int actualBlockSize = Math.Min(block, freeSize - index);
                Buffer.BlockCopy(array, 0, array, index << 2, actualBlockSize << 2);
                index += block;
                block <<= 1;
            }
        }


        public T[] GetChunk(int position)
        {
            return _buffer[position];
        }

        public T[] GetPart(int from, int length)
        {
            var result = new T[length];

            for (var i = 0; i < length; ++i)
            {
                result[i] = this[from + i];
            }

            return result;
        }

        public void AddChunk()
        {
            if (!_isPossibleResize)
                throw new Exception("Resize not possible");

            _capacity += _maxWidth;
            _rows += 1;
            _length += _maxWidth;
            _lastArraySize = _maxWidth;

            Array.Resize<T[]>(ref _buffer, _rows);

            if (_pool != null)
            {
                var bufferId = 0;
                _buffer[_rows - 1] = _pool.Rent(out bufferId);
                _poolBufferId[_rows - 1] = bufferId;

                ++_allocatedRows;
                _sizeOfLastAllocatedRow = _maxWidth;

            }
            else
            { 
                _buffer[_rows - 1] = new T[_maxWidth];
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            var j = 0;
            var i = 0;

            for (; j < _rows - 1; j++)
            {
                for (i = 0; i < _maxWidth; i++)
                    yield return _buffer[j][i];

            }

            for (i = 0; i < _lastArraySize; i++)
                yield return _buffer[j][i];
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            // Lets call the generic version here
            return this.GetEnumerator();
        }

        public int Length
        {
            get { return _length; }
        }

        public void SetSize(int size)
        {
            if (size > _capacity)
                throw new Exception("New size bigger then capacity");

            _length = size;
            _rows = (size / _maxWidth) + 1;
            _lastArraySize = size - (_rows - 1) * _maxWidth;
            if (_lastArraySize == 0)
            {
                --_rows;
                _lastArraySize = _maxWidth;
            }
        }

        public void ResetToSize(int size, T initialValue)
        {
            if (size > _capacity)
                throw new Exception("New size bigger then capacity");

            SetSize(size);

            var j = 0;
            var i = 0;

            for (; j < _rows - 1; j++)
            {
                for (i = 0; i < _maxWidth; i++)
                    _buffer[j][i] = initialValue;
            }

            for (i = 0; i < _lastArraySize; i++)
                _buffer[j][i] = initialValue;
        }

        // TODO : increase speed
        public static ChunkedArray<T> CreateFromArray(T[] source)
        {
            ChunkedArray<T> result = new ChunkedArray<T>(source.Length);

            var i = 0;
            foreach ( var t in source )
            {
                result[i++] = t;
            }

            return result;
        }

        public void CopyFrom(ChunkedArray<T> source, int validIndex)
        {
            SetSize(validIndex);

            var i = 0;

            for (; i < _rows - 1; ++i)
            {
                Array.Copy(source._buffer[i], _buffer[i], _maxWidth);
            }

            Array.Copy(source._buffer[i], _buffer[i], _lastArraySize);
        }

        public bool SequenceEqual(ChunkedArray<T> other)
        {
            bool result = true;

            if (_length != other._length)
                return false;

            var i = 0;

            for (; i < _rows - 1; ++i)
            {
                result = _buffer[i].SequenceEqual(other._buffer[i]);

                if (!result)
                    return result;
            }

            for (var t = 0; t < _lastArraySize; ++t)
            {
                if (_buffer[i][t].CompareTo(other._buffer[i][t]) != 0)
                    return false;
            }

            return result;
        }

        public T this[uint i]
        {
            get { return this[(int)i]; }
            set { this[(int)i] = value; }
        }

        public T this[int i]
        {
            get
            {
                return _buffer[i >> _columnStartBit][_rowPositionBitMask & i];
            }
            set
            {
                _buffer[i >> _columnStartBit][_rowPositionBitMask & i] = value;
            }
        }

        public void Dispose()
        {
            if (_pool != null)
            {
                for (var i = _allocatedRows - 1; i >=0 ; --i)
                {
                    var size = (i == _allocatedRows - 1) ? _sizeOfLastAllocatedRow : _maxWidth;

                    _pool.Free(_poolBufferId[i], size );
                }
            }
        }

        // Sorting implementation 

        internal static class IntrospectiveSortUtilities
        {
            // This is the threshold where Introspective sort switches to Insertion sort.
            // Imperically, 16 seems to speed up most cases without slowing down others, at least for integers.
            // Large value types may benefit from a smaller number.
            internal const int IntrosortSizeThreshold = 16;

            internal const int QuickSortDepthThreshold = 32;

            internal static int FloorLog2(int n)
            {
                int result = 0;
                while (n >= 1)
                {
                    result++;
                    n = n / 2;
                }
                return result;
            }

        }

        public static void Sort(ChunkedArray<T> keys, int index, int length, IComparer<T> comparer)
        {
            IntrospectiveSort(keys, index, length, comparer);
        }

        private static void SwapIfGreater(ChunkedArray<T> keys, IComparer<T> comparer, int a, int b)
        {
            if (a != b)
            {
                if (comparer.Compare(keys[a], keys[b]) > 0)
                {
                    T key = keys[a];
                    keys[a] = keys[b];
                    keys[b] = key;
                }
            }
        }

        private static void Swap(ChunkedArray<T> a, int i, int j)
        {
            if (i != j)
            {
                T t = a[i];
                a[i] = a[j];
                a[j] = t;
            }
        }

        internal static void IntrospectiveSort(ChunkedArray<T> keys, int left, int length, IComparer<T> comparer)
        {

            if (length < 2)
                return;

            IntroSort(keys, left, length + left - 1, 2 * IntrospectiveSortUtilities.FloorLog2(keys.Length), comparer);
        }

        private static void IntroSort(ChunkedArray<T> keys, int lo, int hi, int depthLimit, IComparer<T> comparer)
        {
            while (hi > lo)
            {
                int partitionSize = hi - lo + 1;
                if (partitionSize <= IntrospectiveSortUtilities.IntrosortSizeThreshold)
                {
                    if (partitionSize == 1)
                    {
                        return;
                    }
                    if (partitionSize == 2)
                    {
                        SwapIfGreater(keys, comparer, lo, hi);
                        return;
                    }
                    if (partitionSize == 3)
                    {
                        SwapIfGreater(keys, comparer, lo, hi - 1);
                        SwapIfGreater(keys, comparer, lo, hi);
                        SwapIfGreater(keys, comparer, hi - 1, hi);
                        return;
                    }

                    InsertionSort(keys, lo, hi, comparer);
                    return;
                }

                if (depthLimit == 0)
                {
                    Heapsort(keys, lo, hi, comparer);
                    return;
                }
                depthLimit--;

                int p = PickPivotAndPartition(keys, lo, hi, comparer);
                // Note we've already partitioned around the pivot and do not have to move the pivot again.
                IntroSort(keys, p + 1, hi, depthLimit, comparer);
                hi = p - 1;
            }
        }

        private static int PickPivotAndPartition(ChunkedArray<T> keys, int lo, int hi, IComparer<T> comparer)
        {
            // Compute median-of-three.  But also partition them, since we've done the comparison.
            int middle = lo + ((hi - lo) / 2);

            // Sort lo, mid and hi appropriately, then pick mid as the pivot.
            SwapIfGreater(keys, comparer, lo, middle);  // swap the low with the mid point
            SwapIfGreater(keys, comparer, lo, hi);   // swap the low with the high
            SwapIfGreater(keys, comparer, middle, hi); // swap the middle with the high

            T pivot = keys[middle];
            Swap(keys, middle, hi - 1);
            int left = lo, right = hi - 1;  // We already partitioned lo and hi and put the pivot in hi - 1.  And we pre-increment & decrement below.

            while (left < right)
            {
                while (comparer.Compare(keys[++left], pivot) < 0) ;
                while (comparer.Compare(pivot, keys[--right]) < 0) ;

                if (left >= right)
                    break;

                Swap(keys, left, right);
            }

            // Put pivot in the right location.
            Swap(keys, left, (hi - 1));
            return left;
        }

        private static void Heapsort(ChunkedArray<T> keys, int lo, int hi, IComparer<T> comparer)
        {
            int n = hi - lo + 1;
            for (int i = n / 2; i >= 1; i = i - 1)
            {
                DownHeap(keys, i, n, lo, comparer);
            }
            for (int i = n; i > 1; i = i - 1)
            {
                Swap(keys, lo, lo + i - 1);
                DownHeap(keys, 1, i - 1, lo, comparer);
            }
        }

        private static void DownHeap(ChunkedArray<T> keys, int i, int n, int lo, IComparer<T> comparer)
        {
            T d = keys[lo + i - 1];
            int child;
            while (i <= n / 2)
            {
                child = 2 * i;
                if (child < n && comparer.Compare(keys[lo + child - 1], keys[lo + child]) < 0)
                {
                    child++;
                }
                if (!(comparer.Compare(d, keys[lo + child - 1]) < 0))
                    break;
                keys[lo + i - 1] = keys[lo + child - 1];
                i = child;
            }
            keys[lo + i - 1] = d;
        }

        private static void InsertionSort(ChunkedArray<T> keys, int lo, int hi, IComparer<T> comparer)
        {
            int i, j;
            T t;
            for (i = lo; i < hi; i++)
            {
                j = i;
                t = keys[i + 1];
                while (j >= lo && comparer.Compare(t, keys[j]) < 0)
                {
                    keys[j + 1] = keys[j];
                    j--;
                }
                keys[j + 1] = t;
            }
        }

    }
}
