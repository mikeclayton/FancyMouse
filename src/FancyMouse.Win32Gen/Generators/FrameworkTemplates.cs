namespace FancyMouse.Win32Gen.Generators;

/// <summary>
/// The <c>Win32Result</c>/<c>Win32ReturnCode</c> framework types every
/// consuming project needs, read from every embedded "Framework\{FileName}.cs"
/// resource in this assembly rather than hard-coded as string literals - same
/// reasoning as <see cref="ApiWrapperTemplates"/>. Each fragment carries its
/// own using directives (a using directive can legally follow a file-scoped
/// namespace declaration) - only the namespace itself is added at generate
/// time, since that depends on the consuming project.
/// </summary>
internal static class FrameworkTemplates
{
    private const string FrameworkSegment = "Framework";

    private static readonly Lazy<IReadOnlyList<(string FileName, string Source)>> All =
        new(FrameworkTemplates.LoadAll);

    public static IReadOnlyList<(string FileName, string Source)> Get()
        => FrameworkTemplates.All.Value;

    private static IReadOnlyList<(string FileName, string Source)> LoadAll()
    {
        var assembly = typeof(FrameworkTemplates).Assembly;
        var results = new List<(string FileName, string Source)>();
        foreach (var resourceName in assembly.GetManifestResourceNames())
        {
            var fileName = FrameworkTemplates.GetFileName(resourceName);
            if (fileName is null)
            {
                continue;
            }

            using var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream is null)
            {
                continue;
            }

            using var reader = new StreamReader(stream);
            var source = reader.ReadToEnd().Replace("\r\n", "\n").Trim('\n');
            results.Add((fileName, source));
        }

        return results;
    }

    // "FancyMouse.Win32Gen.Framework.Win32Result{T}.cs" -> "Win32Result{T}"
    private static string? GetFileName(string resourceName)
    {
        var parts = resourceName.Split('.');
        var frameworkIndex = Array.IndexOf(parts, FrameworkTemplates.FrameworkSegment);
        return frameworkIndex >= 0 && (frameworkIndex + 1) < parts.Length
            ? parts[frameworkIndex + 1]
            : null;
    }
}
