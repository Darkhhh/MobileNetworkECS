using System.Reflection;
using MobileNetworkECS.Core.Filters;
using MobileNetworkECS.Core.Utils;
using MobileNetworkECS.Core.Worlds;

namespace MobileNetworkECS.Tests.Filters;

[TestClass]
public class FilterTest
{
    private static readonly Dictionary<Type, IEcsPool> Pools = new();
    private static readonly List<int> Entities = new();
    private const int PoolsStorage = 2;
    [ClassInitialize]
    public static void Initialize(TestContext testContext)
    {
        Pools.Add(typeof(Position), new EcsPool<Position>(typeof(Position)));
        Pools.Add(typeof(Velocity), new EcsPool<Velocity>(typeof(Velocity)));
        
        Entities.AddRange(new List<int>{1, 2, 3});
    }
    private static EcsPool<T> GetPool<T>() where T : struct => (EcsPool<T>) Pools[typeof(T)];

    // Method was taken from EcsWorld.RegisterFilter(filter)
    private static (uint[] incMask, uint[] excMask) GetMasksByTypes(EcsRegisteredFilter filter)
    {
        var incMaskArray = new uint[PoolsStorage];
        var excMaskArray = new uint[PoolsStorage];
        foreach (var type in filter.GetIncTypes())
        {
            var poolId = Pools[type].GetId();
            var arrayIndex = poolId / IEcsWorld.BitSize;
            var poolIndex = poolId - arrayIndex * IEcsWorld.BitSize;
            incMaskArray[arrayIndex] = BitHandler.SetBit(incMaskArray[arrayIndex], poolIndex, true);
        }

        foreach (var type in filter.GetDecTypes())
        {
            var poolId = Pools[type].GetId();
            var arrayIndex = poolId / IEcsWorld.BitSize;
            var poolIndex = poolId - arrayIndex * IEcsWorld.BitSize;
            excMaskArray[arrayIndex] = BitHandler.SetBit(excMaskArray[arrayIndex], poolIndex, true);
        }

        return (incMaskArray, excMaskArray);
    }

    private static EcsRegisteredFilter GetInnerFilter(EcsFilter filter)
    {
        var type = typeof(EcsFilter);
        foreach (var fieldInfo in type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic))
        {
            if (!fieldInfo.FieldType.IsAssignableFrom(typeof(EcsRegisteredFilter))) continue;
            
            var innerFilter = fieldInfo.GetValue(filter) ?? throw new Exception("Inner filter is null");
            return (EcsRegisteredFilter) innerFilter;
        }

        throw new Exception("Can not get registered filter");
    }
    
    [TestMethod]
    public void FilterAndRegisteredFilterCombinedWorkCorrectly()
    {
        var worldPlug = new EcsWorldPlug();
        var filter = new EcsFilter(new EcsWorldPlug());
        filter.Inc<Position>().Exc<Velocity>().Register(worldPlug);
        var registeredFilter = GetInnerFilter(filter);
        
        var positionPool = GetPool<Position>();
        var velocityPool = GetPool<Velocity>();
        var masks = GetMasksByTypes(registeredFilter);
        registeredFilter.SetMasks(masks.incMask, masks.excMask);

        var suitableEntities = new List<RawEntity>();
        for (var i = 0; i < Entities.Count - 1; i++)
        {
            var e = new RawEntity(i);
            e.SetPool(positionPool.GetId(), true);
            registeredFilter.CheckEntity(e, out var added, out var removed);
            Assert.IsTrue(added);
            Assert.IsFalse(removed);
            suitableEntities.Add(e);
        }

        Assert.AreEqual(suitableEntities.Count, filter.Cast<int>().Count());
    }

    [TestMethod]
    public void AddAndDeleteInFilterWorksCorrectly()
    {
        var worldPlug = new EcsWorldPlug();
        var filter = new EcsFilter(new EcsWorldPlug());
        filter.Inc<Position>().Exc<Velocity>().Register(worldPlug);
        var registeredFilter = GetInnerFilter(filter);
        
        var positionPool = GetPool<Position>();
        var velocityPool = GetPool<Velocity>();
        var masks = GetMasksByTypes(registeredFilter);
        registeredFilter.SetMasks(masks.incMask, masks.excMask);
        
        var suitableEntities = new List<RawEntity>();
        for (var i = 0; i < Entities.Count - 1; i++)
        {
            var e = new RawEntity(i);
            e.SetPool(positionPool.GetId(), true);
            registeredFilter.CheckEntity(e, out var added, out var removed);
            Assert.IsTrue(added);
            Assert.IsFalse(removed);
            suitableEntities.Add(e);
        }

        var c = 0;
        foreach (int _ in filter) c++;
        
        Assert.AreEqual(suitableEntities.Count, c);
        var runCount = 0;
        foreach (int entity in filter)
        {
            runCount++;
            if (runCount == 1)
            {
                suitableEntities[0].SetPool(velocityPool.GetId(), true);
                registeredFilter.CheckEntity(suitableEntities[0], out var added, out var removed);
                Assert.IsFalse(added);
                Assert.IsTrue(removed);
            }
        }
        Assert.AreEqual(suitableEntities.Count, runCount);
        c = 0;
        foreach (int _ in filter) c++;
        Assert.AreEqual(suitableEntities.Count - 1, c);
    }
    
    
    private struct Position { public float X, Y; }
    private struct Velocity { public float vX, vY; }

    private class EcsWorldPlug : IEcsWorld
    {
        public IEcsWorld AddSystem(IEcsSystem system) => this;
        public IEcsWorld BindPool<T>() where T : struct => this;
        public bool HasPool<T>() => false;
        public IEcsWorld RegisterFilter(IEcsRegisteredFilter filter) => this;
        public int CreateEntity() => -1;
        public EcsPool<T> GetPool<T>() where T : struct => null;
        public IEcsPool GetPoolByType(Type type) => null;
        public IReadOnlyList<IEcsSystem> GetAllSystems() => Array.Empty<IEcsSystem>().ToList();
        public IEcsWorld Init() => this;
        public void Run() {  }
        public void Dispose() {  }
        public void Destroy() {  }
    }
}