using System.Text;
using FancyMouse.Win32Gen.CsWin32;
using FancyMouse.Win32Gen.Metadata;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace FancyMouse.Win32Gen.Generators;

/// <summary>
/// For every real P/Invoke function name <see cref="CsWin32Methods"/>
/// resolves from NativeMethods.txt (wildcards expanded, exclusions already
/// applied - CsWin32's own resolution, not this generator's), emits a
/// <c>Win32Result</c>/<c>Win32ReturnCode</c>-flavoured wrapper method for
/// every one it has an <see cref="ApiWrapperTemplates"/> entry for -
/// annotated with the same xmldoc block CsWin32 itself already attached to
/// the api's raw extern declaration - into the appropriate static class
/// (<c>User32</c>, <c>Kernel32</c>, ...).
/// </summary>
/// <remarks>
/// Api names with no template are reported via
/// <see cref="Win32ApiGeneratorDiagnostics"/> instead of acted on - a name
/// win32metadata confirms is a real function but has no template is a
/// build error (<see cref="Win32ApiGeneratorDiagnostics.FunctionMissingTemplate"/>).
/// </remarks>
[Generator]
public sealed class Win32FunctionGenerator : IIncrementalGenerator
{
    private const string OutputFolder = "Functions";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var rootNamespace = Win32GeneratorHelpers.GetRootNamespace(context);
        var cswin32Methods = CsWin32Helper.GetCsWin32Methods(context);

        var metadataIndex = context.AnalyzerConfigOptionsProvider
            .Select(static (options, _) => Win32MetadataDirectory.Load(Win32MetadataHelper.GetWin32MetadataPaths(options)));

        context.RegisterSourceOutput(
            rootNamespace.Combine(cswin32Methods).Combine(metadataIndex),
            static (spc, data) =>
            {
                var ((rootNamespace, cswin32Methods), metadataIndex) = data;
                Win32FunctionGenerator.Emit(spc, rootNamespace, cswin32Methods, metadataIndex);
            });
    }

    private static void Emit(
        SourceProductionContext context,
        string rootNamespace,
        CsWin32Methods cswin32Methods,
        Win32MetadataDirectory? metadataIndex)
    {
        // keyed by api name so the same name requested from more than one
        // NativeMethods.txt file (or duplicated within one) still only
        // produces a single wrapper method - each occurrence is still
        // logged below, just not emitted twice.
        var win32genWrappers = new Dictionary<string, ApiWrapperTemplate>(StringComparer.Ordinal);
        foreach (var csWin32MethodName in cswin32Methods.GetMethodNames())
        {
            if (Win32FunctionGenerator.TryResolveTemplate(context, csWin32MethodName, metadataIndex, out var win32genTemplate))
            {
                win32genWrappers[csWin32MethodName] = win32genTemplate;
            }
        }

        // one file per api, not one per class - so each generated file
        // stays small enough to actually navigate on disk, even once every
        // wrapper carries a full xmldoc block.
        foreach (var kvp in win32genWrappers)
        {
            var cswin32MethodName = kvp.Key;
            var win32genWrapper = kvp.Value;

            var parts = new List<string>();

            // xmldocs come straight from CsWin32's own raw extern
            // declaration for this api (already sitting in
            // cswin32Methods) instead of being independently rendered from
            // the win32docs file - one less thing to keep in sync with
            // whatever CsWin32 itself does, and it's regenerated fresh
            // every pass either way.
            if (cswin32Methods.TryGetNativeMethod(cswin32MethodName, out var nativeMethod))
            {
                var xmlDocs = nativeMethod!.TryExtractXmlDocs();
                if (xmlDocs is not null)
                {
                    parts.Add(xmlDocs);
                }
            }

            parts.Add(win32genWrapper.MethodSource);
            var methodSource = string.Join("\n", parts);

            var source = Win32GeneratorHelpers.BuildClassSource(rootNamespace, win32genWrapper.ClassName, new[] { win32genWrapper with { MethodSource = methodSource } });
            context.AddSource($"{Win32FunctionGenerator.OutputFolder}/{win32genWrapper.ClassName}_{cswin32MethodName}.g.cs", SourceText.From(source, Encoding.UTF8));
        }
    }

    // apiName comes from CsWin32's own resolved PInvoke class now, not a
    // NativeMethods.txt line, so there's no source location to point
    // diagnostics at - Location.None is the best available.
    //
    // returns the matched template instead of writing into a shared
    // dictionary itself, so the mutation happens visibly at the call site
    // rather than being hidden inside a method whose name doesn't suggest
    // it has that side effect.
    private static bool TryResolveTemplate(
        SourceProductionContext context,
        string apiName,
        Win32MetadataDirectory? metadataIndex,
        out ApiWrapperTemplate template)
    {
        if (ApiWrapperTemplates.TryGet(apiName, out template))
        {
            return true;
        }

        if (metadataIndex is not null && metadataIndex.TryGet(apiName, out var entry))
        {
            switch (entry!.Kind)
            {
                // an enum, constant, native-typedef struct, or delegate
                // genuinely doesn't need a wrapper - not a gap, so nothing
                // to report.
                case Win32MemberKind.Enum:
                case Win32MemberKind.Constant:
                case Win32MemberKind.Struct:
                case Win32MemberKind.Delegate:
                    return false;

                // unlike NoTemplateFound below, this is a confirmed gap:
                // win32metadata says this name really is a P/Invoke
                // function, and there's just no template for it yet.
                case Win32MemberKind.Function:
                    context.ReportDiagnostic(Diagnostic.Create(Win32ApiGeneratorDiagnostics.FunctionMissingTemplate, Location.None, apiName));
                    return false;
            }
        }

        context.ReportDiagnostic(Diagnostic.Create(Win32ApiGeneratorDiagnostics.NoTemplateFound, Location.None, apiName));
        return false;
    }
}
