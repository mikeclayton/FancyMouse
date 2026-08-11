using System.Collections.Immutable;
using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace FancyMouse.Win32Gen.Analyzers;

/// <summary>
/// Offers to fix a <see cref="Win32ResultAnalyzerDiagnostics.ValuePropertyUsed"/>
/// by rewriting <c>x.Value</c> to <c>x.GetValue()</c>.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(Win32ResultValuePropertyCodeFixProvider))]
[Shared]
public sealed class Win32ResultValuePropertyCodeFixProvider : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds { get; } =
        ImmutableArray.Create(Win32ResultAnalyzerDiagnostics.ValuePropertyUsed.Id);

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
        var memberAccess = node as MemberAccessExpressionSyntax
            ?? node.FirstAncestorOrSelf<MemberAccessExpressionSyntax>();
        if (memberAccess is null)
        {
            return;
        }

        context.RegisterCodeFix(
            CodeAction.Create(
                "Use .GetValue() instead",
                ct => Win32ResultValuePropertyCodeFixProvider.ReplaceWithGetValueAsync(context.Document, memberAccess, ct),
                equivalenceKey: "UseGetValue"),
            diagnostic);
    }

    private static async Task<Document> ReplaceWithGetValueAsync(
        Document document,
        MemberAccessExpressionSyntax memberAccess,
        CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root is null)
        {
            return document;
        }

        var invocation = SyntaxFactory.InvocationExpression(
            memberAccess.WithName(SyntaxFactory.IdentifierName("GetValue")));

        var newRoot = root.ReplaceNode(memberAccess, invocation);
        return document.WithSyntaxRoot(newRoot);
    }
}
