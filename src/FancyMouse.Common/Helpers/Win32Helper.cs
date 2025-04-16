using System.ComponentModel;
using System.Runtime.InteropServices;

using static FancyMouse.Common.NativeMethods.Core;
using static FancyMouse.Common.NativeMethods.User32;

namespace FancyMouse.Common.Helpers;

public static class Win32Helper
{
    public static class User32
    {
        /// <summary>
        /// Returns the dots per inch (dpi) value for the specified window.
        /// </summary>
        public static UINT GetDpiForWindow(HWND hwnd)
        {
            return NativeMethods.User32.GetDpiForWindow(hwnd);
        }

        /// <summary>
        /// Retrieves information about the specified window.
        /// The function also retrieves the 32-bit (DWORD) value at the specified offset into the extra window memory.
        /// </summary>
        public static int GetWindowLong(HWND hWnd, WINDOW_LONG_PTR_INDEX nIndex)
        {
            var result = NativeMethods.User32.GetWindowLongW(hWnd, nIndex);
            if (result != 0)
            {
                return result;
            }

            // if GetWindowLongW returns 0 it *could* be an error
            var lastError = Marshal.GetLastWin32Error();
            if (lastError == 0)
            {
                // not an error - it's a legitimate "0" result
                return result;
            }

            throw new Win32Exception(lastError);
        }

        /// <summary>
        /// The MapWindowPoints function converts (maps) a set of points from a
        /// coordinate space relative to one window to a coordinate space relative to another window.
        /// </summary>
        public static POINT MapWindowPoint(HWND hWndFrom, HWND hWndTo, POINT point)
        {
            var cPoints = (UINT)1;
            var lpPoint = new LPPOINT(point);
            var result = NativeMethods.User32.MapWindowPoints(hWndFrom, hWndTo, lpPoint, cPoints);
            if (result == 0)
            {
                var error = Marshal.GetLastWin32Error();
                throw new Win32Exception(error);
            }

            var mappedPoint = lpPoint.ToStructure();
            lpPoint.Free();

            return mappedPoint;
        }

        public static void SetWindowLong(HWND hWnd, WINDOW_LONG_PTR_INDEX nIndex, LONG dwNewLong)
        {
            // clear the last error before calling SetWindowLongW
            NativeMethods.Kernel32.SetLastError(0);

            var result = NativeMethods.User32.SetWindowLongW(hWnd, nIndex, dwNewLong);

            // failure is only if SetWindowLongW returns 0 and last error is non-zero
            var lastError = Marshal.GetLastWin32Error();
            if ((result == 0) && (lastError != 0))
            {
                throw new Win32Exception(lastError);
            }
        }
    }
}
