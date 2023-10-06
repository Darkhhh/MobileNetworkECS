using MobileNetworkECS.Core.Worlds;

namespace MobileNetworkECS.Extensions.EntityWrap;

public abstract class EntityWrapBase
{
    public abstract object GetRawValue();
    public abstract int GetEntityId();
    public abstract IEcsWorld GetWorld();
}

public class EntityWrap<T> : EntityWrapBase where T : class
{
    public T Value { get; init; }

    private readonly IEcsWorld _world;
    private readonly int _entity;
    
    public EntityWrap(T item, IEcsWorld world)
    {
        Value = item;
        _world = world;

        var wrapPool = _world.GetPool<EntityWrapComponent>();
        _entity = _world.CreateEntity();
        ref var data = ref wrapPool.Add(_entity);
        data.Reference = this;
    }

    public override object GetRawValue() => Value;

    public override int GetEntityId() => _entity;

    public override IEcsWorld GetWorld() => _world;
}