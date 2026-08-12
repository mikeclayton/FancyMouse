using FancyMouse.Win32Gen.Metadata;
using FancyMouse.Win32Gen.NativeMethods;
using FancyMouse.Win32Gen.Win32Docs;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Microsoft.Windows.CsWin32;

namespace FancyMouse.Win32Gen.CsWin32;

/// <summary>
/// Drives CsWin32's own <see cref="Generator"/> directly to produce real,
/// fully-resolved syntax (friendly overloads, SafeHandle types, XML doc
/// comments) for whatever a NativeMethods.txt file names - including
/// wildcard/exclusion expansion - instead of this project re-deriving any
/// of that from raw win32metadata/win32docs itself.
/// </summary>
/// <remarks>
/// Confirmed by direct testing against a real project's NativeMethods.txt
/// and <see cref="Microsoft.CodeAnalysis.Compilation"/> (see
/// <see cref="Win32FunctionGenerator"/>'s use of this, currently limited to
/// a proof-of-concept diagnostic - not yet wired into actual wrapper
/// output): "Module.*" wildcard lines correctly expand to every real
/// function in that module, "-Name" exclusions correctly suppress just
/// that one function, and no files are written to disk as a side effect -
/// <see cref="Generator.GetCompilationUnits"/> returns in-memory
/// <see cref="CompilationUnitSyntax"/> only. One nuance: CsWin32 emits two
/// overloads per function - the friendly one's own doc comment is just an
/// <c>&lt;inheritdoc/&gt;</c> pointing at the raw extern overload, which is
/// the one actually carrying the full summary/param/returns/remarks text.
/// </remarks>
internal static class CsWin32Helper
{
    /// <summary>
    /// The name <see cref="GeneratorOptions.ClassName"/> is set to below -
    /// the actual P/Invoke wrapper methods land as members of a class with
    /// this name (one per module DLL, all sharing the name as partial
    /// classes); every other generated compilation unit (handle-wrapper
    /// structs and their Equals/GetHashCode/ToString/... boilerplate,
    /// enums, delegates, ...) is not. Exposed so <see cref="CsWin32Methods"/>
    /// filtering <see cref="Generator.GetCompilationUnits"/>'s output down
    /// to just the real P/Invoke methods doesn't have to duplicate the
    /// literal.
    /// </summary>
    public const string PInvokeClassName = "PInvoke";

    /// <summary>
    /// Drives CsWin32's own <see cref="Generator"/> against this project's
    /// NativeMethods.txt and <see cref="Microsoft.CodeAnalysis.Compilation"/>,
    /// and returns just the real P/Invoke wrapper methods as a
    /// <see cref="CsWin32Methods"/>.
    /// </summary>
    /// <remarks>
    /// Resolves the metadata/docs paths, casts Compilation/ParseOptions to
    /// their C#-flavoured subtypes, and fails hard if any of that isn't
    /// available: every consuming project references CsWin32 (metadata/doc
    /// paths always resolve) and is a C# project (Compilation/ParseOptions
    /// are always the C#-flavoured subtypes), so a failure of any of these
    /// checks means something is genuinely wrong with the build, not a
    /// legitimate "this project doesn't use CsWin32" case to quietly skip
    /// past.
    /// </remarks>
    public static IncrementalValueProvider<CsWin32Methods> GetCsWin32Methods(
        IncrementalGeneratorInitializationContext context)
    {
        var entries = NativeMethodHelper.ReadNativeMethodTxts(context);

        var metadataPaths = context.AnalyzerConfigOptionsProvider
            .Select(static (options, _) => Win32MetadataHelper.GetWin32MetadataPaths(options));

        var docPaths = context.AnalyzerConfigOptionsProvider
            .Select(static (options, _) => Win32DocsHelpers.GetWin32DocsPaths(options));

        var compilationAndParseOptions = context.CompilationProvider.Combine(context.ParseOptionsProvider);

        return entries.Combine(docPaths).Combine(metadataPaths).Combine(compilationAndParseOptions)
            .Select(static (data, cancellationToken) =>
            {
                var (((entries, docPaths), metadataPaths), compilationAndParseOptions) = data;
                var (compilation, parseOptions) = compilationAndParseOptions;
                var csharpCompilation = (compilation as CSharpCompilation) ?? throw new InvalidOperationException();
                var csharpParseOptions = (parseOptions as CSharpParseOptions) ?? throw new InvalidOperationException();

                var metadataLibraryPath = metadataPaths.FirstOrDefault()
                    ?? throw new InvalidOperationException(
                        "No win32metadata .winmd path was found (CsWin32InputMetadataPaths) - does the consuming project reference CsWin32?");

                if (docPaths.IsDefaultOrEmpty)
                {
                    throw new InvalidOperationException(
                        "No win32docs path was found (CsWin32InputDocPaths) - does the consuming project reference CsWin32?");
                }

                var docs = Docs.Merge(docPaths.Select(Docs.Get).ToList());
                var cswin32CompilationUnits = CsWin32Helper.GenerateCompilationUnits(
                    metadataLibraryPath,
                    docs,
                    entries,
                    csharpCompilation,
                    csharpParseOptions,
                    cancellationToken);

                return CsWin32Methods.FromCompilationUnits(cswin32CompilationUnits);
            });
    }

    /// <summary>
    /// Replays every entry of <paramref name="nativeMethodsEntries"/>
    /// against a fresh <see cref="Generator"/> (exclusions via
    /// <see cref="Generator.AddGeneratorExclusion"/>, everything else via
    /// <see cref="Generator.TryGenerate"/>), then returns whatever
    /// <see cref="Generator.GetCompilationUnits"/> produced.
    /// </summary>
    /// <remarks>
    /// Deliberately fails hard: an entry CsWin32 can't generate - whether
    /// via a thrown <see cref="GenerationFailedException"/> (e.g. a
    /// platform-gated type <paramref name="compilation"/> doesn't declare
    /// support for) or a bare <see langword="false"/> return from
    /// <see cref="Generator.TryGenerate"/> - is not caught or skipped here.
    /// Deciding what "can't generate this one" should mean (skip with a
    /// diagnostic? fail the whole build?) belongs at the caller, not here.
    /// </remarks>
    private static Dictionary<string, CompilationUnitSyntax> GenerateCompilationUnits(
        string metadataLibraryPath,
        Docs docs,
        NativeMethodsEntries nativeMethodsEntries,
        CSharpCompilation compilation,
        CSharpParseOptions parseOptions,
        CancellationToken cancellationToken)
    {
        var options = new GeneratorOptions
        {
            ClassName = CsWin32Helper.PInvokeClassName,
            Public = true,
        };

        using var generator = new Generator(metadataLibraryPath, docs, Array.Empty<string>(), options, compilation, parseOptions);

        void TryGenerateOrThrow(string apiNameOrModuleWildcard)
        {
            if (!generator.TryGenerate(apiNameOrModuleWildcard, out _, cancellationToken))
            {
                throw new InvalidOperationException($"Generator.TryGenerate returned false for '{apiNameOrModuleWildcard}'.");
            }
        }

        foreach (var entry in nativeMethodsEntries.Entries)
        {
            switch (entry.Kind)
            {
                case NativeMethodsEntryKind.Exclusion:
                    generator.AddGeneratorExclusion(entry.Name);
                    break;

                case NativeMethodsEntryKind.ModuleWildcard:
                    // NativeMethodsEntry strips the ".*" suffix off Name -
                    // Generator.TryGenerate wants it back.
                    TryGenerateOrThrow($"{entry.Name}.*");
                    break;

                case NativeMethodsEntryKind.ApiName:
                    TryGenerateOrThrow(entry.Name);
                    break;
            }
        }

        // materialized into a dictionary (rather than returned as the lazy
        // IEnumerable GetCompilationUnits itself produces) so it's fully
        // enumerated before the `using` above disposes the generator.
        return generator.GetCompilationUnits(cancellationToken)
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }
}
