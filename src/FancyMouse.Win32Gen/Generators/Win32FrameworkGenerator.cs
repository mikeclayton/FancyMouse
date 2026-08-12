using System.Collections.Immutable;
using System.Text;
using FancyMouse.Win32Gen.CsWin32;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace FancyMouse.Win32Gen.Generators;

/// <summary>
/// Emits the <c>Win32Result{T}</c>/<c>Win32ReturnCode{T}</c>/
/// <c>Win32ReturnCode</c> framework types (see
/// <see cref="FrameworkTemplates"/>) into every consuming project.
/// </summary>
/// <remarks>
/// Which per-type <c>Win32ReturnCode_{TypeName}.cs</c> fragments are needed
/// is worked out from the exact same ground truth <see cref="Win32FunctionGenerator"/>
/// itself wraps - CsWin32's own resolved P/Invoke syntax (via
/// <see cref="CsWin32Methods"/>) for whatever api names actually end up with
/// an <see cref="ApiWrapperTemplates"/> entry - rather than an independent
/// walk of the raw win32metadata file. The two used to disagree: a function
/// only pulled in implicitly by CsWin32 (never a literal NativeMethods.txt
/// line - see <see cref="Win32FunctionGenerator"/>'s own remarks) was
/// invisible to a metadata walk scoped to literal NativeMethods.txt entries,
/// so its wrapper's actual return type - already correctly resolved by
/// <see cref="Win32FunctionGenerator"/> - could end up with no matching
/// framework fragment at all.
/// </remarks>
[Generator]
public sealed class Win32FrameworkGenerator : IIncrementalGenerator
{
    private const string OutputFolder = "Framework";

    // Win32ReturnCode_*.cs framework fragments are named after the return
    // type they're written against (e.g. "Win32ReturnCode_HDC" for HDC),
    // matching what CsWin32Method.GetReturnTypeName produces - so a file
    // in that shape only gets emitted if this project's own wrapped
    // functions actually return that type. Every other framework fragment
    // (Win32Result{T}.cs, Win32ReturnCode{T}.cs, the generic-only
    // Win32ReturnCode.cs core) doesn't depend on T, so it's always emitted.
    private const string PerTypeFrameworkFilePrefix = "Win32ReturnCode_";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var rootNamespace = Win32GeneratorHelpers.GetRootNamespace(context);
        var cswin32Methods = CsWin32Helper.GetCsWin32Methods(context);

        context.RegisterSourceOutput(
            rootNamespace.Combine(cswin32Methods),
            static (spc, data) =>
            {
                var (rootNamespace, cswin32Methods) = data;
                Win32FrameworkGenerator.Emit(spc, rootNamespace, cswin32Methods);
            });
    }

    private static void Emit(
        SourceProductionContext context,
        string rootNamespace,
        CsWin32Methods cswin32Methods)
    {
        // only api names that actually end up with a real wrapper body need
        // a framework fragment - a name CsWin32 resolved but with no
        // ApiWrapperTemplates entry never calls a Win32ReturnCode extension
        // at all (Win32FunctionGenerator reports that gap itself, via
        // WIN32GEN001/WIN32GEN002 - this generator doesn't need to repeat
        // it).
        var returnTypes = ImmutableHashSet.CreateBuilder<string>(StringComparer.Ordinal);
        foreach (var apiName in cswin32Methods.GetMethodNames())
        {
            if (ApiWrapperTemplates.TryGet(apiName, out _)
                && cswin32Methods.TryGetNativeMethod(apiName, out var nativeMethod))
            {
                returnTypes.Add(nativeMethod!.GetReturnTypeName());
            }
        }

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
