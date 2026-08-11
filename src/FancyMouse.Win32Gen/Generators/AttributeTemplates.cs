namespace FancyMouse.Win32Gen.Generators;

/// <summary>
/// The declarative success/last-error/review-status attributes (e.g.
/// <c>SuccessIsNonZeroAttribute</c>), read from every embedded
/// "Attributes\{FileName}.cs" resource in this assembly - same reasoning
/// as <see cref="FrameworkTemplates"/> and <see cref="ApiWrapperTemplates"/>:
/// real, IDE-editable .cs files rather than string literals.
/// </summary>
/// <remarks>
/// Emitted into every consuming project unconditionally, same as the
/// framework types - unlike the per-return-type <c>Win32ReturnCode_*.cs</c>
/// partials, these attribute definitions don't reference any
/// CsWin32-generated type, so there's no accessibility gap to worry about
/// scoping them down for.
/// </remarks>
internal static class AttributeTemplates
{
    private const string AttributesSegment = "Attributes";

    private static readonly Lazy<IReadOnlyList<(string FileName, string Source)>> All =
        new(AttributeTemplates.LoadAll);

    public static IReadOnlyList<(string FileName, string Source)> Get()
        => AttributeTemplates.All.Value;

    private static IReadOnlyList<(string FileName, string Source)> LoadAll()
    {
        var assembly = typeof(AttributeTemplates).Assembly;
        var results = new List<(string FileName, string Source)>();
        foreach (var resourceName in assembly.GetManifestResourceNames())
        {
            var fileName = AttributeTemplates.GetFileName(resourceName);
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

    // "FancyMouse.Win32Gen.Attributes.SuccessIsNonZeroAttribute.cs" -> "SuccessIsNonZeroAttribute"
    private static string? GetFileName(string resourceName)
    {
        var parts = resourceName.Split('.');
        var attributesIndex = Array.IndexOf(parts, AttributeTemplates.AttributesSegment);
        return attributesIndex >= 0 && (attributesIndex + 1) < parts.Length
            ? parts[attributesIndex + 1]
            : null;
    }
}
