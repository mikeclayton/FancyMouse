using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;

using FancyMouse.Win32.Interop;

using static FancyMouse.Win32.NativeMethods.Core;
using static FancyMouse.Win32.NativeMethods.Shell32;
using static FancyMouse.Win32.NativeMethods.User32;

namespace FancyMouse.Common.Helpers;

public sealed class TrayIcon
{
#pragma warning disable SA1310 // Field names should not contain underscore
    private const MESSAGE_TYPE WM_TRAYICON_SHOWCONTEXTMENU = MESSAGE_TYPE.WM_USER + 100;
    private const MESSAGE_TYPE WM_TRAYICON_EXITCOMMAND = MESSAGE_TYPE.WM_USER + 101;
#pragma warning restore SA1310 // Field names should not contain underscore

    // initialise with an empty delegate so we don't get complaints about
    // it being null when the constructor exits.
    public event EventHandler<EventArgs> ExitCommandClicked = (sender, e) => { };

    public TrayIcon()
    {
        // cache the window proc delegate so it doesn't get garbage-collected
        this.WndProc = this.WindowProc;
    }

    private WNDPROC WndProc
    {
        get;
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

    private HMENU ContextMenu
    {
        get;
        set;
    }

    private void OnExitCommandClicked(EventArgs e)
    {
        this.ExitCommandClicked.Invoke(this, e);
    }

    public void Initialize()
    {
        this.InitializeTrayWindow();
        this.InitializeTrayIcon();
        this.InitializeTrayMenu();
    }

    private void InitializeTrayWindow()
    {
        var window = Win32Helper.CreateMessageOnlyWindow(
            className: "FancyMouseTrayIconClass",
            windowName: "FancyMouseTrayIconWindow",
            wndProc: this.WndProc);
        this.Window = window;
    }

    private void InitializeTrayIcon()
    {
        var hWnd = this.Window?.Hwnd ?? throw new InvalidOperationException();

        var icon = TrayIcon.GetTrayIconResource();
        this.Icon = icon;

        var notifyIconData = new NOTIFYICONDATAW(
            cbSize: (DWORD)NOTIFYICONDATAW.Size,
            hWnd: hWnd,
            uID: 1,
            uFlags: NOTIFY_ICON_DATA_FLAGS.NIF_MESSAGE | NOTIFY_ICON_DATA_FLAGS.NIF_ICON | NOTIFY_ICON_DATA_FLAGS.NIF_TIP,
            uCallbackMessage: TrayIcon.WM_TRAYICON_SHOWCONTEXTMENU,
            hIcon: (HICON)icon.Handle,
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
        var result = Shell32.Shell_NotifyIcon(NOTIFY_ICON_MESSAGE.NIM_ADD, iconDataPtr);
        iconDataPtr.Free();
        if (!result)
        {
            throw Win32Helper.NewWin32Exception((UINT)result.Value, nameof(Shell32.Shell_NotifyIcon));
        }
    }

    private void InitializeTrayMenu()
    {
        var hMenu = User32.CreatePopupMenu();
        var hResult = User32.AppendMenu(hMenu, MENU_ITEM_FLAGS.MF_STRING, (UINT_PTR)(uint)TrayIcon.WM_TRAYICON_EXITCOMMAND, (LPCWSTR)"Exit");
        this.ContextMenu = hMenu;
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

    public LRESULT WindowProc(HWND hWnd, MESSAGE_TYPE msg, WPARAM wParam, LPARAM lParam)
    {
        switch (msg)
        {
            // tray icon callback message
            case TrayIcon.WM_TRAYICON_SHOWCONTEXTMENU:
                if ((MESSAGE_TYPE)lParam.Value is MESSAGE_TYPE.WM_LBUTTONDOWN or MESSAGE_TYPE.WM_RBUTTONDOWN)
                {
                    // show the context menu associated with the tray icon
                    TrayIcon.ShowTrayIconContextMenu(hWnd, this.ContextMenu);
                }

                break;

            /* received during tray icon create */
            case MESSAGE_TYPE.WM_GETMINMAXINFO:
            case MESSAGE_TYPE.WM_NCCREATE:
            case MESSAGE_TYPE.WM_NCCALCSIZE:
            case MESSAGE_TYPE.WM_CREATE:
                break;

            /* context menu display */
            case MESSAGE_TYPE.WM_WINDOWPOSCHANGING:
            case MESSAGE_TYPE.WM_WINDOWPOSCHANGED:
            case MESSAGE_TYPE.WM_NCACTIVATE:
            case MESSAGE_TYPE.WM_ACTIVATE:
            case MESSAGE_TYPE.WM_IME_SETCONTEXT:
            case MESSAGE_TYPE.WM_IME_NOTIFY:
            case MESSAGE_TYPE.WM_SETFOCUS:
                break;

            /* context menu message loop */
            case MESSAGE_TYPE.WM_ENTERMENULOOP:
            case MESSAGE_TYPE.WM_SETCURSOR:
            case MESSAGE_TYPE.WM_INITMENU:
            case MESSAGE_TYPE.WM_INITMENUPOPUP:
            case MESSAGE_TYPE.WM_UAHINITMENU:
            case MESSAGE_TYPE.WM_UAHMEASUREMENUITEM:
            case MESSAGE_TYPE.WM_ENTERIDLE:
            case MESSAGE_TYPE.WM_MENUSELECT:
            case MESSAGE_TYPE.WM_UNINITMENUPOPUP:
            case MESSAGE_TYPE.WM_EXITMENULOOP:
            case MESSAGE_TYPE.WM_KILLFOCUS:
                break;

            /* menu item clicked */
            case MESSAGE_TYPE.WM_COMMAND:
                const long lowWordMask = 0x0000FFFF;
                var commandIndex = (MESSAGE_TYPE)(((IntPtr)wParam.Value).ToInt64() & lowWordMask);
                switch (commandIndex)
                {
                    case TrayIcon.WM_TRAYICON_EXITCOMMAND:
                        this.OnExitCommandClicked(EventArgs.Empty);
                        break;
                    default:
                        throw new InvalidOperationException();
                }

                break;

            default:
                break;
        }

        return User32.DefWindowProc(hWnd, msg, wParam, lParam);
    }

    private static void ShowTrayIconContextMenu(HWND hWnd, HMENU hMenu)
    {
        if (hMenu.IsNull)
        {
            throw new InvalidOperationException();
        }

        User32.SetForegroundWindow(hWnd);

        var result = default(BOOL);

        // Get cursor position and convert it to client coordinates
        var cursorLocation = User32.GetCursorPos();
        User32.ScreenToClient(hWnd, cursorLocation);

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
        result = User32.SetMenuInfo(hMenu, lpcMenuInfo);
        lpcMenuInfo.Free();

        // Display the context menu at the cursor position
        result = User32.TrackPopupMenuEx(
              hMenu,
              TRACK_POPUP_MENU_FLAGS.TPM_LEFTALIGN | TRACK_POPUP_MENU_FLAGS.TPM_BOTTOMALIGN | TRACK_POPUP_MENU_FLAGS.TPM_LEFTBUTTON,
              cursorLocation.x,
              cursorLocation.y,
              hWnd,
              LPTPMPARAMS.Null);
    }
}
