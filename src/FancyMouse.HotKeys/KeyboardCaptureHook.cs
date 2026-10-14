using FancyMouse.HotKeys.Win32Gen;

using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Input.KeyboardAndMouse;
using Windows.Win32.UI.WindowsAndMessaging;

namespace FancyMouse.HotKeys;

public delegate void KeyEvent(int key);

public delegate bool IsActive();

public delegate bool FilterAccessibleKeyboardEvents(int key, nuint extraInfo);

/// <summary>
/// Mirrors PowerToys' own <c>HotkeySettingsControlHook</c>/native <c>KeyboardHook.cpp</c> - a
/// global, system-wide low-level keyboard hook (<c>WH_KEYBOARD_LL</c>), which is what lets the
/// shortcut-capture dialog see key combinations - the Windows key alone, Alt+Tab, etc. - that
/// never reach a WinUI3 control's own routed KeyDown/KeyUp events, since the OS intercepts those
/// before they'd ever reach the app's message queue. Reimplemented here directly against
/// CsWin32-generated declarations rather than pulling in PowerToys' own native C++/WinRT
/// <c>PowerToys.Interop.KeyboardHook</c> project - the real implementation behind it is a plain,
/// ~100-line <c>SetWindowsHookEx</c>/<c>CallNextHookEx</c> hook with no further native
/// dependencies of its own, so there's nothing to gain from the native project here.
/// </summary>
public sealed class KeyboardCaptureHook : IDisposable
{
    private const int WmKeyDown = 0x0100;
    private const int WmKeyUp = 0x0101;
    private const int WmSysKeyDown = 0x0104;
    private const int WmSysKeyUp = 0x0105;

    private readonly KeyEvent keyDown;
    private readonly KeyEvent keyUp;
    private readonly IsActive isActive;
    private readonly FilterAccessibleKeyboardEvents? filterAccessibleKeyboardEvents;

    // kept alive for the lifetime of the hook - SetWindowsHookEx does not root the delegate
    // itself, so without this the GC could collect it while the native hook still holds a
    // function pointer into it.
    private readonly HOOKPROC hookProc;

    private HHOOK hook;
    private bool disposed;

    public KeyboardCaptureHook(KeyEvent keyDown, KeyEvent keyUp, IsActive isActive, FilterAccessibleKeyboardEvents? filterAccessibleKeyboardEvents)
    {
        this.keyDown = keyDown;
        this.keyUp = keyUp;
        this.isActive = isActive;
        this.filterAccessibleKeyboardEvents = filterAccessibleKeyboardEvents;
        this.hookProc = this.HookProc;

        this.hook = User32.SetWindowsHookEx(WINDOWS_HOOK_ID.WH_KEYBOARD_LL, this.hookProc, default, 0)
            .ThrowIfFailed()
            .GetValue();
    }

    public bool GetDisposedState() => this.disposed;

    public void Dispose()
    {
        if (!this.disposed)
        {
            if (!this.hook.IsNull)
            {
                _ = User32.UnhookWindowsHookEx(this.hook)
                    .IgnoreFailure();
                this.hook = default;
            }

            this.disposed = true;
        }
    }

    private unsafe LRESULT HookProc(int code, WPARAM wParam, LPARAM lParam)
    {
        if (code == PInvoke.HC_ACTION && this.isActive())
        {
            var info = *(KBDLLHOOKSTRUCT*)lParam.Value;
            var message = (int)wParam.Value;

            if (this.filterAccessibleKeyboardEvents is null || this.filterAccessibleKeyboardEvents((int)info.vkCode, info.dwExtraInfo))
            {
                switch (message)
                {
                    case WmKeyDown:
                    case WmSysKeyDown:
                        this.keyDown((int)info.vkCode);
                        return (LRESULT)1;
                    case WmKeyUp:
                    case WmSysKeyUp:
                        this.keyUp((int)info.vkCode);
                        return (LRESULT)1;
                }
            }
        }

        return User32.CallNextHookEx(default, code, wParam, lParam)
            .IgnoreFailure()
            .GetValue();
    }
}
