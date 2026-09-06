using ArchUnitNET.Fluent.Syntax.Elements.Types;
using ArchUnitNET.xUnitV3;
using Xunit;
using Assembly = System.Reflection.Assembly;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace Library.ArchitectureTests;

public sealed class DependencyRuleTests
{
    [Fact]
    public void Api_DependsOnNoModuleRuntime() =>
        Types().That().ResideInAssembly(Solution.Api)
            .Should().NotDependOnAny(TypesOf(Solution.Catalog, Solution.Lending, Solution.Insights))
            .Check(Solution.Architecture);

    [Fact]
    public void Api_DependsOnNoEntityFramework() =>
        Types().That().ResideInAssembly(Solution.Api)
            .Should().NotDependOnAnyTypesThat()
            .ResideInNamespaceMatching(@"^Microsoft\.EntityFrameworkCore")
            .Check(Solution.Architecture);

    [Theory]
    [MemberData(nameof(ModulePairs))]
    public void Module_ReachesAnotherModuleOnlyThroughItsContracts(Assembly module, Assembly otherRuntime) =>
        Types().That().ResideInAssembly(module)
            .Should().NotDependOnAny(TypesOf(otherRuntime))
            .Check(Solution.Architecture);

    [Fact]
    public void Shared_DependsOnNoModule() =>
        Types().That().ResideInAssembly(Solution.Shared)
            .Should().NotDependOnAny(TypesOf(Solution.Catalog, Solution.Lending, Solution.Insights))
            .Check(Solution.Architecture);

    public static TheoryData<Assembly, Assembly> ModulePairs =>
        new()
        {
            { Solution.Catalog, Solution.Lending },
            { Solution.Catalog, Solution.Insights },
            { Solution.Lending, Solution.Catalog },
            { Solution.Lending, Solution.Insights },
            { Solution.Insights, Solution.Catalog },
            { Solution.Insights, Solution.Lending },
        };

    // Coverage instrumentation injects a tracker type into every assembly it rewrites, under one
    // name, so without this an assembly rule reads it as a dependency on every other assembly.
    private static GivenTypesConjunction TypesOf(Assembly assembly, params Assembly[] others) =>
        Types().That().ResideInAssembly(assembly, others)
            .And().DoNotResideInNamespaceMatching(@"^Microsoft\.CodeCoverage");
}
