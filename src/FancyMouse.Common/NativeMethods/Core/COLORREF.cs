using System.Diagnostics.CodeAnalysis;

namespace FancyMouse.Common.NativeMethods;

internal static partial class Core
{
    /// <summary>
    /// The red, green, blue (RGB) color value (32 bits). See COLORREF for information on this type.
    /// This type is declared in WinDef.h as follows:
    /// typedef DWORD COLORREF;
    /// </summary>
    /// <remarks>
    /// See https://learn.microsoft.com/en-us/windows/win32/winprog/windows-data-types
    /// </remarks>
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
