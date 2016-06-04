using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StreamLib.Utils
{
    public class ArrayPool<T>
    {
        T[][] _refs;

        public readonly T[] Empty = new T[0];
        int _arraySize;
        int _poolSize;
        int _lastFree;
        

        public ArrayPool ( int arraySize, int poolSize )
        {
            _refs = new T[poolSize][];
            _arraySize = arraySize;
            _poolSize = poolSize;
            _lastFree = 0;

            for (var i = 0; i< poolSize; i++ )
            {
                _refs[i] = new T[arraySize];
            }
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
