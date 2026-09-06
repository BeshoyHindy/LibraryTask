using System.Xml.Linq;
using Xunit;

namespace Library.ArchitectureTests;

// The compiler drops a reference nothing uses, so type-level rules cannot see a project reference
// that should not have been declared. These read the project files themselves.
public sealed class ProjectReferenceTests
{
    public static TheoryData<string, string[]> Graph =>
        new()
        {
            { "src/Library.Warmup/Library.Warmup.csproj", [] },
            { "src/Library.Shared/Library.Shared.csproj", [] },
            { "src/Modules/Catalog/Catalog.Contracts/Catalog.Contracts.csproj", [] },
            { "src/Modules/Lending/Lending.Contracts/Lending.Contracts.csproj", [] },
            { "src/Modules/Insights/Insights.Contracts/Insights.Contracts.csproj", [] },
            { "src/Modules/Catalog/Catalog/Catalog.csproj", ["Catalog.Contracts", "Library.Shared"] },
            { "src/Modules/Lending/Lending/Lending.csproj", ["Catalog.Contracts", "Lending.Contracts", "Library.Shared"] },
            {
                "src/Modules/Insights/Insights/Insights.csproj",
                ["Catalog.Contracts", "Insights.Contracts", "Lending.Contracts", "Library.Shared"]
            },
            { "src/Library.Migrations/Library.Migrations.csproj", ["Catalog", "Lending"] },
            { "src/Library.DbMigrator/Library.DbMigrator.csproj", ["Library.Migrations"] },
            { "src/Library.Service/Library.Service.csproj", ["Catalog", "Insights", "Lending", "Library.Shared"] },
            { "src/Library.Api/Library.Api.csproj", ["Catalog.Contracts", "Insights.Contracts", "Lending.Contracts"] },
            { "src/Library.AppHost/Library.AppHost.csproj", ["Library.Api", "Library.DbMigrator", "Library.Service"] },
            {
                "tests/Library.TestSupport/Library.TestSupport.csproj",
                ["Library.Api", "Library.Migrations", "Library.Service"]
            },
        };

    [Theory]
    [MemberData(nameof(Graph))]
    public void Project_ReferencesOnlyTheProjectsItIsAllowedTo(string projectPath, string[] expected)
    {
        var references = ReferencesOf(projectPath, "ProjectReference")
            .Select(Path.GetFileNameWithoutExtension)
            .Order(StringComparer.Ordinal);

        Assert.Equal(expected.Order(StringComparer.Ordinal), references);
    }

    [Fact]
    public void Api_TakesNoEntityFrameworkPackage()
    {
        var packages = ReferencesOf("src/Library.Api/Library.Api.csproj", "PackageReference");

        Assert.DoesNotContain(packages, package => package.StartsWith("Microsoft.EntityFrameworkCore", StringComparison.Ordinal));
        Assert.DoesNotContain(packages, package => package.StartsWith("Npgsql", StringComparison.Ordinal));
    }

    private static IReadOnlyList<string> ReferencesOf(string projectPath, string itemName) =>
    [
        .. XDocument.Load(Path.Combine(Repository.Root, projectPath))
            .Descendants(itemName)
            .Select(item => item.Attribute("Include")?.Value)
            .OfType<string>()
            .Select(include => include.Replace('\\', '/'))
    ];
}
