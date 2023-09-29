namespace MobileNetworkECS.Core;

public interface IRawEntity
{
    private static int _id;
    public static int CreateId() => ++_id;
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
        
        if (belong == GetBit(_poolsAttachment[arrayIndex], poolIndex)) return;
        _poolsAttachment[arrayIndex] = SetBit(_poolsAttachment[arrayIndex], poolIndex, belong);
    }
    private static uint SetBit(uint val, int index, bool bit)
    {
        if (index is < 0 or > 32) throw new Exception($"Incorrect bit index {index}");                      
        
        uint tempValue = 1;   
        tempValue <<= index; // устанавливаем нужный бит в единицу   
        val &= ~tempValue; // сбрасываем в 0 нужный бит             
        if (bit) // если бит требуется установить в 1             
        {                
            val |= tempValue; // то устанавливаем нужный бит в 1             
        } 
        return val;  
    } 
    private static bool GetBit(uint val, int index) 
    {           
        if (index is > 32 or < 0) throw new Exception($"Incorrect bit index {index}");
        return ((val>>index)&1)>0; // собственно все вычисления 
    }  

    public void UpdatePoolsAmount(int poolsAmount)
    {
        if (poolsAmount <= _poolsAttachment.Length * IEcsWorld.BitSize) return;
        Array.Resize(ref _poolsAttachment, _poolsAttachment.Length + 1);
        _poolsAttachment[^1] = 0;
    }
    
    public void Reset()
    {
        for (var i = 0; i < _poolsAttachment.Length; i++) _poolsAttachment[i] = 0;
        _entityId = -1;
        _exists = false;
    }

    public int GetId() => _entityId;

    public void SetId(int id) => (_entityId, _exists) = (id, true);

    public uint[] GetPoolsAttachment() => _poolsAttachment;
    
    public bool IsEmpty() => _poolsAttachment.All(t => t == 0);

    public bool Exist() => _exists;
}