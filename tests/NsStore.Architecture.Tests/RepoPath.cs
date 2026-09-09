namespace NsStore.Architecture.Tests;

/// <summary>
/// Locates the repository root from the test output directory, for the few rules that can only be
/// checked against source text rather than compiled metadata.
/// </summary>
internal static class RepoPath
{
    public static string Root { get; } = Find();

    public static string Read(string relativePath) =>
        File.ReadAllText(Path.Combine(Root, relativePath.Replace('/', Path.DirectorySeparatorChar)));

    private static string Find()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);

        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "NsStore.slnx")))
        {
            dir = dir.Parent;
        }

        return dir?.FullName
            ?? throw new InvalidOperationException(
                "Could not find NsStore.slnx walking up from the test output directory. "
                + "If the solution file was renamed, update RepoPath.");
    }
}
