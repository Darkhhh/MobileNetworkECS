using MobileNetworkECS.Core.Utils;
using MobileNetworkECS.Core.Worlds;

namespace MobileNetworkECS.Core;

public interface IRawEntity
{
    private static int _id;
    public static int CreateId() => ++_id;
    public static void ResetIds() => _id = 0;
    public int GetId();
    public uint[] GetPoolsAttachment();
}

public class RawEntity : IRawEntity
{
    private int _entityId;
    private bool _exists;
    private uint[] _poolsAttachment = new uint[2];

    
    public RawEntity(int id)
    {
        _entityId = id;
        _exists = true;
    }
    
    public void SetPool(int poolId, bool belong)
    {
        var arrayIndex = poolId / IEcsWorld.BitSize;
        var poolIndex = poolId - arrayIndex * IEcsWorld.BitSize;
        
        if (belong == BitHandler.GetBit(_poolsAttachment[arrayIndex], poolIndex)) return;
        _poolsAttachment[arrayIndex] = BitHandler.SetBit(_poolsAttachment[arrayIndex], poolIndex, belong);
    }

    public void IncreasePoolsAmount()
    {
        Array.Resize(ref _poolsAttachment, _poolsAttachment.Length + 1);
        _poolsAttachment[^1] = 0;
    }
    
    public void Reset()
    {
        for (var i = 0; i < _poolsAttachment.Length; i++) _poolsAttachment[i] = 0;
        _exists = false;
    }

    public void Refresh() => _exists = true;

    public int GetId() => _entityId;

    public void SetId(int id) => (_entityId, _exists) = (id, true);

    public uint[] GetPoolsAttachment() => _poolsAttachment;
    
    public bool IsEmpty() => _poolsAttachment.All(t => t == 0);

    public bool Exist() => _exists;
}