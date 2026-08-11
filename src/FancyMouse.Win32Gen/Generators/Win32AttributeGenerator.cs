using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace FancyMouse.Win32Gen.Generators;

/// <summary>
/// Emits the declarative <c>SuccessIsNonZero</c>/<c>SuccessIsNotNull</c>/
/// <c>AlwaysSucceeds</c>/<c>UseLastError</c>/<c>SuccessDelegate</c>/
/// <c>HumanVerified</c> attribute definitions (see
/// <see cref="AttributeTemplates"/>) into every consuming project,
/// unconditionally - they don't reference any CsWin32-generated type, so
/// there's no accessibility gap to scope emission around the way
/// <see cref="Win32FrameworkGenerator"/> has to for the per-return-type
/// framework partials.
/// </summary>
[Generator]
public sealed class Win32AttributeGenerator : IIncrementalGenerator
{
    private const string OutputFolder = "Attributes";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var rootNamespace = Win32GeneratorHelpers.GetRootNamespace(context);

        context.RegisterSourceOutput(
            rootNamespace,
            static (spc, ns) => Win32AttributeGenerator.Emit(spc, ns));
    }

    private static void Emit(SourceProductionContext context, string rootNamespace)
    {
        foreach (var (fileName, fragment) in AttributeTemplates.Get())
        {
            var source = Win32GeneratorHelpers.BuildFrameworkFileSource(rootNamespace, fragment);
            context.AddSource($"{Win32AttributeGenerator.OutputFolder}/{fileName}.g.cs", SourceText.From(source, Encoding.UTF8));
        }
    }
}
