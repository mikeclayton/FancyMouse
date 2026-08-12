using System.Collections.Concurrent;

namespace FancyMouse.Win32Gen.Generators;

/// <summary>
/// The metadata table the generator drives off, read lazily from the
/// "Functions\{ClassName}\{ApiName}.cs" files embedded as resources in this
/// assembly - so adding or editing a wrapper is just adding or editing a
/// real .cs file with full IDE support, not a C# string literal.
/// </summary>
internal static class ApiWrapperTemplates
{
    private const string FunctionsSegment = "Functions";

    // cheap: resource *names* only, built once from reflection metadata -
    // no resource content is read here.
    private static readonly Lazy<IReadOnlyDictionary<string, string>> ResourceNameByApiName =
        new(ApiWrapperTemplates.BuildResourceIndex);

    // read-through: resource *content* is only read the first time a given
    // api name is actually looked up, and cached from then on.
    private static readonly ConcurrentDictionary<string, ApiWrapperTemplate?> Cache =
        new(StringComparer.Ordinal);

    public static bool TryGet(string apiName, out ApiWrapperTemplate template)
    {
        template = ApiWrapperTemplates.Cache.GetOrAdd(apiName, ApiWrapperTemplates.LoadTemplate)!;
        return template is not null;
    }

    private static ApiWrapperTemplate? LoadTemplate(string apiName)
    {
        if (!ApiWrapperTemplates.ResourceNameByApiName.Value.TryGetValue(apiName, out var resourceName))
        {
            return null;
        }

        var assembly = typeof(ApiWrapperTemplates).Assembly;
        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream is null)
        {
            return null;
        }

        using var reader = new StreamReader(stream);
        var source = reader.ReadToEnd().Replace("\r\n", "\n").Trim('\n');
        var className = ApiWrapperTemplates.GetResourceSegment(resourceName, offset: 1)
            ?? throw new InvalidOperationException($"'{resourceName}' doesn't follow the Functions.<ClassName>.<ApiName>.cs resource naming convention.");

        return new ApiWrapperTemplate(className, source);
    }

    private static IReadOnlyDictionary<string, string> BuildResourceIndex()
    {
        var assembly = typeof(ApiWrapperTemplates).Assembly;
        var index = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var resourceName in assembly.GetManifestResourceNames())
        {
            var apiName = ApiWrapperTemplates.GetResourceSegment(resourceName, offset: 2);
            if (apiName is not null)
            {
                // a leading underscore is allowed purely so newly-added,
                // not-yet-reviewed wrapper files sort together on disk
                // (e.g. "_GetMessage.cs") - it isn't part of the api name
                // itself, so it's stripped before this becomes the lookup key.
                index[apiName.TrimStart('_')] = resourceName;
            }
        }

        return index;
    }

    /// <summary>
    /// Resource names follow the default MSBuild embedded-resource naming
    /// convention - RootNamespace, then the folder path and file name each
    /// dot-separated - e.g. "FancyMouse.Win32Gen.Functions.User32.GetCursorPos.cs".
    /// Locating the "Functions" segment (rather than assuming a fixed prefix
    /// length) keeps this independent of what the root namespace happens to be.
    /// </summary>
    private static string? GetResourceSegment(string resourceName, int offset)
    {
        var parts = resourceName.Split('.');
        var functionsIndex = Array.IndexOf(parts, ApiWrapperTemplates.FunctionsSegment);
        return functionsIndex >= 0 && (functionsIndex + offset) < parts.Length
            ? parts[functionsIndex + offset]
            : null;
    }
}
