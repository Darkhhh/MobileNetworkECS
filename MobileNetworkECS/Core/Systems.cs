using MobileNetworkECS.Core.Worlds;

namespace MobileNetworkECS.Core;

public interface IEcsSystem { }

public interface IInitSystem : IEcsSystem
{
    public void Init(IEcsWorld world);
}

public interface IRunSystem : IEcsSystem
{
    public void Run(IEcsWorld world);
}

public interface IPostRunSystem : IEcsSystem
{
    public void PostRun(IEcsWorld world);
}

public interface IDisposeSystem : IEcsSystem
{
    public void Dispose(IEcsWorld world);
}