using MobileNetworkECS.Core.Entities;
using MobileNetworkECS.Core.Filters;
using MobileNetworkECS.Core.Utils;
using Exception = System.Exception;

namespace MobileNetworkECS.Core.Worlds;

public class EcsWorld : IEcsWorld
{
    private readonly Dictionary<Type, IEcsPool> _pools = new(64);
    private readonly List<IEcsRegisteredFilter> _filters = new(64);
    private readonly List<IEcsSystem> _allSystems = new(128);
    private readonly List<IInitSystem> _initSystems = new(128);
    private readonly List<IRunSystem> _runSystems = new(128);
    private readonly List<IPostRunSystem> _postRunSystems = new(128);
    private readonly List<IDisposeSystem> _disposeSystems = new(128);

    private int _poolsStorage = 2;
    private readonly EntityFactory _factory;

    public EcsWorld()
    {
        _factory = new EntityFactory(this);
    }
    
    public IEcsWorld AddSystem(IEcsSystem system)
    {
        _allSystems.Add(system);
        if (system is IInitSystem initSystem) _initSystems.Add(initSystem);
        if (system is IRunSystem runSystem) _runSystems.Add(runSystem);
        if (system is IPostRunSystem postRunSystem) _postRunSystems.Add(postRunSystem);
        if (system is IDisposeSystem disposeSystem) _disposeSystems.Add(disposeSystem);
        return this;
    }

    public IEcsWorld BindPool<T>() where T : struct
    {
        var type = typeof(T);
        if (_pools.ContainsKey(type)) return this;
        _pools.Add(type, new EcsPool<T>(type, EntityChangedHandler));
        _factory.UpdatePoolsAmount(_pools.Count);
        foreach (var filter in _filters) filter.UpdatePoolsAmount(_pools.Count);
        if (_poolsStorage * IEcsWorld.BitSize < _pools.Count) _poolsStorage++;
        return this;
    }

    private void EntityChangedHandler(Type poolType, int entity, bool added)
    {
        _factory.EntityChangedHandler(_pools[poolType].GetId(), entity, added, _filters);
    }

    public bool HasPool<T>() => _pools.ContainsKey(typeof(T));

    public IEcsWorld RegisterFilter(IEcsRegisteredFilter filter)
    {
        var incMaskArray = new uint[_poolsStorage];
        var excMaskArray = new uint[_poolsStorage];
        foreach (var type in filter.GetIncTypes())
        {
            var poolId = _pools[type].GetId();
            var arrayIndex = poolId / IEcsWorld.BitSize;
            var poolIndex = poolId - arrayIndex * IEcsWorld.BitSize;
            incMaskArray[arrayIndex] = BitHandler.SetBit(incMaskArray[arrayIndex], poolIndex, true);
        }

        foreach (var type in filter.GetDecTypes())
        {
            var poolId = _pools[type].GetId();
            var arrayIndex = poolId / IEcsWorld.BitSize;
            var poolIndex = poolId - arrayIndex * IEcsWorld.BitSize;
            excMaskArray[arrayIndex] = BitHandler.SetBit(excMaskArray[arrayIndex], poolIndex, true);
        }
        filter.SetMasks(incMaskArray, excMaskArray);
        _factory.InitializeFilter(filter);
        _filters.Add(filter);
        return this;
    }

    public int NewEntityId() => _factory.CreateEntityWithFiltersUpdate(_filters, out _);
    public Entity GetEntityById(int entity) => _factory.GetEntityClass(entity);

    public Entity NewEntity()
    {
        _factory.CreateEntityWithFiltersUpdate(_filters, out var result);
        return result;
    }

    public EcsPool<T> GetPool<T>() where T : struct
    {
        if (!_pools.ContainsKey(typeof(T))) BindPool<T>();
        return (EcsPool<T>) _pools[typeof(T)];
    }

    public IEcsPool GetPoolByType(Type type)
    {
        if (!_pools.ContainsKey(type)) throw new Exception("Pool is not registered");
        return _pools[type];
    }

    public IReadOnlyList<IEcsSystem> GetAllSystems() => _allSystems;

    public IEcsWorld Init()
    {
        foreach (var system in _initSystems) system.Init(this);
        return this;
    }

    public void Run()
    {
        foreach (var system in _runSystems) system.Run(this);
        foreach (var system in _postRunSystems) system.PostRun(this);
    }

    public void Dispose()
    {
        foreach (var system in _disposeSystems) system.Dispose(this);
    }

    public void Destroy()
    {
        _factory.Destroy();
        throw new NotImplementedException();
    }
}