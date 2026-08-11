using System.Collections.Immutable;

using Microsoft.CodeAnalysis.Diagnostics;

namespace FancyMouse.Win32Gen.ApiDocs;

/// <summary>
/// Reads the same input CsWin32's own generator uses to find the
/// restored win32docs file(s) for the consuming project - mirrors
/// <see cref="Metadata.Win32MetadataPaths"/> exactly, just for
/// <c>CsWin32InputDocPaths</c> instead of <c>CsWin32InputMetadataPaths</c>.
/// </summary>
internal static class ApiDocsPaths
{
    private const string PropertyName = "build_property.CsWin32InputDocPaths";
    private const char Separator = '|';

    public static ImmutableArray<string> Get(AnalyzerConfigOptionsProvider options)
        => options.GlobalOptions.TryGetValue(ApiDocsPaths.PropertyName, out var value) && !string.IsNullOrEmpty(value)
            ? value.Split(ApiDocsPaths.Separator).ToImmutableArray()
            : ImmutableArray<string>.Empty;
}
