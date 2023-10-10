using System.Collections;
using MobileNetworkECS.Core.Worlds;

namespace MobileNetworkECS.Core.Filters;

public class EcsFilter : IEcsFilter
{
    private readonly IEcsWorld _world;
    private IEcsRegisteredFilter? _registeredFilter;
    private readonly List<Type> _incTypes = new(), _excTypes = new();

    private bool _enumerateAsEntity;

    public Action? FilterWasUpdated { get; set; }
    public int Count
    {
        get
        {
            if (_registeredFilter != null) return _registeredFilter.Count;
            throw new Exception("Filter is not registered");
        }
    }

    public EcsFilter(IEcsWorld world) => (_world, _enumerateAsEntity) = (world, false);

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

    public IEcsFilter Register()
    {
        _registeredFilter = new EcsRegisteredFilter(this, _incTypes.ToArray(), _excTypes.ToArray());
        _world.RegisterFilter(_registeredFilter);
        return this;
    }

    public IEcsFilter SetIncTypes(params Type[] types)
    {
        _incTypes.Clear();
        foreach (var type in types)
        {
            if (!_incTypes.Contains(type) && !_excTypes.Contains(type)) _incTypes.Add(type);
            else throw new Exception("Incorrect type input");
        }
        return this;
    }
    public IEcsFilter SetExcTypes(params Type[] types)
    {
        _excTypes.Clear();
        foreach (var type in types)
        {
            if (!_incTypes.Contains(type) && !_excTypes.Contains(type)) _excTypes.Add(type);
            else throw new Exception("Incorrect type input");
        }
        return this;
    }
    
    public IEcsFilter EnumerateAsEntity()
    {
        _enumerateAsEntity = true;
        return this;
    }

    public IEcsFilter EnumerateAsEntityId()
    {
        _enumerateAsEntity = false;
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
            if (_registeredFilter is {Current: not null})
            {
                return _enumerateAsEntity ? 
                    _world.GetEntityById((int)_registeredFilter.Current) : 
                    _registeredFilter.Current;
            }
            throw new Exception("Filter is not registered");
        }
    }
}