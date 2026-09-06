using System.Reflection;
using Library.Service;
using Library.Shared;
using Xunit;

namespace Library.ArchitectureTests;

public sealed class ModuleRegistrationTests
{
    [Fact]
    public void ModuleAssemblies_HoldEveryRuntimeProjectUnderModules()
    {
        var projects = Repository.ModuleRuntimeProjects()
            .Select(Path.GetFileNameWithoutExtension)
            .Order(StringComparer.Ordinal);

        var registered = ModuleAssemblies.All
            .Select(assembly => assembly.GetName().Name)
            .Order(StringComparer.Ordinal);

        Assert.Equal(projects, registered);
    }

    [Fact]
    public void ModuleAssemblies_EachDeclareTheirModule()
    {
        var modules = ModuleAssemblies.All
            .Select(assembly => assembly.GetCustomAttribute<LibraryModuleAttribute>()?.ModuleType);

        Assert.All(modules, module => Assert.True(module is not null && module.IsAssignableTo(typeof(IModule))));
    }
}
