using Microsoft.CodeAnalysis;

namespace FancyMouse.Win32Gen.Generators;

/// <summary>
/// Diagnostics <see cref="Win32FunctionGenerator"/> reports for
/// NativeMethods.txt entries it deliberately doesn't act on. Kept separate
/// from <see cref="FancyMouse.Win32Gen.NativeMethods.NativeMethodsTxtParser"/>
/// since the parser just classifies entries; deciding what to do about each
/// kind - and telling the user about it - is this generator's call, not the
/// parser's.
/// </summary>
/// <remarks>
/// Severity is deliberately mixed: <see cref="ExclusionIgnored"/>,
/// <see cref="WildcardIgnored"/>, and <see cref="NoTemplateFound"/> are
/// proof-of-concept shortcuts, not mistakes, so they stay
/// <see cref="DiagnosticSeverity.Info"/> (this repo builds with
/// TreatWarningsAsErrors, so anything higher would turn "we haven't
/// implemented this yet" into a build break). <see cref="FunctionMissingTemplate"/>
/// is different - win32metadata has confirmed the name really is a P/Invoke
/// function, so a missing wrapper there is an actual gap, not a shortcut,
/// and is <see cref="DiagnosticSeverity.Error"/> on purpose: code that
/// expects a <c>User32.Xxx()</c>-style wrapper for that function will fail
/// at compile time anyway (no such member exists), so failing the build
/// here - loudly, at the exact NativeMethods.txt line - is more useful than
/// letting it surface later as an unrelated-looking CS0117 somewhere else.
/// </remarks>
internal static class Win32ApiGeneratorDiagnostics
{
    public static readonly DiagnosticDescriptor ExclusionIgnored = new(
        id: "WIN32GEN001",
        title: "NativeMethods.txt exclusion ignored",
        messageFormat: "Ignoring exclusion '-{0}' - exclusions aren't processed by this generator yet",
        category: "FancyMouse.Win32Gen",
        DiagnosticSeverity.Info,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor WildcardIgnored = new(
        id: "WIN32GEN002",
        title: "NativeMethods.txt wildcard ignored",
        messageFormat: "Ignoring wildcard '{0}.*' - module wildcards aren't processed by this generator yet",
        category: "FancyMouse.Win32Gen",
        DiagnosticSeverity.Info,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor NoTemplateFound = new(
        id: "WIN32GEN003",
        title: "NativeMethods.txt entry has no wrapper template",
        messageFormat: "Ignoring entry '{0}' - no wrapper template found, and it isn't a recognized Win32 enum, constant, or function either (could be a struct/interface/handle type, a typo, or win32metadata just wasn't available to check)",
        category: "FancyMouse.Win32Gen",
        DiagnosticSeverity.Info,
        isEnabledByDefault: true);

    /// <summary>
    /// Distinct from <see cref="NoTemplateFound"/>: this fires only when
    /// win32metadata positively confirms the name is a real P/Invoke
    /// function, so - unlike the generic case - this always represents an
    /// actual gap in <see cref="ApiWrapperTemplates"/>, not a name that
    /// simply doesn't need a wrapper (an enum or constant) or one that
    /// couldn't be checked at all.
    /// </summary>
    public static readonly DiagnosticDescriptor FunctionMissingTemplate = new(
        id: "WIN32GEN004",
        title: "NativeMethods.txt function has no wrapper template",
        messageFormat: "No wrapper template for '{0}' - win32metadata confirms this is a real Win32 function; add a template to ApiWrapperTemplates or remove this line",
        category: "FancyMouse.Win32Gen",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);
}
