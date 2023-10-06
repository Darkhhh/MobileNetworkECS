using MobileNetworkECS.Core.Worlds;

namespace MobileNetworkECS.Extensions.SystemGroups;

public static class SystemGroupExtensions
{
    /// <summary>
    /// Returns SystemGroup by name. Better not to use.
    /// </summary>
    /// <param name="world">World, where SystemGroup added</param>
    /// <param name="groupName">Name of the SystemGroup</param>
    /// <returns></returns>
    public static ISystemGroup GetSystemGroup(this IEcsWorld world, string groupName)
    {
        foreach (var system in world.GetAllSystems())
        {
            if (system is not ISystemGroup systemGroup) continue;
            if (systemGroup.GetName() == groupName) return systemGroup;
        }
        throw new Exception($"Could not find SystemGroup with {groupName} name.");
    }
}