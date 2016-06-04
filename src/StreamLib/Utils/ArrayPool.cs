using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StreamLib.Utils
{
    public class ArrayPool<T>
    {
        bool[] _slots;
        T[][] _refs;
        T _zero;

        public readonly T[] Empty = new T[0];
        int _arraySize;
        int _poolSize;
        

        public ArrayPool ( int arraySize, int poolSize, T zero )
        {
            _zero = zero;
            _slots = new bool[poolSize];
            _refs = new T[poolSize][];
            _arraySize = arraySize;
            _poolSize = poolSize;

            for (var i = 0; i< poolSize; i++ )
            {
                _slots[i] = false;
                _refs[i] = new T[arraySize];
            }
        }

        public T[] Rent()
        {
            for (var i = 0; i < _poolSize; i++)
            {
                if ( !_slots[i] )
                {
                    _slots[i] = true;
                    return _refs[i];
                }
            }

            throw new Exception("Pool is full");

            return new T[_arraySize];
        }

        public void Free(T[] array)
        {
            for (var i = 0; i < _poolSize; i++)
            {
                if (array == _refs[i])
                {
                    _slots[i] = false;
                    
                    for (var j = 0; j < _arraySize; ++j)
                    {
                        _refs[i][j] = _zero;
                    }
                    return;
                }
            }

            array = null;
        }
    }
}
