// ReSharper disable CheckNamespace
using System.Diagnostics.CodeAnalysis;

namespace FancyMouse.WindowsHotKeys.Interop;
// ReSharper restore CheckNamespace

internal static class Windef
{

    /// <summary>
    /// The POINT structure defines the x- and y-coordinates of a point.
    /// </summary>
    /// <remarks>
    /// See https://learn.microsoft.com/en-us/windows/win32/api/windef/ns-windef-point
    /// </remarks>
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("ReSharper", "UnusedField.Compiler")]
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

}
