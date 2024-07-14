using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using static FancyMouse.Common.NativeMethods.Core;

namespace FancyMouse.Common.NativeMethods;

internal static partial class Shell32
{
    /// <summary>
    /// Sends a message to the taskbar's status area.
    /// </summary>
    /// <returns>
    /// Returns TRUE if successful, or FALSE otherwise.
    /// If dwMessage is set to NIM_SETVERSION, the function returns TRUE if the version
    /// was successfully changed, or FALSE if the requested version is not supported.
    /// </returns>
    /// <remarks>
    /// See https://learn.microsoft.com/en-us/windows/win32/api/shellapi/ns-shellapi-notifyicondataw
    /// </remarks>
    [SuppressMessage("SA1307", "SA1307:AccessibleFieldsMustBeginWithUpperCaseLetter", Justification = "Parameter name matches Win32 api")]
    [StructLayout(LayoutKind.Sequential)]
    internal readonly struct NOTIFYICONDATAW
    {
        public readonly DWORD cbSize;
        public readonly HWND hWnd;
        public readonly UINT uID;
        public readonly UINT uFlags;
        public readonly UINT uCallbackMessage;
        public readonly HICON hIcon;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
        public readonly WCHAR[] szTip;
        public readonly DWORD dwState;
        public readonly DWORD dwStateMask;

        // [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
        // public readonly WCHAR[] szInfo;
        public readonly DUMMYUNIONNAME union;

        // [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
        // public readonly WCHAR[] szInfoTitle;
        public readonly DWORD dwInfoFlags;
        public readonly GUID guidItem;
        public readonly HICON hBalloonIcon;

        public NOTIFYICONDATAW(
            DWORD cbSize,
            HWND hWnd,
            UINT uID,
            UINT uFlags,
            UINT uCallbackMessage,
            HICON hIcon,
            WCHAR[] szTip,
            DWORD dwState,
            DWORD dwStateMask,
            WCHAR[] szInfo,
            DUMMYUNIONNAME union,
            WCHAR[] szInfoTitle,
            DWORD dwInfoFlags,
            GUID guidItem,
            HICON hBalloonIcon)
        {
            this.cbSize = cbSize;
            this.hWnd = hWnd;
            this.uID = uID;
            this.uFlags = uFlags;
            this.uCallbackMessage = uCallbackMessage;
            this.hIcon = hIcon;
            this.szTip = szTip;
            this.dwState = dwState;
            this.dwStateMask = dwStateMask;

            // this.szInfo = szInfo;
            this.union = union;

            // this.szInfoTitle = szInfoTitle;
            this.dwInfoFlags = dwInfoFlags;
            this.guidItem = guidItem;
            this.hBalloonIcon = hBalloonIcon;
        }

        public static int Size =>
            Marshal.SizeOf(typeof(POINT));

        [StructLayout(LayoutKind.Explicit)]
        public readonly struct DUMMYUNIONNAME
        {
            [FieldOffset(0)]
            public readonly UINT uTimeout;
            [FieldOffset(0)]
            public readonly UINT uVersion;

            public DUMMYUNIONNAME(UINT uTimeout)
            {
                this.uTimeout = uTimeout;
            }
        }

        public override string ToString()
        {
            return $"{this.GetType().Name}";
        }
    }
}
