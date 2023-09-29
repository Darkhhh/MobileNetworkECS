using System.Runtime.CompilerServices;

namespace MobileNetworkECS.Core.Utils;

public class SparseSet
{
    private int[] _sparse; // Uses actual elements as index and stores indexes of dense[]
    private int[] _dense; // Stores actual elements
    private int _n; // Current number of elements in set
    private int _capacity; // Initial capacity, size of dense
    private int _maxValue; // Size of sparse
 
    public SparseSet(int maxValue, int capacity)
    {
        _sparse = new int[maxValue + 1];
        _dense = new int[capacity];
        _capacity = capacity;
        _maxValue = maxValue;
        _n = 0;
    }
 
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int Find(int x)
    {
        if (x > _maxValue) return -1;
        if (_sparse[x] < _n && _dense[_sparse[x]] == x) return _sparse[x];
 
        return -1;
    }
 
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Insert(int x)
    {
        if (_n == _capacity) return;
        if (x > _maxValue) return;
 
        var i = _sparse[x];
        if (i >= _n || _dense[i] != x)
        {
            _dense[_n] = x;
            _sparse[x] = _n;
            _n++;
        }
    }
 
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Delete(int x)
    {
        if (x > _maxValue) return;
 
        var i = _sparse[x];
        if (i < _n && _dense[i] == x)
        {
            _n--;
            _dense[i] = _dense[_n];
            _sparse[_dense[_n]] = i;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public SparseSet AllocateSparse(int newMaxValue)
    {
        if (newMaxValue <= _maxValue) return this;
        Array.Resize(ref _sparse, newMaxValue);
        _maxValue = newMaxValue;
        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public SparseSet AllocateDense(int newCapacity)
    {
        if (newCapacity <= _capacity) return this;
        Array.Resize(ref _dense, newCapacity);
        _capacity = newCapacity;
        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int Count() => _n;
}