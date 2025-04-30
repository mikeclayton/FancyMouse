using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;

using FancyMouse.Common.NativeMethods;

using static FancyMouse.Common.NativeMethods.Core;
using static FancyMouse.Common.NativeMethods.Shell32;
using static FancyMouse.Common.NativeMethods.User32;

namespace FancyMouse.Common.Helpers;

public sealed class TrayIcon
{
#pragma warning disable SA1310 // Field names should not contain underscore
    private const MESSAGE_TYPE WM_TRAYICON = MESSAGE_TYPE.WM_USER + 100;
#pragma warning restore SA1310 // Field names should not contain underscore

    public TrayIcon(Icon icon)
    {
        this.Icon = icon ?? throw new ArgumentNullException(nameof(icon));
    }

    public Icon Icon
    {
        get;
    }

    public void Create(HWND hWnd)
    {
        var x = Marshal.SizeOf<NOTIFYICONDATAW>();

        var iconData = new NOTIFYICONDATAW(
            cbSize: (DWORD)NOTIFYICONDATAW.Size,
            hWnd: hWnd,
            uID: 1,
            uFlags: NOTIFY_ICON_DATA_FLAGS.NIF_MESSAGE | NOTIFY_ICON_DATA_FLAGS.NIF_ICON | NOTIFY_ICON_DATA_FLAGS.NIF_TIP,
            uCallbackMessage: TrayIcon.WM_TRAYICON,
            hIcon: (HICON)this.Icon.Handle,
            szTip: "FancyMouse",
            dwState: 0,
            dwStateMask: 0,
            szInfo: string.Empty,
            union: new NOTIFYICONDATAW.DUMMYUNIONNAME(0),
            szInfoTitle: string.Empty,
            dwInfoFlags: 0,
            guidItem: GUID.Empty,
            hBalloonIcon: HICON.Null);

        var iconDataPtr = new PNOTIFYICONDATAW(iconData);
        var result = Shell32.Shell_NotifyIconW(NOTIFY_ICON_MESSAGE.NIM_ADD, iconDataPtr);
        iconDataPtr.Free();

        if (!result)
        {
            throw new InvalidOperationException();
        }
    }

    public static Icon GetTrayIconResource()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = "FancyMouse.Common.images.icon.ico";
        using var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException();
        var icon = new Icon(stream);
        return icon;
    }
}
