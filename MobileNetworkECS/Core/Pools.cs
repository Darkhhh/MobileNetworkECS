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

    private int[] _componentsByEntities = new int[256];
    private int[] _recycledComponentsIndices = new int[256];
    private int _recycledComponentsAmount = 0;
    
    
    internal EcsPool(int id, Type componentType, Action<Type, int, bool> poolChanged)
    {
        _componentType = componentType;
        PoolChanged = poolChanged;
        _id = id;
        Init();
    }

    public EcsPool(int id, Type componentType) : this(id, componentType, (_, _, _) => { }) { }

    private void Init()
    {
        _recycledComponentsAmount = 256;
        for (var i = 0; i < _recycledComponentsAmount; i++) _recycledComponentsIndices[i] = i;
        Array.Fill(_componentsByEntities, -1);
        Array.Fill(_components, new Component{Entity = int.MinValue, Exists = false, Value = default});
    }

    public int GetId() => _id;

    public ref T Add(int entity)
    {
        if (Has(entity)) throw new Exception("Entity already in pool");
        if (_componentsByEntities.Length < entity) Array.Resize(ref _componentsByEntities, entity + 1);
        int componentIndex;
        
        if (_recycledComponentsAmount > 0)
        {
            componentIndex = _recycledComponentsIndices[--_recycledComponentsAmount];
            ref var component = ref _components[componentIndex];
            component.Exists = true;
            component.Entity = entity;
        }
        else
        {
            if (_size + 1 >= _components.Length) Array.Resize(ref _components, _components.Length * 2);
            componentIndex = _size++;
            _components[componentIndex] = new Component { Exists = true, Entity = entity, Value = default };
        }
        
        _componentsByEntities[entity] = componentIndex;
        PoolChanged.Invoke(_componentType, entity, true);
        return ref _components[componentIndex].Value;
    }

    public bool Has(int entity)
    {
        return entity < _componentsByEntities.Length 
               && _componentsByEntities[entity] != -1 
               && _components[_componentsByEntities[entity]].Entity == entity;
    }
    
    public ref T Get(int entity)
    {
        if (!Has(entity)) throw new Exception($"Trying to access not created component with entity id:{entity}");
        return ref _components[_componentsByEntities[entity]].Value;
    }

    public void Remove(int entity)
    {
        if (!Has(entity)) return;
        ref var component = ref _components[_componentsByEntities[entity]];
#if DEBUG
        if (component.Entity != entity) throw new Exception("Incorrect component");
#endif
        component.Exists = false;
        component.Entity = int.MinValue;
        component.Value = default;
        _recycledComponentsAmount++;
        if (_recycledComponentsIndices.Length <= _recycledComponentsAmount + 1)
            Array.Resize(ref _recycledComponentsIndices, _recycledComponentsAmount * 2);
        _recycledComponentsIndices[_recycledComponentsAmount] = _componentsByEntities[entity];
        _componentsByEntities[entity] = -1;
        
        PoolChanged.Invoke(_componentType, entity, false);
    }

    private struct Component
    {
        public bool Exists;
        public int Entity;
        public T Value;
    }
}