namespace FancyMouse.Win32Gen.Metadata;

/// <summary>
/// What a name resolves to in the win32metadata .winmd.
/// </summary>
internal enum Win32MemberKind
{
    Enum,
    Constant,
    Function,

    /// <summary>
    /// A "native typedef" struct - the sealed, single-field wrapper shape
    /// win32metadata uses for handles (HWND, HBRUSH, ...) and primitives
    /// (BOOL, ...), identified by its NativeTypedefAttribute.
    /// </summary>
    Struct,

    /// <summary>
    /// A function pointer typedef (e.g. WNDPROC) - a TypeDefinition whose
    /// base type is System.MulticastDelegate.
    /// </summary>
    Delegate,
}
