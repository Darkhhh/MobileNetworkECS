using MobileNetworkECS.Core.Filters;
using MobileNetworkECS.Core.Worlds;
using MobileNetworkECS.Tests.Filters;

namespace MobileNetworkECS.Tests.Entities;

[TestClass]
public class EntityFactoryTests
{
    [TestMethod]
    public void CorrectPoolsUpdate()
    {
        var worldPlug = new FilterTest.EcsWorldPlug();
        var factory = new EntityFactory(worldPlug);
        Assert.IsFalse(factory.UpdatePoolsAmount(113));
        Assert.IsTrue(factory.UpdatePoolsAmount(129));
        Assert.IsFalse(factory.UpdatePoolsAmount(130));
    }

    [TestMethod]
    public void CorrectCreatingAndUpdatingEntity()
    {
        var worldPlug = new FilterTest.EcsWorldPlug();
        var factory = new EntityFactory(worldPlug);
        var firstCreatedEntity = factory.CreateEntityWithFiltersUpdate(Array.Empty<IEcsRegisteredFilter>(), out _);
        var secondCreatedEntity = factory.CreateEntityWithFiltersUpdate(Array.Empty<IEcsRegisteredFilter>(), out _);
        
        factory.EntityChangedHandler(0, firstCreatedEntity, true, Array.Empty<IEcsRegisteredFilter>());
        factory.EntityChangedHandler(2, secondCreatedEntity, true, Array.Empty<IEcsRegisteredFilter>());
        
        factory.EntityChangedHandler(0, firstCreatedEntity, false, Array.Empty<IEcsRegisteredFilter>());
        
        Assert.IsFalse(factory.GetRawEntity(firstCreatedEntity).Exist());
        
        var thirdCreatedEntity = factory.CreateEntityWithFiltersUpdate(Array.Empty<IEcsRegisteredFilter>(), out _);
        
        Assert.AreEqual(firstCreatedEntity, thirdCreatedEntity);
        Assert.IsTrue(factory.GetRawEntity(firstCreatedEntity).Exist());
    }
}