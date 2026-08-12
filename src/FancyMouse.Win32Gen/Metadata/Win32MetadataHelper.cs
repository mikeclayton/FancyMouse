using System.Collections.Immutable;

using Microsoft.CodeAnalysis.Diagnostics;

namespace FancyMouse.Win32Gen.Metadata;

/// <summary>
/// Reads the same input CsWin32's own generator uses to find the
/// win32metadata .winmd file(s) for the consuming project.
/// </summary>
/// <remarks>
/// CsWin32's build props (Microsoft.Windows.CsWin32.targets) set a
/// pipe-separated MSBuild property, <c>CsWin32InputMetadataPaths</c>, and
/// mark it a <c>CompilerVisibleProperty</c> - which makes it visible to
/// every generator running in the compilation, not just CsWin32's own.
/// Reading it here means this generator needs no metadata package
/// reference of its own: no extra dependency weight, and no risk of
/// classifying against a different win32metadata version than whatever
/// CsWin32 itself is actually generating P/Invoke wrappers from. If the
/// consuming project doesn't reference CsWin32 at all, this property is
/// simply absent - metadata-based classification becomes unavailable
/// rather than an error.
/// </remarks>
internal static class Win32MetadataHelper
{
    private const string PropertyName = "build_property.CsWin32InputMetadataPaths";
    private const char Separator = '|';

    public static ImmutableArray<string> GetWin32MetadataPaths(AnalyzerConfigOptionsProvider options)
        => options.GlobalOptions.TryGetValue(Win32MetadataHelper.PropertyName, out var value) && !string.IsNullOrEmpty(value)
            ? value.Split(Win32MetadataHelper.Separator).ToImmutableArray()
            : ImmutableArray<string>.Empty;
}
