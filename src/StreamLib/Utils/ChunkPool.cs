using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StreamLib.Utils
{
    public class ChunkPool<T>
    {
        T[][] _refs;

        int _arraySize;
        int _poolSize;
        int _requiredPoolSize;
        int _lastFree;
        bool[] _slots;
        Dictionary<T[], int> _index;

        public ChunkPool ( int arraySize )
        {
            _refs = new T[100][];
            _arraySize = arraySize;
            _poolSize = 0;
            _requiredPoolSize = 0;
            _lastFree = 0;
            _slots = new bool[100];
            _index = new Dictionary<T[], int>();
        }

        public T[] Rent()
        {
            EnlargePool();

            var _retPos = 0;
            while (_slots[_lastFree])
            {
                ++_lastFree;
                if (_poolSize == _lastFree) _lastFree = 0;
            }

            _slots[_lastFree] = true;
            _retPos = _lastFree;

            _lastFree++;
            if (_poolSize == _lastFree) _lastFree = 0;

            return _refs[_retPos];
        }

        public void Free(T[] array, int freeSize)
        {
            _slots[_index[array]] = false;
            Array.Clear(array, 0, freeSize);
            ShrikPool();
        }

        private void EnlargePool()
        {
            ++_requiredPoolSize;

            if (_poolSize < _requiredPoolSize)
            {
                Array.Resize<T[]>(ref _refs, _requiredPoolSize);
                Array.Resize<bool>(ref _slots, _requiredPoolSize);

                for (var i = _poolSize; i < _requiredPoolSize; ++i)
                {
                    _refs[i] = new T[_arraySize];
                    _slots[i] = false;
                    _index.Add(_refs[i], i);
                }

                _poolSize = _requiredPoolSize;
            }

        }

        private void ShrikPool()
        {
            --_requiredPoolSize;
        }

    }
}
