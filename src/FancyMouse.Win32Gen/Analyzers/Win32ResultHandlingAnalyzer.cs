using System.Collections.Immutable;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace FancyMouse.Win32Gen.Analyzers;

/// <summary>
/// Flags any call to a method returning <c>Win32Result&lt;T&gt;</c> or
/// <c>Win32ReturnCode&lt;T&gt;</c> that isn't chained to
/// <c>.ThrowIfFailed()</c> or <c>.IgnoreFailure()</c> - the two ways this
/// framework lets a caller declare "I've considered whether this could
/// fail", as opposed to silently discarding a result nobody looked at.
/// </summary>
/// <remarks>
/// <c>Win32Result&lt;T&gt;</c> is regenerated per consuming project (see
/// FancyMouse.Win32Gen's own design - there's no single shared type to
/// reference), so matching is structural: a generic type literally named
/// <c>Win32Result</c> or <c>Win32ReturnCode</c> with one type parameter,
/// wherever it lives.
///
/// Deliberately scoped to a single generation pass's worth of complexity:
/// only direct fluent chaining is understood
/// (<c>X().ThrowIfFailed()</c>/<c>X().IgnoreFailure()</c>, or the whole
/// chain being <c>return</c>ed or passed as an argument, so the obligation
/// explicitly transfers to the caller). Assigning to a local and calling
/// the terminal method in a later, separate statement isn't recognised
/// yet - see <see cref="Win32ResultAnalyzerDiagnostics.UnhandledResult"/>
/// for the rest of what's intentionally left for a later pass. Generated
/// code (the wrapper methods themselves - which legitimately return a
/// result without handling it, since handling it is the caller's job) is
/// excluded via <see cref="GeneratedCodeAnalysisFlags.None"/>.
/// </remarks>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class Win32ResultHandlingAnalyzer : DiagnosticAnalyzer
{
    // ThrowIfFailed()/IgnoreFailure() ARE the act of handling - calling
    // either one, on any receiver, satisfies the rule regardless of what
    // happens to their own return value afterward.
    private static readonly ImmutableHashSet<string> TerminalMethodNames = ImmutableHashSet.Create(
        StringComparer.Ordinal,
        "ThrowIfFailed",
        "IgnoreFailure");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
        ImmutableArray.Create(Win32ResultAnalyzerDiagnostics.UnhandledResult);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterOperationAction(Win32ResultHandlingAnalyzer.AnalyzeInvocation, OperationKind.Invocation);
    }

    private static void AnalyzeInvocation(OperationAnalysisContext context)
    {
        var invocation = (IInvocationOperation)context.Operation;
        var method = invocation.TargetMethod;

        // ThrowIfFailed()/IgnoreFailure() calls are themselves exempt -
        // invoking one of them is what satisfies the rule for whatever
        // came before it in the chain; the call itself needs no further
        // handling regardless of its own (matching) return type.
        if (Win32ResultHandlingAnalyzer.TerminalMethodNames.Contains(method.Name))
        {
            return;
        }

        if (!Win32ResultHandlingAnalyzer.ReturnsWin32ResultOrReturnCode(method))
        {
            return;
        }

        if (Win32ResultHandlingAnalyzer.IsHandled(invocation))
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(
            Win32ResultAnalyzerDiagnostics.UnhandledResult,
            invocation.Syntax.GetLocation(),
            method.Name));
    }

    private static bool IsHandled(IInvocationOperation invocation)
    {
        IOperation current = invocation;

        while (true)
        {
            var parent = current.Parent;

            switch (parent)
            {
                // returned (including expression-bodied members) - the
                // caller inherits the obligation to handle it.
                case IReturnOperation:
                    return true;

                // passed onward as an argument - some other method is now
                // responsible for it.
                case IArgumentOperation:
                    return true;

                case IInvocationOperation chained:
                    if (Win32ResultHandlingAnalyzer.TerminalMethodNames.Contains(chained.TargetMethod.Name))
                    {
                        return true;
                    }

                    if (Win32ResultHandlingAnalyzer.ReturnsWin32ResultOrReturnCode(chained.TargetMethod))
                    {
                        // still inside the fluent chain (SuccessIsNonZero,
                        // WithLastError, WithSuccess, AsWin32Result, ...) -
                        // keep walking to see where it actually ends up.
                        current = chained;
                        continue;
                    }

                    // chained onward to something outside the fluent
                    // vocabulary (e.g. .GetValue(), which returns a plain
                    // T) without ever reaching a terminal call first.
                    return false;

                default:
                    return false;
            }
        }
    }

    // Deliberately scoped to calls *into* the generated Win32Gen surface,
    // not to every Win32Result/Win32ReturnCode-shaped value anywhere in
    // the codebase - a hand-written method that happens to return
    // Win32Result<T> (e.g. one that already handles the underlying api
    // call internally and forwards the outcome onward as a deliberate
    // design choice) isn't something this analyzer should second-guess.
    // Every generated type - the wrapper classes and the fluent framework
    // methods (ThrowIfFailed, SuccessIsNonZero, ...) alike - lands in the
    // same "{RootNamespace}.Win32Gen" namespace by construction, so
    // checking the called method's own immediate namespace segment is
    // enough to identify "this call is into generated code" at every
    // layer, not just the initial wrapper call.
    private static bool ReturnsWin32ResultOrReturnCode(IMethodSymbol method)
        => method.ReturnType is INamedTypeSymbol { Arity: 1 } named
            && named.Name is "Win32Result" or "Win32ReturnCode"
            && method.ContainingType?.ContainingNamespace is { IsGlobalNamespace: false, Name: "Win32Gen" };
}
