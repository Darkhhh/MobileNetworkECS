using MobileNetworkECS.Core;
using MobileNetworkECS.Core.Worlds;

namespace MobileNetworkECS.Extensions.SystemGroups;

public interface ISystemGroup : IInitSystem, IRunSystem, IPostRunSystem, IDisposeSystem
{
    public string GetName();
    public bool IsActive();
    public void Activate(bool forceExecute = false, bool deactivateAfterExecute = false);
    public void Deactivate();

    public IReadOnlyList<IEcsSystem> GetAllSystems();
}

public class SystemGroup : ISystemGroup
{
    private readonly List<IEcsSystem> _allSystems = new(8);
    private readonly List<IInitSystem> _initSystems = new(8);
    private readonly List<IRunSystem> _runSystems = new(8);
    private readonly List<IPostRunSystem> _postRunSystems = new(8);
    private readonly List<IDisposeSystem> _disposeSystems = new(8);

    private readonly IEcsWorld _world;
    private readonly string _name;
    private bool _active, _deactivateAfterExecute;

    public SystemGroup(IEcsWorld world, string name)
    {
        _name = name;
        _world = world;
        _active = true;
        _deactivateAfterExecute = false;
    }
    
    public SystemGroup AddSystem(IEcsSystem system)
    {
        _allSystems.Add(system);
        if (system is IInitSystem initSystem) _initSystems.Add(initSystem);
        if (system is IRunSystem runSystem) _runSystems.Add(runSystem);
        if (system is IPostRunSystem postRunSystem) _postRunSystems.Add(postRunSystem);
        if (system is IDisposeSystem disposeSystem) _disposeSystems.Add(disposeSystem);
        return this;
    }
    
    public void Init(IEcsWorld world)
    {
        foreach (var system in _initSystems) system.Init(world);
    }

    public void Run(IEcsWorld world)
    {
        if (!_active) return;
        foreach (var system in _runSystems) system.Run(world);
    }

    public void PostRun(IEcsWorld world)
    {
        if (!_active) return;
        foreach (var system in _postRunSystems) system.PostRun(world);
        if (!_deactivateAfterExecute) return;
        _deactivateAfterExecute = false;
        _active = false;
    }

    public void Dispose(IEcsWorld world)
    {
        if (!_active) return;
        foreach (var system in _disposeSystems) system.Dispose(world);
    }

    public string GetName() => _name;

    public bool IsActive() => _active;

    public void Activate(bool forceExecute = false, bool deactivateAfterExecute = false)
    {
        _active = true;
        _deactivateAfterExecute = deactivateAfterExecute;
        if (!forceExecute) return;
        
        Run(_world);
        PostRun(_world);
    }

    public void Deactivate() => _active = false;
    public IReadOnlyList<IEcsSystem> GetAllSystems() => _allSystems;
}