using System.Collections.Immutable;
using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace FancyMouse.Win32Gen.Analyzers;

/// <summary>
/// Offers to fix a <see cref="Win32ResultAnalyzerDiagnostics.UnhandledResult"/>
/// by inserting <c>.ThrowIfFailed()</c> or <c>.IgnoreFailure()</c>
/// immediately after the flagged api call - not necessarily at the end of
/// whatever chain it's part of, so e.g. <c>X().GetValue()</c> becomes
/// <c>X().ThrowIfFailed().GetValue()</c> rather than appending after
/// <c>GetValue()</c>.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(Win32ResultHandlingCodeFixProvider))]
[Shared]
public sealed class Win32ResultHandlingCodeFixProvider : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds { get; } =
        ImmutableArray.Create(Win32ResultAnalyzerDiagnostics.UnhandledResult.Id);

    public override FixAllProvider GetFixAllProvider()
        => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root is null)
        {
            return;
        }

        var diagnostic = context.Diagnostics[0];
        var node = root.FindNode(diagnostic.Location.SourceSpan, getInnermostNodeForTie: true);
        var invocation = node as InvocationExpressionSyntax
            ?? node.FirstAncestorOrSelf<InvocationExpressionSyntax>();
        if (invocation is null)
        {
            return;
        }

        context.RegisterCodeFix(
            CodeAction.Create(
                "Chain .ThrowIfFailed()",
                ct => Win32ResultHandlingCodeFixProvider.AppendCallAsync(context.Document, invocation, "ThrowIfFailed", ct),
                equivalenceKey: "ThrowIfFailed"),
            diagnostic);

        context.RegisterCodeFix(
            CodeAction.Create(
                "Chain .IgnoreFailure()",
                ct => Win32ResultHandlingCodeFixProvider.AppendCallAsync(context.Document, invocation, "IgnoreFailure", ct),
                equivalenceKey: "IgnoreFailure"),
            diagnostic);
    }

    private static async Task<Document> AppendCallAsync(
        Document document,
        InvocationExpressionSyntax invocation,
        string methodName,
        CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root is null)
        {
            return document;
        }

        // wraps just the flagged invocation - if it's already the receiver
        // of a further chained call (e.g. "X().GetValue()"), that outer
        // call is left in place around the new one, so the fix lands
        // immediately after the api call rather than at the end of the
        // whole expression.
        var wrapped = SyntaxFactory.InvocationExpression(
            SyntaxFactory.MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                invocation,
                SyntaxFactory.IdentifierName(methodName)));

        var newRoot = root.ReplaceNode(invocation, wrapped);
        return document.WithSyntaxRoot(newRoot);
    }
}
