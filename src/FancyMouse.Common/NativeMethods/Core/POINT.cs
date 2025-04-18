using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace FancyMouse.Common.NativeMethods;

public static partial class Core
{
    /// <summary>
    /// The POINT structure defines the x- and y-coordinates of a point.
    /// </summary>
    /// <remarks>
    /// See https://learn.microsoft.com/en-us/windows/win32/api/windef/ns-windef-point
    /// </remarks>
    [SuppressMessage("SA1307", "SA1307:AccessibleFieldsMustBeginWithUpperCaseLetter", Justification = "Names match Win32 api")]
    public readonly struct POINT
    {
        /// <summary>
        /// Specifies the x-coordinate of the point.
        /// </summary>
#pragma warning disable CA1051 // Do not declare visible instance fields
        public readonly LONG x;
#pragma warning restore CA1051 // Do not declare visible instance fields

        /// <summary>
        /// Specifies the y-coordinate of the point.
        /// </summary>
#pragma warning disable CA1051 // Do not declare visible instance fields
        public readonly LONG y;
#pragma warning restore CA1051 // Do not declare visible instance fields

        public POINT(
            int x,
            int y)
        {
            this.x = x;
            this.y = y;
        }

        public POINT(
            LONG x,
            LONG y)
        {
            this.x = x;
            this.y = y;
        }

        public static int Size =>
            Marshal.SizeOf<POINT>();

        public override string ToString()
        {
            return $"x={this.x},y={this.y}";
        }
    }
}
