using System.Collections.Immutable;
using System.Text;
using FancyMouse.Win32Gen.ApiDocs;
using FancyMouse.Win32Gen.ApiTable;
using FancyMouse.Win32Gen.Metadata;
using FancyMouse.Win32Gen.NativeMethods;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace FancyMouse.Win32Gen.Generators;

/// <summary>
/// Reads NativeMethods.txt, and for every api name it has an
/// <see cref="ApiWrapperTemplates"/> entry for, emits a
/// <c>Win32Result</c>/<c>Win32ReturnCode</c>-flavoured wrapper method -
/// annotated with the matching win32docs xmldoc block, see
/// <see cref="ApiDocsHelper"/> - into the appropriate static class
/// (<c>User32</c>, <c>Kernel32</c>, ...).
/// </summary>
/// <remarks>
/// Api names with no template are reported via
/// <see cref="Win32ApiGeneratorDiagnostics"/> instead of acted on -
/// exclusions/wildcards are logged and ignored, and a name win32metadata
/// confirms is a real function but has no template is a build error
/// (<see cref="Win32ApiGeneratorDiagnostics.FunctionMissingTemplate"/>).
/// </remarks>
[Generator]
public sealed class Win32FunctionGenerator : IIncrementalGenerator
{
    private const string OutputFolder = "Functions";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var rootNamespace = Win32GeneratorHelpers.GetRootNamespace(context);
        var entries = Win32GeneratorHelpers.GetEntries(context);

        var metadataIndex = context.AnalyzerConfigOptionsProvider
            .Select(static (options, _) => Win32MetadataIndex.Load(Win32MetadataPaths.Get(options)));

        var docPaths = context.AnalyzerConfigOptionsProvider
            .Select(static (options, _) => ApiDocsPaths.Get(options));

        context.RegisterSourceOutput(
            entries.Combine(rootNamespace).Combine(metadataIndex).Combine(docPaths),
            static (spc, data) =>
            {
                var (((entries, rootNamespace), metadataIndex), docPaths) = data;
                Win32FunctionGenerator.Emit(spc, rootNamespace, entries, metadataIndex, docPaths);
            });
    }

    private static void Emit(
        SourceProductionContext context,
        string rootNamespace,
        ImmutableArray<NativeMethodsEntry> entries,
        Win32MetadataIndex? metadataIndex,
        ImmutableArray<string> docPaths)
    {
        // keyed by api name so the same name requested from more than one
        // NativeMethods.txt file (or duplicated within one) still only
        // produces a single wrapper method - each occurrence is still
        // logged below, just not emitted twice.
        var matched = new Dictionary<string, ApiWrapperTemplate>(StringComparer.Ordinal);

        foreach (var entry in entries)
        {
            switch (entry.Kind)
            {
                case NativeMethodsEntryKind.Exclusion:
                    context.ReportDiagnostic(Diagnostic.Create(Win32ApiGeneratorDiagnostics.ExclusionIgnored, entry.Location, entry.Name));
                    break;

                case NativeMethodsEntryKind.ModuleWildcard:
                    context.ReportDiagnostic(Diagnostic.Create(Win32ApiGeneratorDiagnostics.WildcardIgnored, entry.Location, entry.Name));
                    break;

                case NativeMethodsEntryKind.ApiName:
                    Win32FunctionGenerator.ProcessApiName(context, entry, metadataIndex, matched);
                    break;
            }
        }

        // one instance per generation pass - loads the win32docs file (and
        // caches its own per-api rendered output) once, however many
        // wrappers get matched in this pass, rather than once per api.
        var apiDocs = new ApiDocsHelper(docPaths);

        // drives the [SuccessIsXxx]/[UseLastError]/[HumanVerified]
        // attributes applied to each wrapper below.
        var apiTable = ApiTableHelper.Get();

        // one file per api, not one per class - so each generated file
        // stays small enough to actually navigate on disk, even once every
        // wrapper carries a full xmldoc block.
        foreach (var pair in matched)
        {
            var apiName = pair.Key;
            var template = pair.Value;

            // xmldocs are generated fresh from the live win32docs file on
            // every pass, not committed into the Source\Functions\*.cs
            // templates themselves - keeps the templates focused on the
            // actual wrapper logic, and doc text never goes stale relative
            // to whatever win32docs version is currently restored.
            var xmlDocs = apiDocs.GetXmlDocsForFunction(apiName);
            var attributeLines = Win32FunctionGenerator.BuildAttributeLines(apiTable, apiName);

            var parts = new List<string>();
            if (xmlDocs is not null)
            {
                parts.Add(xmlDocs);
            }

            parts.AddRange(attributeLines);
            parts.Add(template.MethodSource);
            var methodSource = string.Join("\n", parts);

            var source = Win32GeneratorHelpers.BuildClassSource(rootNamespace, template.ClassName, new[] { template with { MethodSource = methodSource } });
            context.AddSource($"{Win32FunctionGenerator.OutputFolder}/{template.ClassName}_{apiName}.g.cs", SourceText.From(source, Encoding.UTF8));
        }
    }

    // one line per attribute, immediately after the xmldocs and before the
    // method signature - an api with no ApiTable.txt entry just gets none.
    private static IReadOnlyList<string> BuildAttributeLines(FancyMouse.Win32Gen.ApiTable.ApiTable apiTable, string apiName)
    {
        if (!apiTable.TryGet(apiName, out var entry))
        {
            return Array.Empty<string>();
        }

        var lines = new List<string>();
        foreach (var kind in entry.Attributes)
        {
            var attribute = Win32FunctionGenerator.ToAttributeSyntax(kind);
            if (attribute is not null)
            {
                lines.Add(attribute);
            }
        }

        return lines;
    }

    private static string? ToAttributeSyntax(ApiAttributeKind kind)
        => kind switch
        {
            ApiAttributeKind.SuccessIsNonZero => "[SuccessIsNonZero]",
            ApiAttributeKind.SuccessIsNotNull => "[SuccessIsNotNull]",
            ApiAttributeKind.AlwaysSucceeds => "[AlwaysSucceeds]",
            ApiAttributeKind.UseLastError => "[UseLastError]",
            ApiAttributeKind.HumanVerified => "[HumanVerified]",

            // SuccessDelegateAttribute requires a method-name argument, but
            // the table doesn't carry one yet (see ApiTableParser's
            // placeholder handling of [SuccessIsCustom]) - an empty string
            // satisfies the constructor without claiming a real method
            // exists, so the attribute can still be emitted rather than
            // silently dropped, until the table can carry the real name.
            ApiAttributeKind.SuccessDelegate => "[SuccessDelegate(\"\")]",

            _ => null,
        };

    private static void ProcessApiName(
        SourceProductionContext context,
        NativeMethodsEntry entry,
        Win32MetadataIndex? metadataIndex,
        Dictionary<string, ApiWrapperTemplate> matched)
    {
        if (ApiWrapperTemplates.TryGet(entry.Name, out var template))
        {
            matched[entry.Name] = template;
            return;
        }

        if (metadataIndex is not null && metadataIndex.TryClassify(entry.Name, out var kind))
        {
            switch (kind)
            {
                // an enum, constant, native-typedef struct, or delegate
                // genuinely doesn't need a wrapper - not a gap, so nothing
                // to report.
                case Win32MemberKind.Enum:
                case Win32MemberKind.Constant:
                case Win32MemberKind.Struct:
                case Win32MemberKind.Delegate:
                    return;

                // unlike NoTemplateFound below, this is a confirmed gap:
                // win32metadata says this name really is a P/Invoke
                // function, and there's just no template for it yet.
                case Win32MemberKind.Function:
                    context.ReportDiagnostic(Diagnostic.Create(Win32ApiGeneratorDiagnostics.FunctionMissingTemplate, entry.Location, entry.Name));
                    return;
            }
        }

        context.ReportDiagnostic(Diagnostic.Create(Win32ApiGeneratorDiagnostics.NoTemplateFound, entry.Location, entry.Name));
    }
}
