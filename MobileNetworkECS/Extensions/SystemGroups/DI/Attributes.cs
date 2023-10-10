namespace MobileNetworkECS.Extensions.SystemGroups.DI;

[AttributeUsage(AttributeTargets.Field)]
public class SystemGroupInjectAttribute : Attribute
{
    public string Name { get; }
    public SystemGroupInjectAttribute(string name) => Name = name;
}