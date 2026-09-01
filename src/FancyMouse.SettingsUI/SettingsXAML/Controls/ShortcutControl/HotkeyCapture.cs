// Derived from Microsoft PowerToys.
// https://github.com/microsoft/PowerToys
//
// Original notice:
// =====
// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
// =====
//
// Substantially modified for FancyMouse.
// See THIRD-PARTY-NOTICES.md for attribution details.

using FancyMouse.HotKeys;

namespace FancyMouse.Settings.UI.Controls;

/// <summary>
/// Used internally by the <see cref="ShortcutControl"/> to track which
/// keys are present in the "live" hotkey combination currently being captured
/// by the control.
/// </summary>
public sealed class HotkeyCapture
{
    private const int VkTab = 0x09;

    public bool Win { get; set; }

    public bool Ctrl { get; set; }

    public bool Alt { get; set; }

    public bool Shift { get; set; }

    public int Code { get; set; }

    public static HotkeyCapture FromKeystroke(Keystroke? keystroke)
    {
        if (keystroke is null)
        {
            return new HotkeyCapture();
        }

        return new HotkeyCapture
        {
            Win = keystroke.Modifiers.HasFlag(KeyModifiers.Windows),
            Ctrl = keystroke.Modifiers.HasFlag(KeyModifiers.Control),
            Alt = keystroke.Modifiers.HasFlag(KeyModifiers.Alt),
            Shift = keystroke.Modifiers.HasFlag(KeyModifiers.Shift),
            Code = (int)keystroke.Key,
        };
    }

    public Keystroke? ToKeystroke()
    {
        if (this.IsEmpty())
        {
            return null;
        }

        var modifiers = KeyModifiers.None;
        if (this.Win)
        {
            modifiers |= KeyModifiers.Windows;
        }

        if (this.Ctrl)
        {
            modifiers |= KeyModifiers.Control;
        }

        if (this.Alt)
        {
            modifiers |= KeyModifiers.Alt;
        }

        if (this.Shift)
        {
            modifiers |= KeyModifiers.Shift;
        }

        return new Keystroke((Keys)this.Code, modifiers);
    }

    public HotkeyCapture Clone()
    {
        return new HotkeyCapture
        {
            Win = this.Win,
            Ctrl = this.Ctrl,
            Alt = this.Alt,
            Shift = this.Shift,
            Code = this.Code,
        };
    }

    public bool IsValid()
    {
        if (this.IsAccessibleShortcut())
        {
            return false;
        }

        return (this.Alt || this.Ctrl || this.Win || this.Shift) && this.Code != 0;
    }

    public bool IsEmpty()
    {
        return !this.Alt && !this.Ctrl && !this.Win && !this.Shift && this.Code == 0;
    }

    public bool IsAccessibleShortcut()
    {
        // Shift+Tab and Tab are accessible shortcuts.
        return (!this.Alt && !this.Ctrl && !this.Win && this.Shift && this.Code == HotkeyCapture.VkTab)
            || (!this.Alt && !this.Ctrl && !this.Win && !this.Shift && this.Code == HotkeyCapture.VkTab);
    }

    public List<object> GetKeysList()
    {
        var shortcutList = new List<object>();

        if (this.Win)
        {
            shortcutList.Add(92); // the Windows key or button
        }

        if (this.Ctrl)
        {
            shortcutList.Add("Ctrl");
        }

        if (this.Alt)
        {
            shortcutList.Add("Alt");
        }

        if (this.Shift)
        {
            shortcutList.Add(16); // the Shift key or button
        }

        if (this.Code > 0)
        {
            switch (this.Code)
            {
                // https://learn.microsoft.com/uwp/api/windows.system.virtualkey
                case 38: // Up
                case 40: // Down
                case 37: // Left
                case 39: // Right
                    shortcutList.Add(this.Code);
                    break;
                default:
                    shortcutList.Add(FriendlyKeyNameHelper.GetFriendlyKeyName(this.Code) ?? ((Keys)this.Code).ToString());
                    break;
            }
        }

        return shortcutList;
    }

    public override string ToString()
    {
        var parts = new List<string>();

        if (this.Win)
        {
            parts.Add("Win");
        }

        if (this.Ctrl)
        {
            parts.Add("Ctrl");
        }

        if (this.Alt)
        {
            parts.Add("Alt");
        }

        if (this.Shift)
        {
            parts.Add("Shift");
        }

        if (this.Code > 0)
        {
            parts.Add(FriendlyKeyNameHelper.GetFriendlyKeyName(this.Code) ?? ((Keys)this.Code).ToString());
        }

        return string.Join(" + ", parts);
    }
}
