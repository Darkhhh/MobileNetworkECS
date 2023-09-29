namespace MobileNetworkECS.Core;

public interface IEcsPool
{
    private static int _id;
    public static int CreateId() => _id++;

    public int GetId();

    public bool Has(int entity);

    public void Remove(int entity);

    internal Action<Type, int, bool> PoolChanged { get; init; }
}


public class EcsPool<T> : IEcsPool where T: struct
{
    /// <summary>
    /// Type is component of a pool. Int is entity id. Bool is added or removed (true - added, false - removed). 
    /// </summary>
    public Action<Type, int, bool> PoolChanged { get; init; }
    
    private readonly Type _componentType;
    private Component[] _components = new Component[256];
    private int _size;
    private readonly int _id;
    
    
    internal EcsPool(Type componentType, Action<Type, int, bool> poolChanged)
    {
        _componentType = componentType;
        PoolChanged = poolChanged;
        _id = IEcsPool.CreateId();
    }

    public EcsPool(Type componentType)
    {
        _componentType = componentType;
        PoolChanged = (_, _, _) => { };
        _id = IEcsPool.CreateId();
    }

    public int GetId() => _id;

    public ref T Add(int entity)
    {
        for (var i = 0; i < _components.Length; i++)
        {
            if (_components[i].Exists && _components[i].Entity == entity)
                throw new Exception("Entity already in pool");
        }
        
        for (var i = 0; i < _components.Length; i++)
        {
            if (_components[i].Exists) continue;
            
            _components[i].Exists = true;
            _components[i].Entity = entity;
            PoolChanged.Invoke(_componentType, entity, true);
            return ref _components[i].Value;
        }
        
        if (_size + 1 >= _components.Length)
        {
            Array.Resize(ref _components, _components.Length * 2);
        }

        _components[_size] = new Component
        {
            Exists = true,
            Entity = entity,
            Value = default
        };
            
        _size++;
        PoolChanged.Invoke(_componentType, entity, true);
        return ref _components[_size - 1].Value;
    }

    public bool Has(int entity)
    {
        return _components.Any(component => component.Entity == entity && component.Exists);
    }
    
    public ref T Get(int entity)
    {
        for (var i = 0; i < _components.Length; i++)
        {
            if (!_components[i].Exists) continue;
            if (_components[i].Entity != entity) continue;
            return ref _components[i].Value;
        }
        throw new Exception("Trying to access not created component");
    }

    public void Remove(int entity)
    {
        for (var i = 0; i < _components.Length; i++)
        {
            if (_components[i].Entity != entity) continue;
            _components[i].Exists = false;
            _components[i].Entity = int.MinValue;
            _components[i].Value = default;
            PoolChanged.Invoke(_componentType, entity, false);
        }
    }

    private struct Component
    {
        public bool Exists;
        public int Entity;
        public T Value;
    }
}