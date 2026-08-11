using System.Collections.Immutable;

namespace FancyMouse.Win32Gen.ApiTable;

/// <summary>
/// Reads the embedded <c>ApiTable.txt</c> resource and returns a cached,
/// parsed <see cref="ApiTable"/> instance - loaded and parsed once per
/// analyzer load, not once per lookup.
/// </summary>
internal static class ApiTableHelper
{
    private static readonly Lazy<ApiTable> Cached = new(ApiTableHelper.Load);

    public static ApiTable Get()
        => ApiTableHelper.Cached.Value;

    private static ApiTable Load()
    {
        var assembly = typeof(ApiTableHelper).Assembly;
        var resourceName = assembly.GetManifestResourceNames()
            .FirstOrDefault(static name => name.EndsWith(".ApiTable.txt", StringComparison.Ordinal));

        if (resourceName is null)
        {
            return new ApiTable(ImmutableDictionary<string, ApiEntry>.Empty);
        }

        using var stream = assembly.GetManifestResourceStream(resourceName)!;
        using var reader = new StreamReader(stream);
        var text = reader.ReadToEnd();

        return ApiTableParser.Parse(text);
    }
}
