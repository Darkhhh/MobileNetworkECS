namespace MobileNetworkECS.Tests.Entities;

[TestClass]
public class RawEntityFunctioningUnitTests
{
    [TestMethod]
    public void CorrectCreatingIndices()
    {
        var index = 1;
        for (var i = 0; i < EntitiesAmount; i++)
        {
            Assert.AreEqual(index, _entities[i].GetId());
            index++;
        }
        
        _entities[0].SetId(IRawEntity.CreateId());
        Assert.AreEqual(index, _entities[0].GetId());
    }

    [TestMethod]
    public void CorrectPoolAssignment()
    {
        var entity = new RawEntity(IRawEntity.CreateId());
        entity.SetPool(0, true);
        Assert.AreEqual(entity.GetPoolsAttachment()[0], 1u);
    }

    [TestMethod]
    public void CorrectExistAndIsEmptyMethods()
    {
        var entity = new RawEntity(0);
        entity.SetPool(10, true);
        Assert.IsFalse(entity.IsEmpty());
        Assert.IsTrue(entity.Exist());
        
        entity.SetPool(10, false);
        Assert.IsTrue(entity.IsEmpty());
        Assert.IsTrue(entity.Exist());
    }

    [TestMethod]
    public void CorrectSetPoolMethod()
    {
        var entity = new RawEntity(0);
        entity.SetPool(64, true);
        Assert.IsFalse(entity.IsEmpty());
        Assert.AreEqual(1u, entity.GetPoolsAttachment()[1]);
        Assert.AreEqual(0u, entity.GetPoolsAttachment()[0]);
        
        entity.UpdatePoolsAmount(129);
        entity.SetPool(129, true);
        Assert.AreEqual(2u, entity.GetPoolsAttachment()[2]);
    }

    private static List<RawEntity> _entities = null!;
    private const int EntitiesAmount = 10;
    [ClassInitialize]
    public static void Initialize(TestContext testContext)
    {
        _entities = new List<RawEntity>(EntitiesAmount);
        for (var i = 0; i < EntitiesAmount; i++)
        {
            _entities.Add(new RawEntity(IRawEntity.CreateId()));
        }
    }
}