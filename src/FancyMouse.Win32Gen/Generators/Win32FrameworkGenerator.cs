using System.Collections.Immutable;
using System.Text;
using FancyMouse.Win32Gen.Metadata;
using FancyMouse.Win32Gen.NativeMethods;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace FancyMouse.Win32Gen.Generators;

/// <summary>
/// Emits the <c>Win32Result{T}</c>/<c>Win32ReturnCode{T}</c>/
/// <c>Win32ReturnCode</c> framework types (see
/// <see cref="FrameworkTemplates"/>) into every consuming project.
/// </summary>
[Generator]
public sealed class Win32FrameworkGenerator : IIncrementalGenerator
{
    private const string OutputFolder = "Framework";

    // Win32ReturnCode_*.cs framework fragments are named after the return
    // type they're written against (e.g. "Win32ReturnCode_HDC" for HDC),
    // matching what Win32MetadataHelper.GetReturnTypes produces - so a file
    // in that shape only gets emitted if this project's own wrapped
    // functions actually return that type. Every other framework fragment
    // (Win32Result{T}.cs, Win32ReturnCode{T}.cs, the generic-only
    // Win32ReturnCode.cs core) doesn't depend on T, so it's always emitted.
    private const string PerTypeFrameworkFilePrefix = "Win32ReturnCode_";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var rootNamespace = Win32GeneratorHelpers.GetRootNamespace(context);
        var entries = Win32GeneratorHelpers.GetEntries(context);

        var winmdPaths = context.AnalyzerConfigOptionsProvider
            .Select(static (options, _) => Win32MetadataPaths.Get(options));

        var returnTypes = entries.Combine(winmdPaths)
            .Select(static (data, _) =>
            {
                var (entries, winmdPaths) = data;
                var functionNames = entries
                    .Where(static entry => entry.Kind == NativeMethodsEntryKind.ApiName)
                    .Select(static entry => entry.Name)
                    .ToImmutableArray();
                return Win32MetadataHelper.GetReturnTypes(functionNames, winmdPaths);
            });

        context.RegisterSourceOutput(
            rootNamespace.Combine(returnTypes),
            static (spc, data) =>
            {
                var (rootNamespace, returnTypes) = data;
                Win32FrameworkGenerator.Emit(spc, rootNamespace, returnTypes);
            });
    }

    private static void Emit(SourceProductionContext context, string rootNamespace, ImmutableHashSet<string> returnTypes)
    {
        foreach (var (fileName, fragment) in FrameworkTemplates.Get())
        {
            if (fileName.StartsWith(Win32FrameworkGenerator.PerTypeFrameworkFilePrefix, StringComparison.Ordinal))
            {
                var typeName = fileName.Substring(Win32FrameworkGenerator.PerTypeFrameworkFilePrefix.Length);
                if (!returnTypes.Contains(typeName))
                {
                    continue;
                }
            }

            var source = Win32GeneratorHelpers.BuildFrameworkFileSource(rootNamespace, fragment);
            context.AddSource($"{Win32FrameworkGenerator.OutputFolder}/{fileName}.g.cs", SourceText.From(source, Encoding.UTF8));
        }
    }
}
