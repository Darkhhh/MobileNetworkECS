using MobileNetworkECS.Core.Filters;

namespace MobileNetworkECS.Core.Worlds;

public interface IEcsWorld
{
    public const int BitSize = 64;
    
    public IEcsWorld AddSystem(IEcsSystem system);
    public IEcsWorld BindPool<T>() where T : struct;

    public bool HasPool<T>();
    
    /// <summary>
    /// Получает из регистированного фильтра типы пулов, на их основе создает маски и передает в фильтр
    /// </summary>
    /// <param name="filter">Зарегистрированный фильтр, создается внутри EcsFilter</param>
    /// <returns></returns>
    public IEcsWorld RegisterFilter(IEcsRegisteredFilter filter);
    public int CreateEntity();

    public EcsPool<T> GetPool<T>() where T : struct;
    public IEcsPool GetPoolByType(Type type);

    public IReadOnlyList<IEcsSystem> GetAllSystems();

    public IEcsWorld Init();
    public void Run();

    public void Dispose();
    public void Destroy();
}