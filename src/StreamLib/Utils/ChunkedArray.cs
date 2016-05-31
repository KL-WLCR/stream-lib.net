using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StreamLib.Utils
{
    /// ChunkedArray is wrapper over jagged array. Used to avoid allocation in LOH large arrays.
    public class ChunkedArray<T> : IEnumerable<T>
        where T : IComparable
    {
        int _maxWidth;
        int _rows;
        int _lastArraySize;
        int _length;

        public T[][] _buffer;

        public ChunkedArray(int size, int chunkSize = 8000)
        {
            _length = size;
            _maxWidth = chunkSize;
            _rows = (size / _maxWidth) + 1;
            _lastArraySize = size - (_rows - 1) * _maxWidth;
            _buffer = new T[_rows][];

            for (var i = 0; i < _rows - 1; ++i)
            {
                _buffer[i] = new T[_maxWidth];
            }

            _buffer[_rows - 1] = new T[_lastArraySize];
        }

        public IEnumerator<T> GetEnumerator()
        {
            foreach (T[] t1 in _buffer)
            {
                if (t1 == null) break;

                foreach (T t in t1)
                {
                    if (t == null) break;

                    yield return t;
                }
            }
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
            _length = size;
            _rows = (size / _maxWidth) + 1;
            _lastArraySize = size - (_rows - 1) * _maxWidth;
        }

        public void CopyFrom(ChunkedArray<T> source, int validIndex)
        {
            SetSize(validIndex);

            var i = 0;

            for (; i < _rows-1; ++i)
            {
                Array.Copy(source._buffer[i], _buffer[i], _maxWidth );
            }

            Array.Copy(source._buffer[i], _buffer[i], _lastArraySize);
        }

        public ChunkedArray<T> Sort(IComparer<T> comparer)
        {
            // Sort chunks
            var i = 0;

            for (; i < _rows - 1; ++i)
            {
                Array.Sort(_buffer[i], comparer);
            }

            Array.Sort(_buffer[i], 0, _lastArraySize, comparer);

            // Merge join sorted chunks
            var result = new ChunkedArray<T>(_length);
            var positions = new int[_rows];
            var start_t = 0;

            for (var j = 0; j < _length; ++j)
            {
                while (positions[start_t] >= _maxWidth)
                {
                    ++start_t;
                    if (start_t == _rows)
                        break;
                }

                T minValue = _buffer[start_t][positions[start_t]];
                int minPosition = 0;

                for (var t = start_t; t < _rows; ++t)
                {
                    if (t == (_rows - 1) && positions[t] >= _lastArraySize)
                        continue;

                    if (positions[t] == _maxWidth)
                        continue;

                    if (comparer.Compare(_buffer[t][positions[t]], minValue) < 0)
                    {
                        minValue = _buffer[t][positions[t]];
                        minPosition = t;
                    }
                }

                ++positions[minPosition];
                result[j] = minValue;
            }

            return result;
        }

        public bool SequenceEqual (ChunkedArray<T> other)
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
                if (_buffer[i][t].CompareTo( other._buffer[i][t] ) != 0 )
                    return false;
            }

            return result;
        }

        public T this[uint i]
        {
            get
            {
                int r = (int)i / _maxWidth;
                int c = (int)i - r * _maxWidth;
                return _buffer[r][c];
            }
            set
            {
                int r = (int)i / _maxWidth;
                int c = (int)i - r * _maxWidth;
                _buffer[r][c] = value;
            }
        }

        public T this[int i]
        {
            get
            {
                int r = i / _maxWidth;
                int c = i - r * _maxWidth;
                return _buffer[r][c];
            }
            set
            {
                int r = i / _maxWidth;
                int c = i - r * _maxWidth;
                _buffer[r][c] = value;
            }
        }

    }
}
