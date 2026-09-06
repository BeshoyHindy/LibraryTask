namespace Library.Shared;

[AttributeUsage(AttributeTargets.Assembly)]
public sealed class LibraryModuleAttribute(Type moduleType) : Attribute
{
    public Type ModuleType { get; } = moduleType;
}
