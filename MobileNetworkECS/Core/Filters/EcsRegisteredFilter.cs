using System.Collections;
using MobileNetworkECS.Core.Utils;
using MobileNetworkECS.Core.Worlds;

namespace MobileNetworkECS.Core.Filters;

public class EcsRegisteredFilter : IEcsRegisteredFilter
{
    public IEcsFilter Filter { get; init; }

    private readonly Type[] _incTypes, _excTypes;
    private uint[]? _incMask;
    private uint[]? _excMask;
    private readonly SparseSet _entitiesSet = new(1024, 1024);
    private int _currentIndex;
    private bool _lock;
    public int Count => _entitiesSet.Count();
    private readonly List<DelayedOperation> _delayedOperations = new();

    public EcsRegisteredFilter(IEcsFilter filter, Type[] incTypes, Type[] excTypes)
    {
        Filter = filter;
        _incTypes = incTypes;
        _excTypes = excTypes;
        _currentIndex = -1;
        _lock = false;
    }

    public Type[] GetIncTypes() => _incTypes;

    public Type[] GetDecTypes() => _excTypes;

    public void SetMasks(uint[] incMask, uint[] excMask) => (_incMask, _excMask) = (incMask, excMask);

    public void CheckEntity(IRawEntity rawEntity, out bool added, out bool removed)
    {
        var poolsAttachment = rawEntity.GetPoolsAttachment();
        var entity = rawEntity.GetId();
#if DEBUG
        if (_incMask is null || _excMask is null) throw new Exception("Null masks in filter");
        if (_incMask.Length != poolsAttachment.Length || _excMask.Length != poolsAttachment.Length)
            throw new Exception("Incorrect size for masks or pool attachment");
#endif
        added = removed = false;
        var contains = _entitiesSet.Find(entity) != -1;
        var poolCorrect = new bool[poolsAttachment.Length];
        Array.Fill(poolCorrect, false);
        
        for (var i = 0; i < poolsAttachment.Length; i++)
        {
            var pool = poolsAttachment[i];
            var inc = _incMask[i];
            var exc = _excMask[i];
            var entityInc = pool & inc;
            var entityDec = ~pool & exc;
            poolCorrect[i] = entityInc == inc && entityDec == exc;
        }

        if (poolCorrect.All(t => t))
        {
            if (contains) return;
            AddEntity(entity);
            added = true;
            return;
        }

        if (!contains) return;
        RemoveEntity(entity);
        removed = true;
    }

    private void AddEntity(int entity)
    {
        switch (_lock)
        {
            case true:
                _delayedOperations.Add(new DelayedOperation { Entity = entity, Add = true });
                break;
            case false:
                if (_entitiesSet.Full()) _entitiesSet.AllocateDense(_entitiesSet.Count() * 2);
                _entitiesSet.Insert(entity);
                Filter.FilterWasUpdated?.Invoke();
                break;
        }
    }

    private void RemoveEntity(int entity)
    {
        switch (_lock)
        {
            case true:
                _delayedOperations.Add(new DelayedOperation { Entity = entity, Add = false });
                break;
            case false:
                _entitiesSet.Delete(entity);
                Filter.FilterWasUpdated?.Invoke();
                break;
        }
    }

    public void UpdateMaxEntityIndex(int amount) => _entitiesSet.AllocateSparse(amount);

    public void UpdatePoolsAmount(int poolsAmount)
    {
#if DEBUG
        if (_incMask is null || _excMask is null) throw new Exception("Null masks in filter");
#endif
        if (_incMask.Length * IEcsWorld.BitSize >= poolsAmount) return;
        
        Array.Resize(ref _incMask, _incMask.Length + 1);
        _incMask[^1] = 0;
        Array.Resize(ref _excMask, _incMask.Length + 1);
        _excMask[^1] = 0;
    }


    #region IEnumerator & IEnumerable Implementation

    public IEnumerator GetEnumerator() => this;

    public bool MoveNext()
    {
        if (_currentIndex < 0) _lock = true;
        return ++_currentIndex < Count;
    }

    public void Reset()
    {
        _currentIndex = -1;
        Unlock();
    }

    private void Unlock()
    {
        _lock = false;
        foreach (var operation in _delayedOperations)
        {
            switch (operation.Add)
            {
                case true:
                    AddEntity(operation.Entity);
                    break;
                case false:
                    RemoveEntity(operation.Entity);
                    break;
            }
        }
        if (_delayedOperations.Count > 0) Filter.FilterWasUpdated?.Invoke();
        _delayedOperations.Clear();
    }

    public object Current => _entitiesSet.GetByIndex(_currentIndex);

    #endregion
    
    private struct DelayedOperation
    {
        public int Entity;
        public bool Add;
    }
}