using System.Diagnostics.CodeAnalysis;

namespace FancyMouse.WindowsHotKeys.Interop;

internal static class Windef
{
    /// <summary>
    /// The POINT structure defines the x- and y-coordinates of a point.
    /// </summary>
    /// <remarks>
    /// See https://learn.microsoft.com/en-us/windows/win32/api/windef/ns-windef-point
    /// </remarks>
    [SuppressMessage("SA1307", "SA1307:AccessibleFieldsMustBeginWithUpperCaseLetter", Justification = "Names match Win32 api")]
    public struct POINT
    {
        /// <summary>
        /// Specifies the x-coordinate of the point.
        /// </summary>
        public int x;

        /// <summary>
        /// Specifies the y-coordinate of the point.
        /// </summary>
        public int y;
    }
}
