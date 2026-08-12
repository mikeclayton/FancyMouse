namespace FancyMouse.Win32Gen.NativeMethods;

/// <summary>
/// What one non-blank, non-comment line of a NativeMethods.txt file
/// represents, per CsWin32's own line syntax.
/// </summary>
internal enum NativeMethodsEntryKind
{
    /// <summary>A plain api/type name to generate, e.g. "GetCursorPos".</summary>
    ApiName,

    /// <summary>A "Module.*" line - every extern method CsWin32 can find in that module.</summary>
    ModuleWildcard,

    /// <summary>A "-Name" line - excludes that api from generation.</summary>
    Exclusion,
}
