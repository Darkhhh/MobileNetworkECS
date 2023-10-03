using System.Collections;
using MobileNetworkECS.Core.Worlds;

namespace MobileNetworkECS.Core.Filters;

public interface IEcsFilter : IEnumerable, IEnumerator
{
    public IEcsFilter Inc<T>() where T : struct;
    
    public IEcsFilter Exc<T>() where T : struct;

    public IEcsFilter Register(IEcsWorld world);
    
    public Action? FilterWasUpdated { get; set; }
}