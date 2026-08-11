using System.Collections.Immutable;

using MessagePack;
using MessagePack.Formatters;
using MessagePack.Resolvers;

namespace FancyMouse.Win32Gen.ApiDocs;

/// <summary>
/// Reads CsWin32's own win32docs data file (the same
/// Microsoft.Windows.SDK.Win32Docs "apidocs.msgpack" CsWin32 itself
/// consumes for its rich XML doc comments) and renders the entry for a
/// given api name as a ready-to-emit XML doc comment block, so this
/// generator's own wrapper methods can carry the same summary/remarks/
/// returns text CsWin32's own generated PInvoke declarations already have,
/// instead of losing it at the wrapper boundary.
/// </summary>
/// <remarks>
/// An instance, not a static class, deliberately: the win32docs file is
/// large (tens of thousands of entries), so it's only worth loading once
/// per generation pass, however many different api names get looked up
/// during that pass - construct one instance, call
/// <see cref="GetXmlDocsForFunction"/> as many times as needed, then let it
/// go. The underlying dictionary is loaded lazily on first use, and the
/// rendered XML doc text is cached per api name too, so a repeated lookup
/// for the same name never re-parses or re-renders anything.
/// </remarks>
internal sealed class ApiDocsHelper
{
    // internal (not private) so tests can serialize a compatible in-memory
    // fixture stream for MergeFromStream, using the same ApiDetailsFormatter
    // wiring the real win32docs file is read with.
    internal static readonly MessagePackSerializerOptions MsgPackOptions = MessagePackSerializerOptions.Standard.WithResolver(
        CompositeResolver.Create(
            new IMessagePackFormatter[] { new ApiDetailsFormatter() },
            new IFormatterResolver[] { StandardResolver.Instance }));

    private readonly ImmutableArray<string> docPaths;
    private readonly Dictionary<string, string?> renderedByApiName = new(StringComparer.Ordinal);
    private Dictionary<string, ApiDetails>? apisAndDocs;

    public ApiDocsHelper(ImmutableArray<string> docPaths)
    {
        this.docPaths = docPaths;
    }

    private ApiDocsHelper(Dictionary<string, ApiDetails> apisAndDocs)
    {
        this.docPaths = ImmutableArray<string>.Empty;
        this.apisAndDocs = apisAndDocs;
    }

    /// <summary>
    /// Test-only entry point: seeds the merged api docs directly, bypassing
    /// file loading entirely - lets <see cref="GetXmlDocsForFunction"/>'s
    /// W/A-suffix fallback and caching be tested with inline
    /// <see cref="ApiDetails"/> instances instead of a real win32docs file.
    /// </summary>
    internal static ApiDocsHelper FromEntries(Dictionary<string, ApiDetails> apisAndDocs)
        => new(apisAndDocs);

    /// <summary>
    /// Returns a ready-to-emit XML doc comment block (each line already
    /// prefixed with <c>///</c>) for the named api, or <see langword="null"/>
    /// if no documentation entry exists for it.
    /// </summary>
    public string? GetXmlDocsForFunction(string apiName)
    {
        if (this.renderedByApiName.TryGetValue(apiName, out var cached))
        {
            return cached;
        }

        var details = this.FindApiDetails(apiName);
        var rendered = details is null ? null : ApiDocXmlCommentBuilder.Build(details);
        this.renderedByApiName[apiName] = rendered;
        return rendered;
    }

    private ApiDetails? FindApiDetails(string apiName)
    {
        var all = this.apisAndDocs ??= this.LoadAll();

        if (all.TryGetValue(apiName, out var details))
        {
            return details;
        }

        // NativeMethods.txt (and this generator's own templates) often name
        // the CsWin32 "friendly" overload (e.g. "DefWindowProc"), but the
        // docs file - like the raw win32metadata - only has entries for the
        // real 'W'/'A'-suffixed function name. Same fallback as
        // Win32MetadataIndex.TryClassify and Win32MetadataHelper.
        if (all.TryGetValue(apiName + "W", out details) || all.TryGetValue(apiName + "A", out details))
        {
            return details;
        }

        return null;
    }

    private Dictionary<string, ApiDetails> LoadAll()
    {
        var merged = new Dictionary<string, ApiDetails>(StringComparer.Ordinal);
        foreach (var path in this.docPaths)
        {
            ApiDocsHelper.TryLoadInto(path, merged);
        }

        return merged;
    }

    private static void TryLoadInto(string docPath, Dictionary<string, ApiDetails> merged)
    {
        if (!File.Exists(docPath))
        {
            return;
        }

        try
        {
            using var stream = File.OpenRead(docPath);
            ApiDocsHelper.MergeFromStream(stream, merged);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or MessagePackSerializationException)
        {
            // doc lookup is a nice-to-have, not something a build should
            // fail over - just skip this file.
        }
    }

    /// <summary>
    /// Deserializes one win32docs stream and merges its entries into
    /// <paramref name="merged"/>, first-file-wins. Split out of
    /// <see cref="TryLoadInto"/> so the merge policy and the
    /// <see cref="ApiDetailsFormatter"/> wiring can be tested against a
    /// small, in-memory-built MessagePack payload instead of a real
    /// win32docs data file.
    /// </summary>
    internal static void MergeFromStream(Stream stream, Dictionary<string, ApiDetails> merged)
    {
        var data = MessagePackSerializer.Deserialize<Dictionary<string, ApiDetails>>(stream, ApiDocsHelper.MsgPackOptions);
        foreach (var pair in data)
        {
            // first docs file wins - same merge policy CsWin32's own
            // Docs.Merge uses when more than one is present (e.g. WDK
            // metadata alongside SDK metadata).
            if (!merged.ContainsKey(pair.Key))
            {
                merged[pair.Key] = pair.Value;
            }
        }
    }
}
