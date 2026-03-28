using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using static FancyMouse.Win32.NativeMethods.Core;

namespace FancyMouse.Win32.NativeMethods;

public static partial class User32
{
    /// <summary>
    /// Contains information about a menu.
    /// </summary>
    /// <remarks>
    /// See https://learn.microsoft.com/en-us/windows/win32/api/winuser/ns-winuser-menuinfo
    /// </remarks>
    [SuppressMessage("SA1307", "SA1307:AccessibleFieldsMustBeginWithUpperCaseLetter", Justification = "Names match Win32 api")]
    public readonly struct MENUINFO
    {
#pragma warning disable CA1051 // Do not declare visible instance fields
        public readonly DWORD cbSize;
        public readonly MENUINFO_MASK fMask;
        public readonly MENUINFO_STYLE dwStyle;
        public readonly UINT cyMax;
        public readonly HBRUSH hbrBack;
        public readonly DWORD dwContextHelpID;
        public readonly ULONG_PTR dwMenuData;
#pragma warning restore CA1051 // Do not declare visible instance fields

        public MENUINFO(
            DWORD cbSize,
            MENUINFO_MASK fMask,
            MENUINFO_STYLE dwStyle,
            UINT cyMax,
            HBRUSH hbrBack,
            DWORD dwContextHelpID,
            ULONG_PTR dwMenuData)
        {
            this.cbSize = cbSize;
            this.fMask = fMask;
            this.dwStyle = dwStyle;
            this.cyMax = cyMax;
            this.hbrBack = hbrBack;
            this.dwContextHelpID = dwContextHelpID;
            this.dwMenuData = dwMenuData;
        }

        public static int Size =>
            Marshal.SizeOf<MENUINFO>();
    }
}
