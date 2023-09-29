using System.Collections;

namespace MobileNetworkECS.Core.Filters;

public interface IEcsRegisteredFilter : IEnumerable, IEnumerator
{
    internal Type[] GetIncTypes();

    internal Type[] GetDecTypes();

    internal void SetMasks(uint[] incMask, uint[] excMask);

    internal void CheckEntity(IRawEntity rawEntity, out bool added, out bool removed);
    
    internal void UpdatePoolsAmount(int poolsAmount);
    
    public IEcsFilter Filter { get; init; }
}