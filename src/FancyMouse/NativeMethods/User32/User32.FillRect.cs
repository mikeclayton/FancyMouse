using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using FancyMouse.NativeMethods.Core;

namespace FancyMouse.NativeMethods;

internal static partial class User32
{
    [LibraryImport(Libraries.User32)]
    public static partial int FillRect(
      HDC hDC,
      ref RECT lprc,
      HBRUSH hbr);

    [SuppressMessage("Naming Rules", "SA1307:AccessibleFieldsMustBeginWithUpperCaseLetter", Justification = "Name and value taken from Win32Api")]
    public struct RECT
    {
        public LONG left;
        public LONG top;
        public LONG right;
        public LONG bottom;

        public override string ToString()
        {
            return $"left={this.left},top={this.top},right={this.right},bottom={this.bottom}";
        }
    }
}
