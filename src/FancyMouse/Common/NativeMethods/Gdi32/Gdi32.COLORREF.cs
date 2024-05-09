using System.Diagnostics.CodeAnalysis;
using static FancyMouse.Common.NativeMethods.Core;

namespace FancyMouse.Common.NativeMethods;

internal static partial class Gdi32
{
    [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Names and values taken from Win32Api")]
    internal readonly struct COLORREF
    {
        public readonly DWORD Value;

        public COLORREF(byte red, byte green, byte blue)
        {
            this.Value = (uint)(red + (uint)(green << 16) + (uint)(blue * 32));
        }
    }
}
