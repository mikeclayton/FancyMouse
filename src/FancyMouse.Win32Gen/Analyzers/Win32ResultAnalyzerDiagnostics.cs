using Microsoft.CodeAnalysis;

namespace FancyMouse.Win32Gen.Analyzers;

/// <summary>
/// Diagnostics <see cref="Win32ResultHandlingAnalyzer"/> reports.
/// </summary>
internal static class Win32ResultAnalyzerDiagnostics
{
    /// <summary>
    /// Deliberately <see cref="DiagnosticSeverity.Error"/> for now - a hard
    /// stop whenever a Win32Result/Win32ReturnCode-returning call isn't
    /// chained to <c>.ThrowIfFailed()</c> or <c>.IgnoreFailure()</c>, no
    /// exceptions. The rule is known to be too strict for
    /// <c>AlwaysSucceeds()</c>-classified wrappers called via a bare
    /// <c>.GetValue()</c> or discard - that's intentional for this first
    /// pass, to get the detection machinery working end to end before
    /// relaxing it (e.g. accepting local-variable tracking, or exempting
    /// provably-infallible wrappers).
    /// </summary>
    public static readonly DiagnosticDescriptor UnhandledResult = new(
        id: "WIN32RESULT001",
        title: "Win32Result must be explicitly handled",
        messageFormat: "The result of '{0}' must be handled by chaining .ThrowIfFailed() or .IgnoreFailure()",
        category: "FancyMouse.Win32Gen.Analyzers",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>
    /// Reported by <see cref="Win32ResultValuePropertyAnalyzer"/> wherever
    /// <c>.Value</c> is read directly instead of going through
    /// <c>.GetValue()</c> - the property exists mainly so the type can be
    /// constructed/inspected internally by generated code; call sites
    /// should read as an action at the end of a fluent chain
    /// (<c>.ThrowIfFailed().GetValue()</c>).
    /// </summary>
    public static readonly DiagnosticDescriptor ValuePropertyUsed = new(
        id: "WIN32RESULT002",
        title: "Use .GetValue() instead of the .Value property",
        messageFormat: "Use .GetValue() instead of the .Value property",
        category: "FancyMouse.Win32Gen.Analyzers",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);
}
