using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StreamLib.Utils
{
    public class ChunkPool<T> : IDisposable
    {
        T[][] _refs;

        int _arraySize;
        int _poolSize;
        int _requiredPoolSize;
        int _lastFree;
        int _preallocatedSize;
        const int _internalBuffersGrowSize = 100;
        bool[] _slots;

        public ChunkPool(int arraySize)
        {
            _arraySize = arraySize;
            _poolSize = 0;
            _requiredPoolSize = 0;
            _lastFree = 0;
            _preallocatedSize = _internalBuffersGrowSize;
            _slots = new bool[_preallocatedSize];
            _refs = new T[_preallocatedSize][];
        }

        public T[] Rent(out int bufferId)
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

            bufferId = _retPos;
            return _refs[_retPos];
        }

        public void Free(int bufferId, int freeSize)
        {
            _slots[bufferId] = false;

            _lastFree = bufferId;

            ShrikPool();
        }

        private void EnlargePool()
        {
            ++_requiredPoolSize;

            if (_poolSize < _requiredPoolSize)
            {
                if (_preallocatedSize < _requiredPoolSize)
                {
                    Array.Resize<T[]>(ref _refs, _requiredPoolSize + _internalBuffersGrowSize);
                    Array.Resize<bool>(ref _slots, _requiredPoolSize + _internalBuffersGrowSize);
                    _preallocatedSize = _requiredPoolSize + _internalBuffersGrowSize;
                }

                for (var i = _poolSize; i < _requiredPoolSize; ++i)
                {
                    _refs[i] = new T[_arraySize];
                    _slots[i] = false;
                }

                _poolSize = _requiredPoolSize;
            }

        }

        private void ShrikPool()
        {
            --_requiredPoolSize;
        }


        public void Dispose()
        {

        }

    }
}
