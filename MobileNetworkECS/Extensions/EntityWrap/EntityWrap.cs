using MobileNetworkECS.Core.Entities;
using MobileNetworkECS.Core.Worlds;

namespace MobileNetworkECS.Extensions.EntityWrap;

public abstract class EntityWrapBase
{
    public abstract object GetRawValue();
    public abstract int GetEntityId();
    public abstract IEcsWorld GetWorld();
    public abstract bool TryGetAs<T>(out T result);
    public abstract Entity GetEntity();
}

public class EntityWrap<T> : EntityWrapBase where T : class
{
    public T Value { get; }
    private readonly Entity _entity;

    private readonly IEcsWorld _world;
    private readonly int _entityId;
    
    public EntityWrap(T item, IEcsWorld world)
    {
        Value = item;
        _world = world;
        
        var wrapPool = _world.GetPool<EntityWrapComponent>();
        _entityId = _world.NewEntityId();
        _entity = _world.GetEntityById(_entityId);
        ref var data = ref wrapPool.Add(_entityId);
        data.Reference = this;
    }

    public override object GetRawValue() => Value;

    public override int GetEntityId() => _entityId;

    public override IEcsWorld GetWorld() => _world;
    public override bool TryGetAs<T1>(out T1 result)
    {
        result = default!;
        if (Value is not T1 r) return false;
        result = r;
        return true;
    }
    public override Entity GetEntity() => _entity;
}