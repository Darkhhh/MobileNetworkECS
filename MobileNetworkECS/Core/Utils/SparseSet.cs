using System.Runtime.CompilerServices;

namespace MobileNetworkECS.Core.Utils;

public class SparseSet
{
    private int[] _sparse; // Uses actual elements as index and stores indexes of dense[]
    private int[] _dense; // Stores actual elements
    private int _numberOfElements; // Current number of elements in set
    private int _capacity; // Initial capacity, size of dense
    private int _maxValue; // Size of sparse, max value writable to dense
 
    public SparseSet(int maxValue, int capacity)
    {
        _sparse = new int[maxValue + 1];
        _dense = new int[capacity];
        _capacity = capacity;
        _maxValue = maxValue;
        _numberOfElements = 0;
    }
 
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int Find(int item)
    {
        if (item > _maxValue) return -1;
        if (_sparse[item] < _numberOfElements && _dense[_sparse[item]] == item) return _sparse[item];
 
        return -1;
    }
 
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Insert(int item)
    {
        if (_numberOfElements == _capacity || item > _maxValue) return false;
 
        var indexOfDenseItem = _sparse[item];
        if (indexOfDenseItem < _numberOfElements && _dense[indexOfDenseItem] == item) return false;
        
        _dense[_numberOfElements] = item;
        _sparse[item] = _numberOfElements;
        _numberOfElements++;
        return true;
    }
 
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Delete(int item)
    {
        if (item > _maxValue) return;
 
        var indexOfDenseItem = _sparse[item];
        if (indexOfDenseItem >= _numberOfElements || _dense[indexOfDenseItem] != item) return;
        
        _numberOfElements--;
        _dense[indexOfDenseItem] = _dense[_numberOfElements];
        _sparse[_dense[_numberOfElements]] = indexOfDenseItem;
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
    public int Count() => _numberOfElements;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Full() => _numberOfElements == _capacity;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetByIndex(int index) => _dense[index];
}