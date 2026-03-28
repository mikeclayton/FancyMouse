using System.Runtime.InteropServices;
using static FancyMouse.Win32.NativeMethods.Core;

namespace FancyMouse.Win32.NativeMethods;

public static partial class User32
{
    /// <summary>
    /// Unregisters a window class, freeing the memory required for the class.
    /// </summary>
    /// <param name="lpClassName">
    /// A null-terminated string or a class atom. If lpClassName is a string, it specifies the window class name.
    /// This class name must have been registered by a previous call to the RegisterClass or RegisterClassEx function.
    /// System classes, such as dialog box controls, cannot be unregistered. If this parameter is an atom,
    /// it must be a class atom created by a previous call to the RegisterClass or RegisterClassEx function.
    /// The atom must be in the low-order word of lpClassName; the high-order word must be zero.
    /// </param>
    /// <returns>
    /// If the function succeeds, the return value is nonzero.
    /// If the class could not be found or if a window still exists that was created with the class, the return
    /// value is zero.
    /// To get extended error information, call GetLastError.
    /// </returns>
    /// <remarks>
    /// See https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-unregisterclassw
    /// </remarks>
    [DllImport(Libraries.User32, CharSet = CharSet.Unicode, SetLastError = true)]
    internal static extern BOOL UnregisterClassW(
        LPCWSTR lpClassName, HINSTANCE hInstance);
}
