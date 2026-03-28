using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using static FancyMouse.Win32.NativeMethods.Core;

namespace FancyMouse.Win32.NativeMethods;

public static partial class User32
{
    /// <summary>
    /// Defines the initialization parameters passed to the window procedure of an application.
    /// These members are identical to the parameters of the CreateWindowEx function.
    /// </summary>
    /// <remarks>
    /// See https://learn.microsoft.com/en-us/windows/win32/api/winuser/ns-winuser-createstructw
    /// </remarks>
    [SuppressMessage("SA1307", "SA1307:AccessibleFieldsMustBeginWithUpperCaseLetter", Justification = "Names match Win32 api")]
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public readonly struct CREATESTRUCTW
    {
#pragma warning disable CA1051 // Do not declare visible instance fields
        public readonly LPVOID lpCreateParams;
        public readonly HINSTANCE hInstance;
        public readonly HMENU hMenu;
        public readonly HWND hwndParent;
        public readonly int cy;
        public readonly int cx;
        public readonly int y;
        public readonly int x;
        public readonly WINDOW_STYLE style;
        public readonly PCWSTR lpszName;
        public readonly PCWSTR lpszClass;
        public readonly WINDOW_EX_STYLE dwExStyle;
#pragma warning restore CA1051 // Do not declare visible instance fields

        public CREATESTRUCTW(
            LPVOID lpCreateParams,
            HINSTANCE hInstance,
            HMENU hMenu,
            HWND hwndParent,
            int cy,
            int cx,
            int y,
            int x,
            WINDOW_STYLE style,
            PCWSTR lpszName,
            PCWSTR lpszClass,
            WINDOW_EX_STYLE dwExStyle)
        {
            this.lpCreateParams = lpCreateParams;
            this.hInstance = hInstance;
            this.hMenu = hMenu;
            this.hwndParent = hwndParent;
            this.cy = cy;
            this.cx = cx;
            this.y = y;
            this.x = x;
            this.style = style;
            this.lpszName = lpszName;
            this.lpszClass = lpszClass;
            this.dwExStyle = dwExStyle;
        }

        public static CREATESTRUCTW ToStructure(IntPtr value)
        {
            return Marshal.PtrToStructure<CREATESTRUCTW>(value);
        }

        private static IntPtr ToPtr(CREATESTRUCTW value)
        {
            var ptr = Marshal.AllocHGlobal(CREATESTRUCTW.Size);
            Marshal.StructureToPtr(value, ptr, false);
            return ptr;
        }

        public static int Size =>
            Marshal.SizeOf<CREATESTRUCTW>();
    }
}
