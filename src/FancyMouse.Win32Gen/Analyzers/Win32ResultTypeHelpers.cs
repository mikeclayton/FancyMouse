using Microsoft.CodeAnalysis;

namespace FancyMouse.Win32Gen.Analyzers;

/// <summary>
/// Structural matching for the generated <c>Win32Result&lt;T&gt;</c>/
/// <c>Win32ReturnCode&lt;T&gt;</c> framework types, shared by every analyzer
/// in this project. There's no single shared type to reference by fully
/// qualified name - each consuming project gets its own copy generated into
/// its own <c>{RootNamespace}.Win32Gen</c> namespace - so matching is done
/// by shape (a generic type literally named <c>Win32Result</c> or
/// <c>Win32ReturnCode</c> with one type parameter) plus the namespace's leaf
/// segment, which every generated type lands in by construction regardless
/// of which consuming project it's in.
/// </summary>
internal static class Win32ResultTypeHelpers
{
    public static bool IsWin32ResultOrReturnCodeType(ITypeSymbol? type)
        => type is INamedTypeSymbol { Arity: 1 } named
            && named.Name is "Win32Result" or "Win32ReturnCode"
            && named.ContainingNamespace is { IsGlobalNamespace: false, Name: "Win32Gen" };
}
