using System.Runtime.InteropServices;

using FancyMouse.HotKeys.Win32Gen;

using Windows.Win32.UI.Input.KeyboardAndMouse;

namespace FancyMouse.HotKeys;

/// <summary>
/// Small public wrappers around GetAsyncKeyState/SendInput for keyboard events - needed by
/// ShortcutControl's accessibility-tab-leaving trick: when Tab is used to leave the shortcut
/// capture control, <see cref="KeyboardCaptureHook"/> swallows it before it reaches the OS, so a
/// synthetic key event is sent via SendInput (tagged with an "ignore me" ExtraInfo value the
/// hook's own filter recognises and passes straight through) to keep the OS's own view of key
/// state consistent with what the user is physically still holding down.
/// </summary>
public static class KeyStateHelper
{
    public static bool IsKeyDown(int virtualKeyCode)
    {
        var state = User32.GetAsyncKeyState(virtualKeyCode)
            .IgnoreFailure()
            .GetValue();
        return (state & 0x8000) != 0;
    }

    public static void SendKeyEvent(int virtualKeyCode, bool keyDown, nuint extraInfo)
    {
        var inputs = new INPUT[]
        {
            new()
            {
                type = INPUT_TYPE.INPUT_KEYBOARD,
                Anonymous = new()
                {
                    ki = new KEYBDINPUT
                    {
                        wVk = (VIRTUAL_KEY)virtualKeyCode,
                        wScan = 0,
                        dwFlags = keyDown ? default : KEYBD_EVENT_FLAGS.KEYEVENTF_KEYUP,
                        time = 0,
                        dwExtraInfo = extraInfo,
                    },
                },
            },
        };

        _ = User32.SendInput(inputs, Marshal.SizeOf<INPUT>())
            .IgnoreFailure();
    }
}
