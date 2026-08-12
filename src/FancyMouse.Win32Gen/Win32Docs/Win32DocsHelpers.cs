using System.Collections.Immutable;

using Microsoft.CodeAnalysis.Diagnostics;

namespace FancyMouse.Win32Gen.Win32Docs;

/// <summary>
/// Reads the same input CsWin32's own generator uses to find the
/// restored win32docs file(s) for the consuming project - mirrors
/// <see cref="Metadata.Win32MetadataHelper"/> exactly, just for
/// <c>CsWin32InputDocPaths</c> instead of <c>CsWin32InputMetadataPaths</c>.
/// </summary>
internal static class Win32DocsHelpers
{
    private const string PropertyName = "build_property.CsWin32InputDocPaths";
    private const char Separator = '|';

    public static ImmutableArray<string> GetWin32DocsPaths(AnalyzerConfigOptionsProvider options)
        => options.GlobalOptions.TryGetValue(Win32DocsHelpers.PropertyName, out var value) && !string.IsNullOrEmpty(value)
            ? value.Split(Win32DocsHelpers.Separator).ToImmutableArray()
            : ImmutableArray<string>.Empty;
}
