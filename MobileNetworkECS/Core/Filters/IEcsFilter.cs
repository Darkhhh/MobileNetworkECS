using System.Collections;

namespace MobileNetworkECS.Core.Filters;

public interface IEcsFilter : IEnumerable, IEnumerator
{
    public IEcsFilter Inc<T>() where T : struct;
    
    public IEcsFilter Exc<T>() where T : struct;

    public IEcsFilter Register();

    public IEcsFilter SetIncTypes(params Type[] types);
    public IEcsFilter SetExcTypes(params Type[] types);

    public IEcsFilter EnumerateAsEntity();
    public IEcsFilter EnumerateAsEntityId();
    
    public Action? FilterWasUpdated { get; set; }
}