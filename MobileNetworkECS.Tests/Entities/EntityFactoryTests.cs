using MobileNetworkECS.Core.Filters;
using MobileNetworkECS.Core.Worlds;

namespace MobileNetworkECS.Tests.Entities;

[TestClass]
public class EntityFactoryTests
{
    [TestMethod]
    public void CorrectPoolsUpdate()
    {
        var factory = new EntityFactory();
        Assert.IsFalse(factory.UpdatePoolsAmount(113));
        Assert.IsTrue(factory.UpdatePoolsAmount(129));
        Assert.IsFalse(factory.UpdatePoolsAmount(130));
    }

    [TestMethod]
    public void CorrectCreatingAndUpdatingEntity()
    {
        var factory = new EntityFactory();
        var firstCreatedEntity = factory.CreateEntityWithFiltersUpdate(Array.Empty<IEcsRegisteredFilter>());
        var secondCreatedEntity = factory.CreateEntityWithFiltersUpdate(Array.Empty<IEcsRegisteredFilter>());
        
        factory.EntityChangedHandler(0, firstCreatedEntity, true);
        factory.EntityChangedHandler(2, secondCreatedEntity, true);
        
        factory.EntityChangedHandler(0, firstCreatedEntity, false);
        
        Assert.IsFalse(factory.GetRawEntity(firstCreatedEntity).Exist());
        
        var thirdCreatedEntity = factory.CreateEntityWithFiltersUpdate(Array.Empty<IEcsRegisteredFilter>());
        
        Assert.AreEqual(firstCreatedEntity, thirdCreatedEntity);
        Assert.IsTrue(factory.GetRawEntity(firstCreatedEntity).Exist());
    }
}