using MobileNetworkECS.Core.Filters;
using MobileNetworkECS.Core.Worlds;

namespace MobileNetworkECS.Tests.Entities;

[TestClass]
public class EntityClassTests
{
    [TestMethod]
    public void CorrectComponentAddingUsage()
    {
        IRawEntity.ResetIds();
        var world = new EcsWorld();
        var filter = new EcsFilter(world);
        filter.Inc<Player>().Exc<Health>().Register();
        var entity = world.NewEntity();
        entity.AddComponent<Player>();
        Assert.AreEqual(1, filter.Count);
        foreach (int e in filter)
        {
            var ent = world.GetEntityById(e);
            Assert.IsTrue(ReferenceEquals(entity, ent));
            ref var component = ref ent.AddComponent<Health>();
            component.Value = 100;
        }

        foreach (int _ in filter) { }
        Assert.AreEqual(0, filter.Count);
    }

    [TestMethod]
    public void CorrectGettingHaveCheckAndRemoving()
    {
        IRawEntity.ResetIds();
        var world = new EcsWorld();
        var entityId = world.NewEntityId();
        var entity = world.GetEntityById(entityId);
        Assert.AreEqual(entityId, entity.GetId());
        entity.AddComponent<Player>();
        Assert.IsTrue(world.GetPool<Player>().Has(entityId));
        Assert.IsTrue(entity.HasComponent<Player>());
        entity.RemoveComponent<Player>();
        Assert.IsFalse(world.GetPool<Player>().Has(entityId));
        Assert.IsFalse(entity.HasComponent<Player>());
    }

    private struct Health
    {
        public int Value;
    }

    private struct Player
    {
        
    }
}