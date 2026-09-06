namespace Library.ArchitectureTests;

// The project files are the subject of these rules, so the tests read them from the working tree
// rather than from anything the build copies next to the assembly.
internal static class Repository
{
    public static string Root { get; } = FindRoot();

    public static IEnumerable<string> ModuleRuntimeProjects() =>
        Directory.EnumerateFiles(Path.Combine(Root, "src", "Modules"), "*.csproj", SearchOption.AllDirectories)
            .Where(project => !Path.GetFileNameWithoutExtension(project).EndsWith(".Contracts", StringComparison.Ordinal));

    private static string FindRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null && !directory.EnumerateFiles("*.slnx").Any())
        {
            directory = directory.Parent;
        }

        return directory?.FullName
            ?? throw new InvalidOperationException($"No .slnx file was found above {AppContext.BaseDirectory}.");
    }
}
