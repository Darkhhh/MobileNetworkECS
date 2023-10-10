using System.Reflection;
using MobileNetworkECS.Core;
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

    public static IEcsWorld InjectSystemGroups(this IEcsWorld world)
    {
        foreach (var system in world.GetAllSystems())
        {
            if (system is SystemGroup) continue;
            FillSystemGroups(world, system);
        }
        return world;
    }

    private static void FillSystemGroups(IEcsWorld world, IEcsSystem system)
    {
        var type = system.GetType();

        foreach (var fieldInfo in type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Instance))
        {
            foreach (var attribute in fieldInfo.GetCustomAttributes())
            {
                if (!attribute.GetType().IsAssignableFrom(typeof(SystemGroupInjectAttribute))) continue;
                var attrData = (SystemGroupInjectAttribute) attribute;
                var systemGroup = world.GetSystemGroup(attrData.Name);
                fieldInfo.SetValue(system, systemGroup);
                break;
            }
        }
    }
}