using System.Reflection;
using MobileNetworkECS.Core;
using MobileNetworkECS.Core.Worlds;

namespace MobileNetworkECS.Extensions.DI;

[AttributeUsage(AttributeTargets.Field)]
public abstract class EcsPoolInjectBaseAttribute : Attribute
{
    public abstract Type GetPoolType();
    public abstract void Bind(IEcsWorld world);
}

public class EcsPoolInjectAttribute<TComponent> : EcsPoolInjectBaseAttribute where TComponent : struct
{
    public override void Bind(IEcsWorld world) => world.BindPool<TComponent>();
    public override Type GetPoolType() => typeof(TComponent);
}


[AttributeUsage(AttributeTargets.Field)]
public class InjectCustomAttribute : Attribute { }


[AttributeUsage(AttributeTargets.Field)]
public class EcsFilterInjectAttribute : Attribute { }

[AttributeUsage(AttributeTargets.Field)]
public abstract class EcsFilterIncInjectBaseAttribute : Attribute
{
    public abstract void Bind(IEcsWorld world);
    public abstract Type[] GetPoolTypes();
}

[AttributeUsage(AttributeTargets.Field)]
public abstract class EcsFilterExcInjectBaseAttribute : Attribute
{
    public abstract void Bind(IEcsWorld world);
    public abstract Type[] GetPoolTypes();
}
