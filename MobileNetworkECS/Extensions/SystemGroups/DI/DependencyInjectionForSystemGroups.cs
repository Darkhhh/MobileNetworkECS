using MobileNetworkECS.Core.Worlds;
using MobileNetworkECS.Extensions.DI;

namespace MobileNetworkECS.Extensions.SystemGroups.DI;

public static class DependencyInjectionForSystemGroups
{
    public static IEcsWorld InjectToSystemGroups(this IEcsWorld world, params object[] injects)
    {
        foreach (var system in world.GetAllSystems())
        {
            if (system is not SystemGroup systemGroup) continue;
            foreach (var ecsSystem in systemGroup.GetAllSystems())
            {
                ExtensionsDependencyInjection.FillPoolsToSystem(world, ecsSystem);
                ExtensionsDependencyInjection.FillFiltersToSystem(world, ecsSystem);
                ExtensionsDependencyInjection.FillCustomsToSystem(ecsSystem, injects);
            }
        }
        return world;
    }
}