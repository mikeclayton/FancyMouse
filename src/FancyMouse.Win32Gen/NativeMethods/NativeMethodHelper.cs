using System.Collections.Immutable;

using Microsoft.CodeAnalysis;

namespace FancyMouse.Win32Gen.NativeMethods;

/// <summary>
/// Helper functions for CsWin32 NativeMethods files.
/// </summary>
internal static class NativeMethodHelper
{
    // CsWin32's SourceGenerator.NativeMethodsTxtAdditionalFileName - a
    // private field on their side, so duplicated here as a literal.
    private const string NativeMethodsTxtAdditionalFileName = "NativeMethods.txt";

    /// <summary>
    /// Reads the contents of the *.NativeMethods.txt file(s) in the project,
    /// using the same file-discovery mechanism as CsWin32.
    /// </summary>
    public static IncrementalValueProvider<NativeMethodsEntries> ReadNativeMethodTxts(IncrementalGeneratorInitializationContext context)
    {
        // CsWin32 matches by filename *suffix* (case-insensitively), not
        // exact name, so multi-file setups like "Foo.NativeMethods.txt" -
        // CsWin32 supports splitting the api list across several files -
        // are picked up here too, not just a file named exactly
        // "NativeMethods.txt" - see
        // https://github.com/microsoft/CsWin32/blob/79085ff58688330145e6c6b294fde3bc4b874a19/src/Microsoft.Windows.CsWin32/SourceGenerator.cs#L198
        // "public void Execute(GeneratorExecutionContext context)"
        static bool IsNativeMethodsTxtFile(string path)
            => Path.GetFileName(path)
                .EndsWith(NativeMethodHelper.NativeMethodsTxtAdditionalFileName, StringComparison.OrdinalIgnoreCase);

        var nativeMethodTxts = context.AdditionalTextsProvider
            .Where(static file => IsNativeMethodsTxtFile(file.Path));

        return nativeMethodTxts
            .Select(static (file, ct) => NativeMethodsTxtParser.Parse(file, ct).Entries.ToImmutableArray())
            .Collect()
            .Select(static (lists, _) => new NativeMethodsEntries(lists.SelectMany(list => list).ToImmutableArray()));
    }
}
