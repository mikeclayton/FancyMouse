using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace FancyMouse.Common.NativeMethods;

public static partial class User32
{
    /// <summary>
    /// Contains information about a menu.
    /// </summary>
    /// <remarks>
    /// See https://learn.microsoft.com/en-us/windows/win32/api/winuser/ns-winuser-menuinfo
    /// </remarks>
    internal readonly struct LPCMENUINFO
    {
        public readonly IntPtr Value;

        public LPCMENUINFO(IntPtr value)
        {
            this.Value = value;
        }

        public LPCMENUINFO(MENUINFO value)
        {
            this.Value = LPCMENUINFO.ToPtr(value);
        }

        public MENUINFO ToStructure()
        {
            return Marshal.PtrToStructure<MENUINFO>(this.Value);
        }

        private static IntPtr ToPtr(MENUINFO value)
        {
            var ptr = Marshal.AllocHGlobal(MENUINFO.Size);
            Marshal.StructureToPtr(value, ptr, false);
            return ptr;
        }

        public void Free()
        {
            Marshal.FreeHGlobal(this.Value);
        }

        public override string ToString()
        {
            return $"{this.GetType().Name}({this.Value})";
        }
    }
}
