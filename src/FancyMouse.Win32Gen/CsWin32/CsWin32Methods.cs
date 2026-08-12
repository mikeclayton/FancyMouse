using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace FancyMouse.Win32Gen.CsWin32;

/// <summary>
/// The real P/Invoke wrapper methods CsWin32 resolved for a project - the
/// <see cref="CsWin32Helper.PInvokeClassName"/> class CsWin32 names via
/// <see cref="GeneratorOptions.ClassName"/> - filtered out of
/// <see cref="CsWin32Helper.GetCsWin32Methods"/>'s output.
/// Every other compilation unit is a supporting type (handle-wrapper
/// structs and their own Equals/GetHashCode/ToString/... boilerplate,
/// enums, delegates, ...) that no caller asked for by name, pulled in only
/// because some requested function references it.
/// </summary>
/// <remarks>
/// A collection of <see cref="CsWin32Method"/> - a raw
/// <see cref="MethodDeclarationSyntax"/> wrapper - rather than the syntax
/// nodes themselves, so callers work against named lookups
/// (<see cref="GetMethodNames"/>, <see cref="GetMethodsByName"/>) instead of
/// walking CsWin32's generated syntax shape directly.
/// </remarks>
internal sealed class CsWin32Methods
{
    private readonly IReadOnlyList<CsWin32Method> methods;

    internal CsWin32Methods(IReadOnlyList<CsWin32Method> methods)
    {
        this.methods = methods;
    }

    public static CsWin32Methods FromCompilationUnits(IReadOnlyDictionary<string, CompilationUnitSyntax> cswin32CompilationUnits)
    {
        var methods = cswin32CompilationUnits.Values
            .SelectMany(unit => unit.DescendantNodes().OfType<ClassDeclarationSyntax>())
            .Where(classDeclaration => classDeclaration.Identifier.Text == CsWin32Helper.PInvokeClassName)
            .SelectMany(classDeclaration => classDeclaration.Members.OfType<MethodDeclarationSyntax>())
            .Select(method => new CsWin32Method(method))
            .ToList();

        return new CsWin32Methods(methods);
    }

    /// <summary>
    /// Every distinct api name CsWin32 resolved, ordered for stable output.
    /// </summary>
    public IReadOnlyList<string> GetMethodNames()
        => this.methods
            .Select(method => method.Name)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToList();

    /// <summary>
    /// Every overload CsWin32 generated for <paramref name="methodName"/> -
    /// typically the native overload alone, or the native and friendly
    /// overloads together (see <see cref="CsWin32Method.IsNativeMethod"/>/
    /// <see cref="CsWin32Method.IsFriendlyMethod"/>).
    /// </summary>
    public IEnumerable<CsWin32Method> GetMethodsByName(string methodName)
        => this.methods.Where(method => method.Name == methodName);

    /// <summary>
    /// Convenience over <see cref="GetMethodsByName"/> for the common case
    /// of wanting specifically the native overload - the one carrying the
    /// xmldoc text and <c>[DllImport]</c>/<c>[LibraryImport]</c> attribute
    /// callers actually need to inspect.
    /// </summary>
    public bool TryGetNativeMethod(string methodName, out CsWin32Method? nativeMethod)
    {
        nativeMethod = this.GetMethodsByName(methodName).FirstOrDefault(method => method.IsNativeMethod);
        return nativeMethod is not null;
    }
}
