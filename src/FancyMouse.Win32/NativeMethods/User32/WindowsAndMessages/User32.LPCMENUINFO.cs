using System.Runtime.InteropServices;

namespace FancyMouse.Win32.NativeMethods;

public static partial class User32
{
    /// <summary>
    /// Contains information about a menu.
    /// </summary>
    /// <remarks>
    /// See https://learn.microsoft.com/en-us/windows/win32/api/winuser/ns-winuser-menuinfo
    /// </remarks>
    public readonly struct LPCMENUINFO
    {
#pragma warning disable CA1051 // Do not declare visible instance fields
        public readonly IntPtr Value;
#pragma warning restore CA1051 // Do not declare visible instance fields

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
