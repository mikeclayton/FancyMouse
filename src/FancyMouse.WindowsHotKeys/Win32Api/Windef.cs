using System.Diagnostics.CodeAnalysis;

namespace FancyMouse.WindowsHotKeys.Win32Api;

[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnusedField.Compiler")]
internal static class Windef
{

    #region Structures

    /// <summary>
    /// The POINT structure defines the x- and y-coordinates of a point.
    /// </summary>
    /// <remarks>
    /// See https://learn.microsoft.com/en-us/windows/win32/api/windef/ns-windef-point
    /// </remarks>
    public struct POINT
    {
        int X;
        int y;
    }

    #endregion

}
