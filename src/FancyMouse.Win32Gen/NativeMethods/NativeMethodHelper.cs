using Microsoft.CodeAnalysis;

namespace FancyMouse.Win32Gen.NativeMethods;

/// <summary>
/// Helpers to discover and read CsWin32's NativeMethods.txt file(s) in the
/// current project. Mirrors the logic used by CsWin32 as much as possible.
/// Doesn't support NativeMethods.json (yet).
/// </summary>
/// <remarks>
/// Mirrors the bits of CsWin32's own <c>SourceGenerator.Execute</c> that
/// decide which AdditionalFiles it reads, so this generator sees the same
/// NativeMethods.txt file(s) CsWin32 does. Kept as its own class - rather
/// than inlined in <see cref="Win32ApiGenerator"/> - so there's a single,
/// obvious place to update if CsWin32 ever changes this convention; none of
/// this is exposed as a public api by the CsWin32 package itself, so it had
/// to be read out of their (decompiled) source rather than called directly.
/// </remarks>
internal static class NativeMethodHelper
{
    // CsWin32's SourceGenerator.NativeMethodsTxtAdditionalFileName - a
    // private field on their side, so duplicated here as a literal.
    private const string NativeMethodsTxtAdditionalFileName = "NativeMethods.txt";

    /// <summary>
    /// All AdditionalFiles CsWin32 itself would treat as NativeMethods.txt
    /// inputs. CsWin32 matches by filename *suffix* (case-insensitively),
    /// not exact name, so multi-file setups like "Foo.NativeMethods.txt"
    /// (CsWin32 supports splitting the api list across several files) are
    /// picked up here too, not just a file named exactly "NativeMethods.txt".
    /// </summary>
    /// <remarks>
    /// See https://github.com/microsoft/CsWin32/blob/79085ff58688330145e6c6b294fde3bc4b874a19/src/Microsoft.Windows.CsWin32/SourceGenerator.cs#L198
    /// ```
    /// public void Execute(GeneratorExecutionContext context)
    /// ```
    /// </remarks>
    public static IncrementalValuesProvider<AdditionalText> DiscoverNativeMethodTxts(IncrementalGeneratorInitializationContext context)
        => context.AdditionalTextsProvider
            .Where(static file => NativeMethodHelper.IsNativeMethodsFile(file.Path));

    private static bool IsNativeMethodsFile(string path)
        => Path.GetFileName(path)
            .EndsWith(NativeMethodHelper.NativeMethodsTxtAdditionalFileName, StringComparison.OrdinalIgnoreCase);
}
