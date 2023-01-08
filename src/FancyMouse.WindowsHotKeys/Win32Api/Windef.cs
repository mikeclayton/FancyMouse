using System.Diagnostics.CodeAnalysis;

namespace FancyMouse.WindowsHotKeys.Win32Api;

/// <summary>
///
/// </summary>
/// <remarks>
/// See https://learn.microsoft.com/en-us/windows/win32/api/windef/
///     https://github.com/MicrosoftDocs/sdk-api/tree/docs/sdk-api-src/content/windef
/// </remarks>
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnusedField.Compiler")]
internal static class Windef
{

    #region Learn / Windows / Apps / Win32 / Desktop Technologies / Graphics and Gaming / Windows GDI
    // see https://learn.microsoft.com/en-us/windows/win32/gdi/windows-gdi

    #region Rectangles / Rectangle Reference / Rectangle Structures
    // see https://learn.microsoft.com/en-us/windows/win32/gdi/rectangle-structures

    /// <summary>
    /// The POINT structure defines the x- and y-coordinates of a point.
    /// </summary>
    /// <remarks>
    /// See https://learn.microsoft.com/en-us/windows/win32/api/windef/ns-windef-point
    /// </remarks>
    public struct POINT
    {

        /// <summary>
        /// Specifies the x-coordinate of the point.
        /// </summary>
        int X;

        /// <summary>
        /// Specifies the y-coordinate of the point.
        /// </summary>
        int y;

    }

    #endregion

    #endregion

}
