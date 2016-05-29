using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StreamLib.Cardinality
{
    public class TempSet
    {
        int _maxWidth = 8000;

        public int Length;
        int _rows;
        int _lastArraySize;

        public uint[][] _M;

        public TempSet(int size)
        {
            Length = size;
            _rows = (size / _maxWidth) + 1;
            _lastArraySize = size - (_rows - 1) * _maxWidth;
            _M = new uint[_rows][];

            for (var i = 0; i < _rows - 1; ++i)
            {
                _M[i] = new uint[_maxWidth];
            }

            _M[_rows - 1] = new uint[_lastArraySize];
        }

        public void SetSize(int size)
        {
            Length = size;
            _rows = (size / _maxWidth) + 1;
            _lastArraySize = size - (_rows - 1) * _maxWidth;
        }

        public void CopyFrom (TempSet source, int validIndex)
        {
            SetSize(validIndex);

            var i = 0;

            for (; i < _rows-1; ++i)
            {
                Array.Copy(source._M[i], _M[i], _maxWidth );
            }

            Array.Copy(source._M[i], _M[i], _lastArraySize);
        }

        public TempSet SortTempSet(IComparer<uint> comparer)
        {
            var i = 0;

            for (; i < _rows - 1; ++i)
            {
                Array.Sort(_M[i], comparer);
            }

            Array.Sort(_M[i], 0, _lastArraySize, comparer);

            var _Tmp = new TempSet(Length);
            var positions = new int[_rows];

            var start_t = 0;

            for (var j = 0; j < Length; ++j)
            {
                while (positions[start_t] >= _maxWidth)
                {
                    start_t++;
                    if (start_t == _rows)
                        break;
                }

                uint minValue = _M[start_t][positions[start_t]];
                int minPosition = 0;

                for (var t = start_t; t < _rows; ++t)
                {
                    if (t == (_rows - 1) && positions[t] >= _lastArraySize)
                        continue;

                    if (positions[t] == _maxWidth)
                        continue;


                    if (comparer.Compare(_M[t][positions[t]], minValue) < 0)
                    {
                        minValue = _M[t][positions[t]];
                        minPosition = t;
                    }
                }

                positions[minPosition]++;
                _Tmp[j] = minValue;
            }

            return _Tmp;
        }

        public bool SequenceEqual (TempSet other)
        {
            if (Length != other.Length)
                return false;

            bool result = true;

            var i = 0;

            for (; i < _rows - 1; ++i)
            {
                result = _M[i].SequenceEqual(other._M[i]);

                if (!result)
                    return result;
            }

            for (var t = 0; t < _lastArraySize; ++t)
            {

                if (_M[i][t] != _M[i][t])
                    return false;
            }

            return result;
        }

        public uint this[uint i]
        {
            get
            {
                int r = (int)i / _maxWidth;
                int c = (int)i - r * _maxWidth;
                return _M[r][c];
            }
            set
            {
                int r = (int)i / _maxWidth;
                int c = (int)i - r * _maxWidth;
                _M[r][c] = value;
            }
        }

        public uint this[int i]
        {
            get
            {
                int r = i / _maxWidth;
                int c = i - r * _maxWidth;
                return _M[r][c];
            }
            set
            {
                int r = i / _maxWidth;
                int c = i - r * _maxWidth;
                _M[r][c] = value;
            }
        }

    }
}
