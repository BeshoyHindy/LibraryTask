using ArchUnitNET.Domain;
using ArchUnitNET.Loader;
using Catalog;
using Insights;
using Lending;
using Library.Api;
using Library.Shared;
using Assembly = System.Reflection.Assembly;

namespace Library.ArchitectureTests;

internal static class Solution
{
    public static Assembly Catalog { get; } = typeof(CatalogModule).Assembly;

    public static Assembly Lending { get; } = typeof(LendingModule).Assembly;

    public static Assembly Insights { get; } = typeof(InsightsModule).Assembly;

    public static Assembly Api { get; } = typeof(ApiMarker).Assembly;

    public static Assembly Shared { get; } = typeof(IModule).Assembly;

    public static Architecture Architecture { get; } = new ArchLoader()
        .LoadAssemblies(Catalog, Lending, Insights, Api, Shared)
        .Build();
}
