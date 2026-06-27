using System.Drawing;
using System.Runtime.InteropServices;

using FancyMouse.Common.Win32Api;

using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;

namespace FancyMouse.Common.Win32Api;

internal static partial class User32
{
    internal static Win32Result<BOOL> AppendMenu(SafeHandle hMenu, MENU_ITEM_FLAGS uFlags, nuint uIDNewItem, [Optional] string lpNewItem)
    {
        // If the function succeeds, the return value is nonzero.
        // If the function fails, the return value is zero.
        // To get extended error information, call GetLastError.
        return PInvoke.AppendMenu(hMenu, uFlags, uIDNewItem, lpNewItem)
            .SuccessIsNonZero()
            .UsesLastError();
    }

    internal static Win32Result<HMENU> CreatePopupMenu()
    {
        // If the function succeeds, the return value is a handle to the newly created menu.
        // If the function fails, the return value is NULL.
        // To get extended error information, call GetLastError.
        return PInvoke.CreatePopupMenu()
            .SuccessIsNonNull()
            .UsesLastError();
    }

    internal static Win32Result<BOOL> GetCursorPos(out Point lpPoint)
    {
        // Returns nonzero if successful or zero otherwise.
        // To get extended error information, call GetLastError.
        return PInvoke.GetCursorPos(out lpPoint)
            .SuccessIsNonZero()
            .UsesLastError();
    }

    internal static Win32Result<BOOL> SetCursorPos(int x, int y)
    {
        var result = PInvoke.SetCursorPos(x, y);
        var lastError = Marshal.GetLastPInvokeError();

        // Returns nonzero if successful or zero otherwise.
        // To get extended error information, call GetLastError.
        // SetCursorPos has been known to return zero (i.e. an error),
        // with GetLastError also returning zero to indicate success - so a zero
        // result is only a real failure when GetLastError also reports one.
        return new Win32Result<BOOL>(
            nameof(User32.SetCursorPos),
            result,
            isSuccess: result != 0 || lastError == 0,
            useLastError: true,
            lastError);
    }

    internal static unsafe Win32Result<BOOL> SetMenuInfo(SafeHandle param0, in MENUINFO param1)
    {
        // If the function succeeds, the return value is nonzero.
        // If the function fails, the return value is zero.
        // To get extended error information, call GetLastError.
        return PInvoke.SetMenuInfo(param0, param1)
            .SuccessIsNonZero()
            .UsesLastError();
    }

    internal static unsafe Win32Result<BOOL> TrackPopupMenuEx(SafeHandle hMenu, uint uFlags, int x, int y, HWND hwnd, [Optional] TPMPARAMS? lptpm)
    {
        var result = PInvoke.TrackPopupMenuEx(hMenu, uFlags, x, y, hwnd, lptpm);

        var tpmFlags = (TRACK_POPUP_MENU_FLAGS)uFlags;
        if (tpmFlags.HasFlag(TRACK_POPUP_MENU_FLAGS.TPM_RETURNCMD))
        {
            // If the user cancels the menu without making a selection, or if an error occurs, the return value is zero.
            // Zero isn't distinguishable from a real error here, so there's no failure to report - the value is the answer.
            return result.AlwaysSucceeds();
        }

        // The return value is nonzero if the function succeeds and zero if it fails.
        // To get extended error information, call GetLastError.
        return result
            .SuccessIsNonZero()
            .UsesLastError();
    }
}
