using MobileNetworkECS.Core.Worlds;

namespace MobileNetworkECS.Extensions.DI;

public class FilterExc<T1> : EcsFilterExcInjectBaseAttribute where T1 : struct
{
    public override void Bind(IEcsWorld world)
    {
        world.BindPool<T1>();
    }

    public override Type[] GetPoolTypes()
    {
        return new[] {typeof(T1)};
    }
}

public class FilterExc<T1,T2> : EcsFilterExcInjectBaseAttribute 
    where T1 : struct 
    where T2 : struct
{
    public override void Bind(IEcsWorld world)
    {
        world.BindPool<T1>().BindPool<T2>();
    }
    public override Type[] GetPoolTypes()
    {
        return new[] {typeof(T1), typeof(T2)};
    }
}

public class FilterExc<T1,T2,T3> : EcsFilterExcInjectBaseAttribute 
    where T1 : struct 
    where T2 : struct
    where T3 : struct
{
    public override void Bind(IEcsWorld world)
    {
        world.BindPool<T1>().BindPool<T2>().BindPool<T3>();
    }
    public override Type[] GetPoolTypes()
    {
        return new[] {typeof(T1), typeof(T2), typeof(T3)};
    }
}

public class FilterExc<T1,T2,T3,T4> : EcsFilterExcInjectBaseAttribute 
    where T1 : struct 
    where T2 : struct
    where T3 : struct
    where T4 : struct
{
    public override void Bind(IEcsWorld world)
    {
        world.BindPool<T1>().BindPool<T2>().BindPool<T3>().BindPool<T4>();
    }
    public override Type[] GetPoolTypes()
    {
        return new[] {typeof(T1), typeof(T2), typeof(T3), typeof(T4)};
    }
}