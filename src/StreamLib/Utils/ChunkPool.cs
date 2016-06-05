﻿using System;
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
        int _growSize;

        public ChunkPool ( int arraySize, int growSize )
        {
            _refs = new T[100][];
            _growSize = growSize;
            _arraySize = arraySize;
            _poolSize = 0;
            _requiredPoolSize = 0;
            _lastFree = 0;
        }

        public void EnlargePool()
        {
            _requiredPoolSize += _growSize;

            if (_poolSize < _requiredPoolSize)
            {
                Array.Resize<T[]>(ref _refs, _requiredPoolSize);

                for ( var i = _poolSize; i < _requiredPoolSize; ++i)
                {
                    _refs[i] = new T[_arraySize];
                }

                _poolSize = _requiredPoolSize;
            }

        }

        public void ShrikPool()
        {
            _requiredPoolSize -= _growSize;
        }

        public T[] Rent()
        {
            _lastFree++;
            if (_poolSize == _lastFree) _lastFree = 0;
            return _refs[_lastFree];
        }

        public void Free(T[] array, int freeSize)
        {
            Array.Clear(array, 0, freeSize);
        }

    }
}
