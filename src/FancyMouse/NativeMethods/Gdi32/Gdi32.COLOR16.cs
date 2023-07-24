using System.Diagnostics.CodeAnalysis;
using static FancyMouse.NativeMethods.Core;

namespace FancyMouse.NativeMethods;

internal static partial class Gdi32
{
    /// <remarks>
    /// See https://learn.microsoft.com/en-us/windows/win32/winprog/windows-data-types
    ///     https://learn.microsoft.com/en-us/windows/win32/directshow/working-with-16-bit-rgb
    /// </remarks>
    [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Names and values taken from Win32Api")]
    internal readonly struct COLOR16
    {
        public readonly WORD Value;

        private static readonly WORD _redMask = new(0xF800);
        private static readonly WORD _greenMask = new(0x07E0);
        private static readonly WORD _blueMask = new(0x001F);

        public COLOR16(WORD value)
        {
            this.Value = value;
        }

        public COLOR16(BYTE red, BYTE green, BYTE blue)
        {
            this.Value = (ushort)(((uint)red << 11) | ((uint)green << 5) | blue);
        }

        public static implicit operator WORD(COLOR16 value) => value.Value;

        public static implicit operator COLOR16(WORD value) => new(value);
    }
}
