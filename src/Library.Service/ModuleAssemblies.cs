using System.Reflection;
using Catalog;
using Insights;
using Lending;

namespace Library.Service;

public static class ModuleAssemblies
{
    public static IReadOnlyList<Assembly> All { get; } =
    [
        typeof(CatalogModule).Assembly,
        typeof(LendingModule).Assembly,
        typeof(InsightsModule).Assembly,
    ];
}
