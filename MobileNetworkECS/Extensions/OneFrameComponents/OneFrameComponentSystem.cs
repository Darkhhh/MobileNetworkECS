using MobileNetworkECS.Core;
using MobileNetworkECS.Core.Filters;
using MobileNetworkECS.Core.Worlds;

namespace MobileNetworkECS.Extensions.OneFrameComponents;

public class OneFrameComponentSystem<T> : IInitSystem, IPostRunSystem where T : struct, IOneFrameComponent
{
    private IEcsFilter _filter = null!;
    private EcsPool<T> _pool = null!;
    public void Init(IEcsWorld world)
    {
        _pool = world.GetPool<T>();
        _filter = new EcsFilter(world).Inc<T>().Register();
    }
    public void PostRun(IEcsWorld world)
    {
        foreach (int entity in _filter) _pool.Remove(entity);
    }
}