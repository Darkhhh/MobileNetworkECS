using System.Collections;
using MobileNetworkECS.Core.Worlds;

namespace MobileNetworkECS.Core.Filters;

public class EcsFilter : IEcsFilter
{
    private IEcsWorld _world;
    private IEcsRegisteredFilter? _registeredFilter;
    private readonly List<Type> _incTypes = new(), _excTypes = new();
    
    public Action? FilterWasUpdated { get; set; }
    public int Count
    {
        get
        {
            if (_registeredFilter != null) return _registeredFilter.Count;
            throw new Exception("Filter is not registered");
        }
    }

    public EcsFilter(IEcsWorld world) => _world = world;

    public IEcsFilter Inc<T>() where T : struct
    {
        _world.BindPool<T>();
        var type = typeof(T);
        if (_excTypes.Contains(type)) throw new Exception("Pool already in Exc pools");
        if (_incTypes.Contains(type)) return this;
        _incTypes.Add(type);
        return this;
    }

    public IEcsFilter Exc<T>() where T : struct
    {
        _world.BindPool<T>();
        var type = typeof(T);
        if (_incTypes.Contains(type)) throw new Exception("Pool already in Inc pools");
        if (_excTypes.Contains(type)) return this;
        _excTypes.Add(type);
        return this;
    }

    public IEcsFilter Register(IEcsWorld world)
    {
        _registeredFilter = new EcsRegisteredFilter(this, _incTypes.ToArray(), _excTypes.ToArray());
        world.RegisterFilter(_registeredFilter);
        return this;
    }

    public IEnumerator GetEnumerator()
    {
        if (_registeredFilter is null) throw new Exception("Filter is not registered");
        Reset();
        return _registeredFilter.GetEnumerator();
    }

    public bool MoveNext()
    {
        if (_registeredFilter is null) throw new Exception("Filter is not registered");
        return _registeredFilter.MoveNext();
    }

    public void Reset() => _registeredFilter?.Reset();

    public object Current
    {
        get
        {
            if (_registeredFilter is {Current: not null}) return _registeredFilter.Current;
            throw new Exception("Filter is not registered");
        }
    }
}