using System.Reflection;
using MobileNetworkECS.Core;
using MobileNetworkECS.Core.Filters;
using MobileNetworkECS.Core.Worlds;

namespace MobileNetworkECS.Extensions.DI;

public static class ExtensionsDependencyInjection
{
    public static void InjectFiltersAndPools(this IEcsWorld world)
    {
        foreach (var system in world.GetAllSystems())
        {
            FillPoolsToSystem(world, system);
            FillFiltersToSystem(world, system);
        }
    }

    public static void InjectCustoms(this IEcsWorld world, params object[] injects)
    {
        foreach (var system in world.GetAllSystems())
        {
            FillCustomsToSystem(system, injects);
        }
    }

    private static void FillPoolsToSystem(IEcsWorld world, IEcsSystem system)
    {
        var type = system.GetType();

        foreach (var fieldInfo in type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Instance))
        {
            foreach (var attribute in fieldInfo.GetCustomAttributes())
            {
                if (!attribute.GetType().IsAssignableFrom(typeof(EcsPoolInjectBaseAttribute))) continue;
                var attrData = (EcsPoolInjectBaseAttribute) attribute;
                attrData.Bind(world);
                var pool = world.GetPoolByType(attrData.GetPoolType());
                fieldInfo.SetValue(system, pool);
                break;
            }
        }
    }

    private static void FillFiltersToSystem(IEcsWorld world, IEcsSystem system)
    {
        var type = system.GetType();

        foreach (var fieldInfo in type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Instance))
        {
            var attributes = fieldInfo.GetCustomAttributes();
            var attrs = attributes as Attribute[] ?? attributes.ToArray();
            if (!attrs.Any(attr => attr.GetType().IsAssignableFrom(typeof(EcsFilterInjectAttribute)))) 
                continue;
            if (!fieldInfo.GetType().IsInstanceOfType(typeof(IEcsFilter)))
                throw new Exception("Filter inject attribute added to wrong field type");
            
            var filter = new EcsFilter(world);

            var incAttr = attrs.FirstOrDefault(attr => 
                attr.GetType().IsAssignableFrom(typeof(EcsFilterIncInjectBaseAttribute)));
            if (incAttr is not null)
            {
                var attrData = (EcsFilterIncInjectBaseAttribute) incAttr;
                attrData.Bind(world);
                filter.SetIncTypes(attrData.GetPoolTypes());
            }
            
            var excAttr = attrs.FirstOrDefault(attr => 
                attr.GetType().IsAssignableFrom(typeof(EcsFilterExcInjectBaseAttribute)));
            if (excAttr is not null)
            {
                var attrData = (EcsFilterIncInjectBaseAttribute) excAttr;
                attrData.Bind(world);
                filter.SetExcTypes(attrData.GetPoolTypes());
            }
            
            fieldInfo.SetValue(system, filter);
        }
    }

    private static void FillCustomsToSystem(IEcsSystem system, object[] customs)
    {
        var type = system.GetType();
        foreach (var fieldInfo in type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Instance))
        {
            if (!fieldInfo
                .GetCustomAttributes()
                .Any(attribute => attribute.GetType().IsAssignableFrom(typeof(InjectCustomAttribute)))) continue;

            foreach (var injectCustom in customs)
            {
                if (!fieldInfo.GetType().IsInstanceOfType(injectCustom)) continue;
                
                fieldInfo.SetValue(system, injectCustom);
            }
        }
    }
}