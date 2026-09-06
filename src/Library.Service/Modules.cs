using System.Reflection;
using Library.Shared;

namespace Library.Service;

public static class Modules
{
    public static void AddModules(this IHostApplicationBuilder builder, IReadOnlyList<Assembly> assemblies)
    {
        ArgumentNullException.ThrowIfNull(assemblies);

        foreach (var module in ModulesOf(assemblies))
        {
            module.AddServices(builder);
        }
    }

    public static void MapModules(this IEndpointRouteBuilder endpoints, IReadOnlyList<Assembly> assemblies)
    {
        ArgumentNullException.ThrowIfNull(assemblies);

        foreach (var module in ModulesOf(assemblies))
        {
            module.MapEndpoints(endpoints);
        }
    }

    private static IEnumerable<IModule> ModulesOf(IReadOnlyList<Assembly> assemblies)
    {
        foreach (var assembly in assemblies)
        {
            var attribute = assembly.GetCustomAttribute<LibraryModuleAttribute>()
                ?? throw new InvalidOperationException($"{assembly.GetName().Name} carries no LibraryModule attribute.");

            yield return Activator.CreateInstance(attribute.ModuleType) as IModule
                ?? throw new InvalidOperationException($"{attribute.ModuleType.Name} is not an IModule.");
        }
    }
}
