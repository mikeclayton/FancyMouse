using FancyMouse.HotKeys.Win32Gen;

using Windows.Win32.UI.Input.KeyboardAndMouse;

namespace FancyMouse.HotKeys;

/// <summary>
/// Mirrors PowerToys' own <c>Utilities.Helper.GetKeyName</c>/<c>LayoutMap</c> - a
/// keyboard-layout-aware friendly name for a virtual-key code (e.g. showing the actual
/// character a punctuation/OEM key produces on the user's own keyboard layout), rather than the
/// raw <see cref="Keys"/> enum name. <see cref="PInvoke.GetKeyNameText"/> takes a scan code, not
/// a virtual-key code, so this pairs it with <see cref="PInvoke.MapVirtualKey"/> first.
/// </summary>
public static class FriendlyKeyNameHelper
{
    public static string? GetFriendlyKeyName(int virtualKeyCode)
    {
        var scanCode = User32.MapVirtualKey((uint)virtualKeyCode, MAP_VIRTUAL_KEY_TYPE.MAPVK_VK_TO_VSC)
            .IgnoreFailure()
            .GetValue();
        if (scanCode == 0)
        {
            return null;
        }

        // bits 16-23 of the GetKeyNameText lParam are the scan code; bit 24 marks an
        // "extended" key (e.g. the right-hand Ctrl/Alt, arrow keys, Insert/Delete/Home/End) -
        // without it, GetKeyNameText can't tell those apart from their left-hand/numpad
        // counterparts, which share the same base scan code.
        var isExtended = FriendlyKeyNameHelper.IsExtendedKey(virtualKeyCode);
        var lParam = (int)((scanCode << 16) | (isExtended ? 0x0100_0000u : 0u));

        Span<char> buffer = stackalloc char[64];
        var result = User32.GetKeyNameText(lParam, buffer)
            .IgnoreFailure();
        if (result.Failure)
        {
            return null;
        }

        return new string(buffer[..result.GetValue()]);
    }

    private static bool IsExtendedKey(int virtualKeyCode) => virtualKeyCode switch
    {
        0x21 or 0x22 or 0x23 or 0x24 or 0x25 or 0x26 or 0x27 or 0x28 => true, // Prior/Next/End/Home/Left/Up/Right/Down
        0x2D or 0x2E => true, // Insert/Delete
        0xA3 or 0xA5 => true, // right Control/Menu
        _ => false,
    };
}
