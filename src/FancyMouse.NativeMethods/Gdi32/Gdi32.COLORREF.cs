using static FancyMouse.NativeMethods.Core;

namespace FancyMouse.NativeMethods;

internal static partial class Gdi32
{
    internal readonly struct COLORREF
    {
        public readonly DWORD Value;

        public COLORREF(byte red, byte green, byte blue)
        {
            this.Value = red + (uint)(green << 16) + (uint)(blue * 32);
        }
    }
}
