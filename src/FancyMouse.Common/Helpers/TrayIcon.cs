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
    public const MESSAGE_TYPE WM_TRAYICON_SHOWCONTEXTMENU = MESSAGE_TYPE.WM_USER + 100;
    public const MESSAGE_TYPE WM_TRAYICON_EXITCOMMAND = MESSAGE_TYPE.WM_USER + 101;
#pragma warning restore SA1310 // Field names should not contain underscore

    public TrayIcon()
    {
    }

    private Icon? Icon
    {
        get;
        set;
    }

    private (ATOM ClassAtom, HWND Hwnd)? Window
    {
        get;
        set;
    }

    private HMENU Menu
    {
        get;
        set;
    }

    public void Initialize()
    {
        var window = Win32Helper.User32.CreateMessageOnlyWindow(
            className: "FancyMouseTrayIconClass",
            windowName: "FancyMouseTrayIconWindow",
            wndProc: this.TrayIconWndProc);
        this.Window = window;

        var hIcon = TrayIcon.GetTrayIconResource();
        this.Icon = hIcon;

        TrayIcon.InitializeTrayIcon(window.Hwnd, (HICON)hIcon.Handle);
        this.Menu = TrayIcon.InitializeTrayMenu(window.Hwnd);
    }

    private static Icon GetTrayIconResource()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = "FancyMouse.Common.images.icon.ico";
        using var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException();
        var icon = new Icon(stream);
        return icon;
    }

    private static void InitializeTrayIcon(HWND hWnd, HICON hIcon)
    {
        var notifyIconData = new NOTIFYICONDATAW(
            cbSize: (DWORD)NOTIFYICONDATAW.Size,
            hWnd: hWnd,
            uID: 1,
            uFlags: NOTIFY_ICON_DATA_FLAGS.NIF_MESSAGE | NOTIFY_ICON_DATA_FLAGS.NIF_ICON | NOTIFY_ICON_DATA_FLAGS.NIF_TIP,
            uCallbackMessage: TrayIcon.WM_TRAYICON_SHOWCONTEXTMENU,
            hIcon: hIcon,
            szTip: "FancyMouse",
            dwState: 0,
            dwStateMask: 0,
            szInfo: string.Empty,
            union: new NOTIFYICONDATAW.DUMMYUNIONNAME(0),
            szInfoTitle: string.Empty,
            dwInfoFlags: 0,
            guidItem: GUID.Empty,
            hBalloonIcon: HICON.Null);
        var iconDataPtr = new PNOTIFYICONDATAW(notifyIconData);
        var result = Shell32.Shell_NotifyIconW(NOTIFY_ICON_MESSAGE.NIM_ADD, iconDataPtr);
        iconDataPtr.Free();
        if (!result)
        {
            throw new InvalidOperationException();
        }
    }

    private static HMENU InitializeTrayMenu(HWND hWnd)
    {
        var hMenu = User32.CreatePopupMenu();
        if (hMenu.IsNull)
        {
            throw Win32Helper.NewWin32Exception((UINT)hMenu.Value, Marshal.GetLastPInvokeError(), nameof(User32.CreatePopupMenu));
        }

        var hResult = User32.AppendMenuW(hMenu, MENU_ITEM_FLAGS.MF_STRING, (UINT_PTR)(uint)TrayIcon.WM_TRAYICON_EXITCOMMAND, (LPCWSTR)"Exit");
        if (!hResult)
        {
            throw Win32Helper.NewWin32Exception((UINT)hResult.Value, Marshal.GetLastPInvokeError(), nameof(User32.AppendMenuW));
        }

        return hMenu;
    }

    public LRESULT TrayIconWndProc(HWND hWnd, MESSAGE_TYPE msg, WPARAM wParam, LPARAM lParam)
    {
        switch (msg)
        {
            case TrayIcon.WM_TRAYICON_SHOWCONTEXTMENU:
                if ((MESSAGE_TYPE)lParam.Value is MESSAGE_TYPE.WM_LBUTTONDOWN or MESSAGE_TYPE.WM_RBUTTONDOWN)
                {
                    // show the context menu associated with the tray icon
                    TrayIcon.ShowTrayIconContextMenu(hWnd, this.Menu);
                }

                break;
            case MESSAGE_TYPE.WM_NOTIFY:
                break;

            case MESSAGE_TYPE.WM_GETMINMAXINFO:
            case MESSAGE_TYPE.WM_NCCREATE:
            case MESSAGE_TYPE.WM_NCCALCSIZE:
            case MESSAGE_TYPE.WM_NCACTIVATE:
            case MESSAGE_TYPE.WM_CREATE:
                break;

            case MESSAGE_TYPE.WM_WINDOWPOSCHANGING:
            case MESSAGE_TYPE.WM_WINDOWPOSCHANGED:
            case MESSAGE_TYPE.WM_ACTIVATE:
            case MESSAGE_TYPE.WM_IME_SETCONTEXT:
            case MESSAGE_TYPE.WM_IME_NOTIFY:
            case MESSAGE_TYPE.WM_SETFOCUS:
                break;

            case MESSAGE_TYPE.WM_ENTERMENULOOP:
            case MESSAGE_TYPE.WM_SETCURSOR:
            case MESSAGE_TYPE.WM_INITMENU:
            case MESSAGE_TYPE.WM_INITMENUPOPUP:
            case MESSAGE_TYPE.WM_UAHINITMENU:
            case MESSAGE_TYPE.WM_UAHMEASUREMENUITEM:
            case MESSAGE_TYPE.WM_ENTERIDLE:
            case MESSAGE_TYPE.WM_UNINITMENUPOPUP:
            case MESSAGE_TYPE.WM_MENUSELECT:
            case MESSAGE_TYPE.WM_EXITMENULOOP:
                break;

            case MESSAGE_TYPE.WM_COMMAND:
                var commandIndex = (MESSAGE_TYPE)(((IntPtr)wParam.Value).ToInt64() & 0x0000FFFF);
                if (commandIndex != TrayIcon.WM_TRAYICON_EXITCOMMAND)
                {
                    throw new InvalidOperationException();
                }

                break;
            case MESSAGE_TYPE.WM_KILLFOCUS:
                break;

            default:
                break;
        }

        return Win32Helper.User32.DefWindowProc(hWnd, msg, wParam, lParam);
    }

    private static void ShowTrayIconContextMenu(HWND hWnd, HMENU hMenu)
    {
        if (hMenu.IsNull)
        {
            throw new InvalidOperationException();
        }

        User32.SetForegroundWindow(hWnd);

        var boolResult = default(BOOL);

        // Get cursor position and convert it to client coordinates
        var cursorLocation = new POINT(0, 0);
        var lpCursorLocation = new LPPOINT(cursorLocation);
        boolResult = User32.GetCursorPos(lpCursorLocation);
        if (!boolResult)
        {
            lpCursorLocation.Free();
            throw Win32Helper.NewWin32Exception((UINT)boolResult.Value, Marshal.GetLastPInvokeError(), nameof(User32.GetCursorPos));
        }

        User32.ScreenToClient(hWnd, lpCursorLocation);
        cursorLocation = lpCursorLocation.ToStructure();
        lpCursorLocation.Free();

        // Set menu information
        var lpcMenuInfo = new LPCMENUINFO(
            new MENUINFO(
                cbSize: (DWORD)MENUINFO.Size,
                fMask: MENUINFO_MASK.MIM_STYLE,
                dwStyle: 0,
                cyMax: 0,
                hbrBack: HBRUSH.Null,
                dwContextHelpID: 0,
                dwMenuData: ULONG_PTR.Null));
        boolResult = User32.SetMenuInfo(hMenu, lpcMenuInfo);
        lpcMenuInfo.Free();
        if (!boolResult)
        {
            throw Win32Helper.NewWin32Exception((UINT)boolResult.Value, Marshal.GetLastPInvokeError(), nameof(User32.SetMenuInfo));
        }

        // Display the context menu at the cursor position
        boolResult = User32.TrackPopupMenuEx(
              hMenu,
              TRACK_POPUP_MENU_FLAGS.TPM_LEFTALIGN | TRACK_POPUP_MENU_FLAGS.TPM_BOTTOMALIGN | TRACK_POPUP_MENU_FLAGS.TPM_LEFTBUTTON,
              cursorLocation.x,
              cursorLocation.y,
              hWnd,
              LPTPMPARAMS.Null);
        if (!boolResult)
        {
            throw Win32Helper.NewWin32Exception((UINT)boolResult.Value, Marshal.GetLastPInvokeError(), nameof(User32.TrackPopupMenuEx));
        }
    }
}
