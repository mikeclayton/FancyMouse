using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace FancyMouse.Win32Gen.CsWin32;

/// <summary>
/// One method declaration out of CsWin32's generated <c>PInvoke</c> class -
/// either the raw extern overload (<see cref="IsNativeMethod"/>) or the
/// "friendly" overload CsWin32 generates alongside it for many functions
/// (<see cref="IsFriendlyMethod"/>).
/// </summary>
/// <remarks>
/// CsWin32 emits up to two overloads per function: the friendly one has
/// converted parameter/return types (e.g. <c>SafeHandle</c> instead of a
/// raw handle struct) and its own doc comment is just an
/// <c>&lt;inheritdoc/&gt;</c> pointing at the native overload; the native
/// overload is the one actually carrying the
/// <c>[DllImport]</c>/<c>[LibraryImport]</c> attribute and the full
/// summary/param/returns/remarks xmldoc text - which is why
/// <see cref="TryExtractXmlDocs"/>, <see cref="TryGetSetLastError"/>, and
/// <see cref="GetReturnTypeName"/> only make sense called against the
/// native overload.
/// </remarks>
internal sealed class CsWin32Method
{
    // the alias CsWin32 declares at the top of each generated file
    // ("using winmdroot = global::Windows.Win32;") and qualifies all its own types with
    private const string WinmdRootAlias = "winmdroot";
    private const string WinmdRootPrefix = CsWin32Method.WinmdRootAlias + ".";
    private const string Win32RootNamespace = "Windows.Win32";

    private readonly MethodDeclarationSyntax syntax;

    internal CsWin32Method(MethodDeclarationSyntax syntax)
    {
        this.syntax = syntax;
    }

    public string Name => this.syntax.Identifier.Text;

    /// <summary>
    /// Gets a value indicating whether this is the raw extern overload - carries the real
    /// <c>[DllImport]</c>/<c>[LibraryImport]</c> attribute and xmldoc text.
    /// </summary>
    public bool IsNativeMethod => this.syntax.Modifiers.Any(SyntaxKind.ExternKeyword);

    /// <summary>
    /// Gets a value indicating whether this is the friendlier overload CsWin32 generates
    /// alongside the native one for many functions - not every function has one.
    /// </summary>
    public bool IsFriendlyMethod => !this.IsNativeMethod;

    /// <summary>
    /// Returns the fully qualified name (e.g. <c>Windows.Win32.Foundation.HWND</c>) of every
    /// CsWin32-generated type in this overload's return type and parameters, including type
    /// arguments (e.g. the <c>RECT</c> in <c>Span&lt;RECT&gt;</c>). Types outside
    /// <c>Windows.Win32</c> (<c>SafeHandle</c>, <c>string</c>, primitives) aren't included.
    /// </summary>
    /// <remarks>
    /// CsWin32 writes every one of its own types as <c>winmdroot.&lt;Namespace&gt;.&lt;Type&gt;</c>
    /// (where <c>winmdroot</c> aliases <c>global::Windows.Win32</c>), so the outermost
    /// qualified names rooted at <c>winmdroot</c> are exactly the generated types this
    /// overload uses - no metadata lookup needed.
    /// </remarks>
    public IEnumerable<string> GetWin32TypeNames()
    {
        var types = new[] { this.syntax.ReturnType }
            .Concat(this.syntax.ParameterList.Parameters.Select(parameter => parameter.Type))
            .OfType<TypeSyntax>();

        return types
            .SelectMany(type => type.DescendantNodesAndSelf().OfType<QualifiedNameSyntax>())
            .Where(name => name.Parent is not QualifiedNameSyntax)
            .Select(name => name.ToString().Replace(" ", string.Empty))
            .Where(name => name.StartsWith(CsWin32Method.WinmdRootPrefix, StringComparison.Ordinal))
            .Select(name => CsWin32Method.Win32RootNamespace + name.Substring(CsWin32Method.WinmdRootAlias.Length))
            .Distinct(StringComparer.Ordinal);
    }

    public string? TryExtractXmlDocs()
    {
        var docLines = this.syntax.GetLeadingTrivia()
            .Where(trivia => trivia.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia))
            .SelectMany(trivia => trivia.ToFullString().Replace("\r\n", "\n").Split('\n'))
            .Select(line => line.Trim())
            .Where(line => line.Length > 0)
            .ToList();

        return docLines.Count == 0 ? null : string.Join("\n", docLines);
    }

    /// <summary>
    /// Reads the <c>SetLastError</c> named argument off this method's
    /// <c>[DllImport]</c>/<c>[LibraryImport]</c> attribute.
    /// </summary>
    /// <remarks>
    /// Currently unused - kept around because it's expected to be useful
    /// again once something downstream needs to compare CsWin32's own
    /// SetLastError value against another source (the way the removed
    /// ApiTable.txt cross-check used to).
    /// </remarks>
    /// <returns>
    /// <see langword="false"/> if this method has no
    /// <c>[DllImport]</c>/<c>[LibraryImport]</c> attribute at all (which
    /// would be anomalous for <see cref="IsNativeMethod"/> - not a
    /// legitimate "default to false" case, so it isn't treated as one).
    /// Otherwise <see langword="true"/>, with <paramref name="setLastError"/>
    /// set to whatever was found - <see langword="false"/> if the
    /// <c>SetLastError</c> argument on an otherwise-present attribute is
    /// simply absent, which matches
    /// <see cref="System.Runtime.InteropServices.DllImportAttribute.SetLastError"/>'s
    /// own default.
    /// </returns>
    private bool TryGetSetLastError(out bool setLastError)
    {
        static bool IsDllOrLibraryImportAttribute(AttributeSyntax attribute)
        {
            var attributeName = attribute.Name is QualifiedNameSyntax qualified
                ? qualified.Right.Identifier.Text
                : attribute.Name.ToString();
            return attributeName is "DllImport" or "LibraryImport";
        }

        var importAttribute = this.syntax.AttributeLists
            .SelectMany(list => list.Attributes)
            .FirstOrDefault(attribute => IsDllOrLibraryImportAttribute(attribute));
        if (importAttribute is null)
        {
            // no dllimport attribute on the method, so fail
            setLastError = false;
            return false;
        }

        var setLastErrorArgument = importAttribute.ArgumentList?.Arguments
            .FirstOrDefault(argument => argument.NameEquals?.Name.Identifier.Text == "SetLastError");
        if (setLastErrorArgument is null)
        {
            // dllimport attribute found, but has no setlasterror property, so "setlasterror = false"
            setLastError = false;
            return true;
        }

        // found a setlasterror property on the dllimport attribute, so use that value
        setLastError = (setLastErrorArgument.Expression is LiteralExpressionSyntax literal)
            && literal.IsKind(SyntaxKind.TrueLiteralExpression);
        return true;
    }

    /// <summary>
    /// This method's declared return type, normalized to the name a
    /// matching <c>Win32ReturnCode_{TypeName}.cs</c> framework fragment
    /// (see <c>FrameworkTemplates</c>) is named after - e.g. the C# keyword
    /// <c>int</c> becomes <c>Int32</c>, matching
    /// <see cref="System.Reflection.PrimitiveTypeCode"/>'s own naming, which
    /// is what those fragment files were originally named against.
    /// Identifier-shaped win32 types (<c>BOOL</c>, <c>HWND</c>, ...) are
    /// returned as-is, with any namespace/alias qualification stripped.
    /// </summary>
    public string GetReturnTypeName()
    {
        var text = this.syntax.ReturnType.ToString().Trim();

        // the last segment of any qualified/aliased name ("winmdroot.Foundation.BOOL",
        // "global::System.IntPtr") is the actual type name - everything else
        // is just how this particular project happened to spell the
        // namespace/alias.
        var lastSegment = text.Split(new[] { '.', ':' }, StringSplitOptions.RemoveEmptyEntries).Last();

        return lastSegment switch
        {
            "int" => "Int32",
            "uint" => "UInt32",
            "short" => "Int16",
            "ushort" => "UInt16",
            "long" => "Int64",
            "ulong" => "UInt64",
            "byte" => "Byte",
            "sbyte" => "SByte",
            "nint" or "IntPtr" => "IntPtr",
            "nuint" or "UIntPtr" => "UIntPtr",
            _ => lastSegment,
        };
    }
}
