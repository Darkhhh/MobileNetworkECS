namespace MobileNetworkECS.Tests.Pools;

[TestClass]
public class PoolTest
{
    [TestMethod]
    public void CorrectCreatingPoolsWithId()
    {
        var pools = new List<IEcsPool>
        {
            new EcsPool<Dummy1>(IEcsPool.CreateId(), typeof(Dummy1)),
            new EcsPool<Dummy2>(IEcsPool.CreateId(), typeof(Dummy2))
        };

        Assert.AreEqual(0, pools[0].GetId());
        Assert.AreEqual(1, pools[1].GetId());
    }

    [TestMethod]
    public void CorrectAddingAndChangingEntityComponent()
    {
        const int coinsAmount = 25;
        
        var entity = 1;
        var pool = new EcsPool<Dummy1>(IEcsPool.CreateId(), typeof(Dummy1));
        
        pool.Add(entity);
        ref var data = ref pool.Get(entity);
        data.Coins = coinsAmount;
        Assert.IsTrue(pool.Has(entity));

        ref var changedData = ref pool.Get(entity);
        Assert.AreEqual(coinsAmount, changedData.Coins);

        Assert.ThrowsException<Exception>(() => pool.Add(entity));
    }
    
    [TestMethod]
    public void CorrectRemovingEntityComponent()
    {
        var entity = 1;
        var pool = new EcsPool<Dummy1>(IEcsPool.CreateId(), typeof(Dummy1));
        
        pool.Add(entity);
        Assert.IsTrue(pool.Has(entity));
        
        pool.Remove(entity);
        Assert.IsFalse(pool.Has(entity));
        Assert.ThrowsException<Exception>(()=>pool.Get(entity));
    }
    
    [TestMethod]
    public void CorrectSeveralEntitiesHandling()
    {
        var pool = new EcsPool<Dummy1>(IEcsPool.CreateId(), typeof(Dummy1));
        
        var entity1 = 1;
        ref var entity1Data = ref pool.Add(entity1);
        entity1Data.Coins = 4;
        
        var entity2 = 2;
        ref var entity2Data = ref pool.Add(entity2);
        entity2Data.Coins = 7;
        
        Assert.IsTrue(pool.Has(entity1));
        Assert.IsTrue(pool.Has(entity2));
        
        pool.Remove(entity1);
        Assert.IsFalse(pool.Has(entity1));
        Assert.IsTrue(pool.Has(entity2));

        ref var d = ref pool.Get(entity2);
        Assert.AreEqual(7, d.Coins);
    }
    
    private struct Dummy1
    {
        public int Coins;
    }
    
    private struct Dummy2
    {
        public int Health;
    }
}