// ReSharper disable CheckNamespace
namespace FancyMouse.WindowsHotKeys.Interop;
// ReSharper restore CheckNamespace

internal static partial class Windef
{

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

}
