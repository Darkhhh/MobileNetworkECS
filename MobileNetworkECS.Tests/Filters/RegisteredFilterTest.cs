using System.Collections;
using MobileNetworkECS.Core.Filters;
using MobileNetworkECS.Core.Utils;
using MobileNetworkECS.Core.Worlds;

namespace MobileNetworkECS.Tests.Filters;

[TestClass]
public class RegisteredFilterTest
{
    private static readonly Dictionary<Type, IEcsPool> Pools = new();
    private static readonly List<int> Entities = new();
    [ClassInitialize]
    public static void Initialize(TestContext testContext)
    {
        Pools.Add(typeof(Position), new EcsPool<Position>(typeof(Position)));
        Pools.Add(typeof(Velocity), new EcsPool<Velocity>(typeof(Velocity)));
        
        Entities.AddRange(new List<int>{1, 2, 3});
    }
    private static EcsPool<T> GetPool<T>() where T : struct => (EcsPool<T>) Pools[typeof(T)];

    
    [TestMethod]
    public void CorrectInitializing()
    {
        var filter = new EcsRegisteredFilter(new EcsFilterPlug(), Array.Empty<Type>(), Array.Empty<Type>());
        Assert.IsNotNull(Entities);
        Assert.AreEqual(3, Entities.Count);
        Assert.IsNotNull(Pools);
        Assert.AreEqual(2, Pools.Count);
        var positionPool = GetPool<Position>();
        var velocityPool = GetPool<Velocity>();
        Assert.IsNotNull(positionPool);
        Assert.IsNotNull(velocityPool);
    }

    [TestMethod]
    public void CorrectFilterEntityCheck()
    {
        var worldPlug = new FilterTest.EcsWorldPlug();
        var filter = new EcsRegisteredFilter(new EcsFilterPlug(), Array.Empty<Type>(), Array.Empty<Type>());
        var positionPool = GetPool<Position>();
        var velocityPool = GetPool<Velocity>();
        // Masks equal to EcsFilter.Inc<Position>().Exc<Velocity>();
        var incMaskArray = new uint[2];
        var excMaskArray = new uint[2];
        incMaskArray[0] = BitHandler.SetBit(incMaskArray[0], positionPool.GetId(), true);
        excMaskArray[0] = BitHandler.SetBit(excMaskArray[0], velocityPool.GetId(), true);
        filter.SetMasks(incMaskArray, excMaskArray);


        var entity1 = new RawEntity(worldPlug, Entities[0]);
        entity1.SetPool(positionPool.GetId(), true);
        filter.CheckEntity(entity1, out var added, out var removed);
        Assert.IsTrue(added);
        Assert.IsFalse(removed);
        
        var entity2 = new RawEntity(worldPlug, Entities[1]);
        entity2.SetPool(positionPool.GetId(), true);
        entity2.SetPool(velocityPool.GetId(), true);
        filter.CheckEntity(entity2, out added, out removed);
        Assert.IsFalse(added);
        Assert.IsFalse(removed);
        
        var entity3 = new RawEntity(worldPlug, Entities[1]);
        entity3.SetPool(positionPool.GetId(), false);
        entity3.SetPool(velocityPool.GetId(), true);
        filter.CheckEntity(entity3, out added, out removed);
        Assert.IsFalse(added);
        Assert.IsFalse(removed);
    }

    [TestMethod]
    public void CorrectAddingAndRemoving()
    {
        var worldPlug = new FilterTest.EcsWorldPlug();
        var filter = new EcsRegisteredFilter(new EcsFilterPlug(), Array.Empty<Type>(), Array.Empty<Type>());
        var positionPool = GetPool<Position>();
        var velocityPool = GetPool<Velocity>();
        // Masks equal to EcsFilter.Inc<Position>().Exc<Velocity>();
        var incMaskArray = new uint[2];
        var excMaskArray = new uint[2];
        incMaskArray[0] = BitHandler.SetBit(incMaskArray[0], positionPool.GetId(), true);
        excMaskArray[0] = BitHandler.SetBit(excMaskArray[0], velocityPool.GetId(), true);
        filter.SetMasks(incMaskArray, excMaskArray);

        // entity1 is acceptable, and not in the filter
        var entity1 = new RawEntity(worldPlug, Entities[0]);
        entity1.SetPool(positionPool.GetId(), true);
        filter.CheckEntity(entity1, out var added, out var removed);
        Assert.IsTrue(added);
        Assert.IsFalse(removed);
        Assert.AreEqual(1, filter.Count);
        
        // entity1 is not acceptable, and in the pool
        entity1.SetPool(velocityPool.GetId(), true);
        filter.CheckEntity(entity1, out added, out removed);
        Assert.IsTrue(removed);
        Assert.IsFalse(added);
        Assert.AreEqual(0, filter.Count);
        
        // entity1 is not acceptable, and not in the pool
        entity1.SetPool(velocityPool.GetId(), false);
        entity1.SetPool(positionPool.GetId(), false);
        filter.CheckEntity(entity1, out added, out removed);
        Assert.IsFalse(added);
        Assert.IsFalse(removed);
    }

    private class EcsFilterPlug : IEcsFilter
    {
        public IEnumerator GetEnumerator() => null;
        public bool MoveNext() => false;
        public void Reset() {  }
        public object Current => null;
        public IEcsFilter Inc<T>() where T : struct => this;
        public IEcsFilter Exc<T>() where T : struct => this;
        public IEcsFilter Register() => this;
        public IEcsFilter SetIncTypes(params Type[] types) => this;
        public IEcsFilter SetExcTypes(params Type[] types) => this;
        public IEcsFilter EnumerateAsEntity() => this;
        public IEcsFilter EnumerateAsEntityId() => this;
        public Action? FilterWasUpdated { get; set; }
    }
    private struct Position { public float X, Y; }
    private struct Velocity { public float vX, vY; }
}