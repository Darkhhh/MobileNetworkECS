using MobileNetworkECS.Core.Worlds;

namespace MobileNetworkECS.Core.Entities;

public class Entity
{
    private readonly int _id;
    private readonly IEcsWorld _world;

    public Entity(IEcsWorld world, int id) => (_world, _id) = (world, id);

    public int GetId() => _id;

    public ref T AddComponent<T>() where T : struct => ref _world.GetPool<T>().Add(_id);
    
    public ref T GetComponent<T>() where T : struct
    {
        var pool = _world.GetPool<T>();
        if (pool.Has(_id)) return ref pool.Get(_id);
        throw new Exception("Trying to get component that do not exist");
    }

    public ref T GetOrAddComponent<T>() where T : struct
    {
        var pool = _world.GetPool<T>();
        if (pool.Has(_id)) return ref pool.Get(_id);
        return ref pool.Add(_id);
    }

    public bool HasComponent<T>() where T : struct => _world.GetPool<T>().Has(_id);

    public void RemoveComponent<T>() where T : struct => _world.GetPool<T>().Remove(_id);
}