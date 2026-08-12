using System.Collections.Immutable;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace FancyMouse.Win32Gen.Analyzers;

/// <summary>
/// Flags direct reads of <c>Win32Result&lt;T&gt;.Value</c>/
/// <c>Win32ReturnCode&lt;T&gt;.Value</c> - callers should go through
/// <c>.GetValue()</c> instead, so a fluent chain reads as an action
/// (<c>.ThrowIfFailed().GetValue()</c>) rather than trailing off into a
/// property access.
/// </summary>
/// <remarks>
/// Generated code (the framework types themselves, which legitimately read
/// <c>this.Value</c> internally) is excluded via
/// <see cref="GeneratedCodeAnalysisFlags.None"/>, same as
/// <see cref="Win32ResultHandlingAnalyzer"/>.
/// </remarks>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class Win32ResultValuePropertyAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
        ImmutableArray.Create(Win32ResultAnalyzerDiagnostics.ValuePropertyUsed);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterOperationAction(Win32ResultValuePropertyAnalyzer.AnalyzePropertyReference, OperationKind.PropertyReference);
    }

    private static void AnalyzePropertyReference(OperationAnalysisContext context)
    {
        var propertyReference = (IPropertyReferenceOperation)context.Operation;
        var property = propertyReference.Property;

        if (property.Name != "Value")
        {
            return;
        }

        if (!Win32ResultTypeHelpers.IsWin32ResultOrReturnCodeType(property.ContainingType))
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(
            Win32ResultAnalyzerDiagnostics.ValuePropertyUsed,
            propertyReference.Syntax.GetLocation()));
    }
}
