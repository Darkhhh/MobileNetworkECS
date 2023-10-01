using MobileNetworkECS.Core.Filters;

namespace MobileNetworkECS.Core.Worlds;

public class EntityFactory
{
    private RawEntity[] _entities = new RawEntity[256];
    private int _entitiesNumber = 0;
    private int[] _recycled = new int[128];
    private int _recycledEntitiesNumber = 0;
    private int _poolsStorage = 2;

    
    public bool UpdatePoolsAmount(int amount)
    {
        if (amount <= _poolsStorage * IEcsWorld.BitSize) return false;
        for (var i = 0; i < _entitiesNumber; i++) _entities[i].IncreasePoolsAmount();
        _poolsStorage++;
        return true;
    }

    public int CreateEntityWithFiltersUpdate(IEnumerable<IEcsRegisteredFilter> filters)
    {
        if (_recycledEntitiesNumber > 0)
        {
            var rawEntity = _entities[_recycledEntitiesNumber - 1];
            _recycled[_recycledEntitiesNumber - 1] = -1;
            _recycledEntitiesNumber--;
            
            rawEntity.Refresh();
            return rawEntity.GetId();
        }

        var id = IRawEntity.CreateId();
        if (_entitiesNumber + 1 > _entities.Length) Array.Resize(ref _entities, _entitiesNumber * 2);
        _entities[_entitiesNumber] = new RawEntity(id);
        _entitiesNumber++;
        
        foreach (var filter in filters) filter.UpdateMaxEntityIndex(id);
        
        return id;
    }

    public void EntityChangedHandler(int poolId, int entity, bool added)
    {
#if DEBUG
        if (entity > _entitiesNumber + 1) throw new Exception("Indexes grow faster than _rawEntities");
#endif
        var rawEntity = _entities[entity - 1];
#if DEBUG
        if (rawEntity.GetId() != entity) throw new Exception("rawEntity.GetId() is not equal to entity");
#endif
        
        rawEntity.SetPool(poolId, added);
        
        if (!rawEntity.IsEmpty()) return;
        
        rawEntity.Reset();
        
        if (_recycledEntitiesNumber + 1 > _recycled.Length) Array.Resize(ref _recycled, _recycled.Length * 2);
        _recycled[_recycledEntitiesNumber] = entity;
        _recycledEntitiesNumber++;
    }

    public RawEntity GetRawEntity(int entity) => _entities[entity - 1];

    public void InitializeFilter(IEcsRegisteredFilter filter)
    {
        for (var i = 0; i < _entitiesNumber; i++)
        {
            filter.CheckEntity(_entities[i], out _, out _);
        }
    }

    public void Destroy()
    {
        _entities = Array.Empty<RawEntity>();
        _recycled = Array.Empty<int>();
        _entitiesNumber = 0;
        _recycledEntitiesNumber = 0;
        _poolsStorage = 2;
    }
}