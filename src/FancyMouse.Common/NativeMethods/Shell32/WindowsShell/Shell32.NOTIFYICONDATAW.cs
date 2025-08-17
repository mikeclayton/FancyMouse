using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

using static FancyMouse.Common.NativeMethods.Core;
using static FancyMouse.Common.NativeMethods.User32;

namespace FancyMouse.Common.NativeMethods;

public static partial class Shell32
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
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal readonly struct NOTIFYICONDATAW
    {
        public readonly DWORD cbSize;
        public readonly HWND hWnd;
        public readonly UINT uID;
        public readonly NOTIFY_ICON_DATA_FLAGS uFlags;
        public readonly MESSAGE_TYPE uCallbackMessage;
        public readonly HICON hIcon;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
        public readonly WCHAR[] szTip;
        public readonly NOTIFY_ICON_STATE dwState;
        public readonly NOTIFY_ICON_STATE dwStateMask;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
        public readonly WCHAR[] szInfo;
        public readonly DUMMYUNIONNAME union;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
        public readonly WCHAR[] szInfoTitle;
        public readonly NOTIFY_ICON_INFOTIP_FLAGS dwInfoFlags;
        public readonly GUID guidItem;
        public readonly HICON hBalloonIcon;

        public NOTIFYICONDATAW(
            DWORD cbSize,
            HWND hWnd,
            UINT uID,
            NOTIFY_ICON_DATA_FLAGS uFlags,
            MESSAGE_TYPE uCallbackMessage,
            HICON hIcon,
            string szTip,
            NOTIFY_ICON_STATE dwState,
            NOTIFY_ICON_STATE dwStateMask,
            string szInfo,
            DUMMYUNIONNAME union,
            string szInfoTitle,
            NOTIFY_ICON_INFOTIP_FLAGS dwInfoFlags,
            GUID guidItem,
            HICON hBalloonIcon)
            : this(
                cbSize,
                hWnd,
                uID,
                uFlags,
                uCallbackMessage,
                hIcon,
                WCHAR.AsNullTerminatedArray(szTip, 128),
                dwState,
                dwStateMask,
                WCHAR.AsNullTerminatedArray(szInfo, 256),
                union,
                WCHAR.AsNullTerminatedArray(szInfoTitle, 64),
                dwInfoFlags,
                guidItem,
                hBalloonIcon)
        {
        }

        public NOTIFYICONDATAW(
            DWORD cbSize,
            HWND hWnd,
            UINT uID,
            NOTIFY_ICON_DATA_FLAGS uFlags,
            MESSAGE_TYPE uCallbackMessage,
            HICON hIcon,
            WCHAR[] szTip,
            NOTIFY_ICON_STATE dwState,
            NOTIFY_ICON_STATE dwStateMask,
            WCHAR[] szInfo,
            DUMMYUNIONNAME union,
            WCHAR[] szInfoTitle,
            NOTIFY_ICON_INFOTIP_FLAGS dwInfoFlags,
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
            this.szInfo = szInfo;
            this.union = union;
            this.szInfoTitle = szInfoTitle;
            this.dwInfoFlags = dwInfoFlags;
            this.guidItem = guidItem;
            this.hBalloonIcon = hBalloonIcon;
        }

        public static int Size =>
            Marshal.SizeOf<NOTIFYICONDATAW>();

        [StructLayout(LayoutKind.Explicit)]
        public readonly struct DUMMYUNIONNAME
        {
            /// <summary>
            /// The timeout value, in milliseconds, for notification. The system enforces minimum and maximum timeout values.
            /// Values specified in uTimeout that are too large are set to the maximum value. Values that are too small
            /// default to the minimum value. The system minimum and maximum timeout values are currently set at 10 seconds
            /// and 30 seconds, respectively.
            /// </summary>
            [FieldOffset(0)]
            public readonly UINT uTimeout;

            /// <summary>
            /// (deprecated as of Windows Vista). Specifies which version of the Shell notification icon interface should
            /// be used. For more information on the differences in these versions, see Shell_NotifyIcon. This member is
            /// employed only when using Shell_NotifyIcon to send an NIM_SETVERSION message.
            /// </summary>
            [FieldOffset(0)]
            [Obsolete("Deprecated as of Windows Vista")]
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
