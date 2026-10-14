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
using FancyMouse.SettingsUI;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.Windows.ApplicationModel.Resources;
using Windows.System;

namespace FancyMouse.Settings.UI.Controls
{
    public sealed partial class ShortcutControl : UserControl, IDisposable
    {
        private const nuint IgnoreKeyEventFlag = 0x5555;

        private readonly HashSet<VirtualKey> modifierKeysOnEntering = [];

        private HotkeyCapture internalCapture = new();
        private HotkeyCapture? lastValidCapture;
        private KeyboardCaptureHook? hook;
        private bool isActive;
        private bool disposedValue;

        private bool isDialogOpen;

        public static readonly DependencyProperty KeystrokeProperty = DependencyProperty.Register(
            nameof(Keystroke),
            typeof(Keystroke),
            typeof(ShortcutControl),
            new PropertyMetadata(null, (d, _) => ((ShortcutControl)d).SetKeys()));

        private static readonly ResourceLoader ResourceLoader = new();

        private readonly ShortcutDialogContentControl content = new();
        private readonly ContentDialog shortcutDialog;

        public Keystroke? Keystroke
        {
            get => (Keystroke?)this.GetValue(ShortcutControl.KeystrokeProperty);
            set => this.SetValue(ShortcutControl.KeystrokeProperty, value);
        }

        public ShortcutControl()
        {
            this.InitializeComponent();

            this.content.ResetClick += this.Content_ResetClick;
            this.content.ClearClick += this.Content_ClearClick;

            // Created in code, not XAML, the same way PowerToys does it - constructing a ContentDialog
            // from XAML has caused WinUI/XAML-Island issues with dark theme in the past.
            this.shortcutDialog = new ContentDialog
            {
                Title = ShortcutControl.ResourceLoader.GetString("ShortcutDialog_Title"),
                Content = this.content,
                PrimaryButtonText = ShortcutControl.ResourceLoader.GetString("ShortcutDialog_Save"),
                CloseButtonText = ShortcutControl.ResourceLoader.GetString("ShortcutDialog_Cancel"),
                DefaultButton = ContentDialogButton.Primary,
            };

            this.Unloaded += this.ShortcutControl_Unloaded;
            this.Loaded += this.ShortcutControl_Loaded;

            AutomationProperties.SetName(this.EditButton, ShortcutControl.ResourceLoader.GetString("ShortcutDialog_Title"));

            this.SetKeys();
        }

        private void ShortcutControl_Unloaded(object sender, RoutedEventArgs e)
        {
            this.shortcutDialog.PrimaryButtonClick -= this.ShortcutDialog_PrimaryButtonClick;
            this.shortcutDialog.Opened -= this.ShortcutDialog_Opened;
            this.shortcutDialog.Closing -= this.ShortcutDialog_Closing;

            if ((Application.Current as App)?.MainWindow is { } window)
            {
                window.Activated -= this.ShortcutDialog_SettingsWindow_Activated;
            }

            this.hook?.Dispose();
            this.hook = null;
        }

        private void ShortcutControl_Loaded(object sender, RoutedEventArgs e)
        {
            // Belongs in Loaded, not the constructor - virtualization (e.g. inside a ListView) can
            // take this control through several Loaded/Unloaded cycles.
            this.hook?.Dispose();
            this.hook = new KeyboardCaptureHook(this.Hook_KeyDown, this.Hook_KeyUp, this.Hook_IsActive, this.FilterAccessibleKeyboardEvents);

            this.shortcutDialog.PrimaryButtonClick += this.ShortcutDialog_PrimaryButtonClick;
            this.shortcutDialog.Opened += this.ShortcutDialog_Opened;
            this.shortcutDialog.Closing += this.ShortcutDialog_Closing;

            if ((Application.Current as App)?.MainWindow is { } window)
            {
                window.Activated += this.ShortcutDialog_SettingsWindow_Activated;
            }
        }

        private void KeyEventHandler(int key, bool matchValue, int matchValueCode)
        {
            var virtualKey = (VirtualKey)key;
            switch (virtualKey)
            {
                case VirtualKey.LeftWindows:
                case VirtualKey.RightWindows:
                    if (!matchValue && this.modifierKeysOnEntering.Contains(virtualKey))
                    {
                        KeyStateHelper.SendKeyEvent((int)virtualKey, keyDown: false, ShortcutControl.IgnoreKeyEventFlag);
                        _ = this.modifierKeysOnEntering.Remove(virtualKey);
                    }

                    this.internalCapture.Win = matchValue;
                    break;
                case VirtualKey.Control:
                case VirtualKey.LeftControl:
                case VirtualKey.RightControl:
                    if (!matchValue && this.modifierKeysOnEntering.Contains(VirtualKey.Control))
                    {
                        KeyStateHelper.SendKeyEvent((int)virtualKey, keyDown: false, ShortcutControl.IgnoreKeyEventFlag);
                        _ = this.modifierKeysOnEntering.Remove(VirtualKey.Control);
                    }

                    this.internalCapture.Ctrl = matchValue;
                    break;
                case VirtualKey.Menu:
                case VirtualKey.LeftMenu:
                case VirtualKey.RightMenu:
                    if (!matchValue && this.modifierKeysOnEntering.Contains(VirtualKey.Menu))
                    {
                        KeyStateHelper.SendKeyEvent((int)virtualKey, keyDown: false, ShortcutControl.IgnoreKeyEventFlag);
                        _ = this.modifierKeysOnEntering.Remove(VirtualKey.Menu);
                    }

                    this.internalCapture.Alt = matchValue;
                    break;
                case VirtualKey.Shift:
                case VirtualKey.LeftShift:
                case VirtualKey.RightShift:
                    if (!matchValue && this.modifierKeysOnEntering.Contains(VirtualKey.Shift))
                    {
                        KeyStateHelper.SendKeyEvent((int)virtualKey, keyDown: false, ShortcutControl.IgnoreKeyEventFlag);
                        _ = this.modifierKeysOnEntering.Remove(VirtualKey.Shift);
                    }

                    this.internalCapture.Shift = matchValue;
                    break;
                case VirtualKey.Escape:
                    this.internalCapture = new HotkeyCapture();
                    this.shortcutDialog.IsPrimaryButtonEnabled = false;
                    return;
                default:
                    this.internalCapture.Code = matchValueCode;
                    break;
            }
        }

        private bool FilterAccessibleKeyboardEvents(int key, nuint extraInfo)
        {
            // A key event sent with this value in its extra-info field is our own synthetic one (see
            // KeyEventHandler's SendKeyEvent calls) - ignore it so it reaches the system instead of
            // being captured again.
            if (extraInfo == ShortcutControl.IgnoreKeyEventFlag)
            {
                return false;
            }

            if ((VirtualKey)key == VirtualKey.Tab)
            {
                // Shift wasn't down on entering and isn't down now (and no other modifier is held) -
                // treat this as a normal Tab press leaving the control.
                if (!this.internalCapture.Shift && !this.modifierKeysOnEntering.Contains(VirtualKey.Shift) && !this.internalCapture.Win && !this.internalCapture.Alt && !this.internalCapture.Ctrl)
                {
                    return false;
                }

                // Shift wasn't down on entering but is down now - the user pressed Shift+Tab to leave
                // the control (not as part of a shortcut). Simulate the Shift keydown the system
                // never saw, and let the hook's own Shift bookkeeping forget it was part of a capture.
                if (this.internalCapture.Shift && !this.modifierKeysOnEntering.Contains(VirtualKey.Shift) && !this.internalCapture.Win && !this.internalCapture.Alt && !this.internalCapture.Ctrl)
                {
                    this.internalCapture.Shift = false;
                    KeyStateHelper.SendKeyEvent((int)VirtualKey.Shift, keyDown: true, ShortcutControl.IgnoreKeyEventFlag);
                    return false;
                }

                // Shift was already down on entering and still is - only the Tab itself should be let
                // through, since the system already believes Shift is held.
                if (!this.internalCapture.Shift && this.modifierKeysOnEntering.Contains(VirtualKey.Shift) && !this.internalCapture.Win && !this.internalCapture.Alt && !this.internalCapture.Ctrl)
                {
                    return false;
                }
            }

            // The Save/Cancel buttons already have keyboard focus - let normal keyboard navigation
            // through instead of capturing it as part of the shortcut.
            if ((this.XamlRoot is not null) && (FocusManager.GetFocusedElement(this.XamlRoot)?.GetType() == typeof(Button)))
            {
                return false;
            }

            return true;
        }

        private void Hook_KeyDown(int key)
        {
            this.KeyEventHandler(key, matchValue: true, matchValueCode: key);

            var keys = this.internalCapture.GetKeysList();
            this.content.Keys = keys;

            if (keys.Count == 0)
            {
                this.shortcutDialog.IsPrimaryButtonEnabled = false;
            }
            else if (keys.Count == 1)
            {
                this.shortcutDialog.IsPrimaryButtonEnabled = false;
                this.content.IsError = !(this.internalCapture.Shift || this.internalCapture.Win || this.internalCapture.Alt || this.internalCapture.Ctrl);
            }

            // Tab and Shift+Tab are accessible keys and shouldn't be captured as part of the shortcut.
            if ((this.internalCapture.Code > 0) && !this.internalCapture.IsAccessibleShortcut())
            {
                this.lastValidCapture = this.internalCapture.Clone();

                if (this.lastValidCapture.IsValid() || this.lastValidCapture.IsEmpty())
                {
                    this.shortcutDialog.IsPrimaryButtonEnabled = true;
                    this.content.IsError = false;
                }
                else
                {
                    this.shortcutDialog.IsPrimaryButtonEnabled = false;
                    this.content.IsError = true;
                }
            }

            this.content.IsWarningAltGr = this.internalCapture.Ctrl && this.internalCapture.Alt && !this.internalCapture.Win && (this.internalCapture.Code > 0);
        }

        private void Hook_KeyUp(int key) => this.KeyEventHandler(key, matchValue: false, matchValueCode: 0);

        private bool Hook_IsActive() => this.isActive;

        private void ShortcutDialog_Opened(ContentDialog sender, ContentDialogOpenedEventArgs args)
        {
            var current = HotkeyCapture.FromKeystroke(this.Keystroke);
            this.shortcutDialog.IsPrimaryButtonEnabled = current.IsValid() || current.IsEmpty();

            // Reset the capture buffer and take a snapshot of which modifier keys were already held
            // down on entering, so FilterAccessibleKeyboardEvents can tell "Tab used to leave the
            // control" apart from "Shift+Tab typed as part of the shortcut".
            this.internalCapture = new HotkeyCapture();
            this.lastValidCapture = null;
            this.modifierKeysOnEntering.Clear();

            if (KeyStateHelper.IsKeyDown((int)VirtualKey.Shift))
            {
                this.modifierKeysOnEntering.Add(VirtualKey.Shift);
            }

            if (KeyStateHelper.IsKeyDown((int)VirtualKey.Control))
            {
                this.modifierKeysOnEntering.Add(VirtualKey.Control);
            }

            if (KeyStateHelper.IsKeyDown((int)VirtualKey.Menu))
            {
                this.modifierKeysOnEntering.Add(VirtualKey.Menu);
            }

            if (KeyStateHelper.IsKeyDown((int)VirtualKey.LeftWindows))
            {
                this.modifierKeysOnEntering.Add(VirtualKey.LeftWindows);
            }

            if (KeyStateHelper.IsKeyDown((int)VirtualKey.RightWindows))
            {
                this.modifierKeysOnEntering.Add(VirtualKey.RightWindows);
            }

            this.isActive = true;
        }

        private async void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.isDialogOpen)
            {
                return;
            }

            this.isDialogOpen = true;
            try
            {
                var keys = HotkeyCapture.FromKeystroke(this.Keystroke).GetKeysList();
                this.content.Keys = keys;

                // 92 is the Windows key - the AltGr warning applies whenever a shortcut contains both
                // Ctrl and Alt but not Win.
                this.content.IsWarningAltGr = keys.Contains("Ctrl") && keys.Contains("Alt") && !keys.Contains(92);
                this.content.IsError = false;

                this.shortcutDialog.XamlRoot = this.XamlRoot;
                this.shortcutDialog.RequestedTheme = this.ActualTheme;
                _ = await this.shortcutDialog.ShowAsync();
            }
            finally
            {
                this.isDialogOpen = false;
            }
        }

        private void Content_ResetClick(object sender, RoutedEventArgs e)
        {
            this.Keystroke = null;
            this.shortcutDialog.Hide();
        }

        private void Content_ClearClick(object sender, RoutedEventArgs e)
        {
            this.Keystroke = null;
            this.shortcutDialog.Hide();
        }

        private void ShortcutDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            if ((this.lastValidCapture is not null) && (this.lastValidCapture.IsValid() || this.lastValidCapture.IsEmpty()))
            {
                this.Keystroke = this.lastValidCapture.ToKeystroke();
            }

            this.shortcutDialog.Hide();
        }

        private void ShortcutDialog_SettingsWindow_Activated(object sender, WindowActivatedEventArgs args)
        {
            args.Handled = true;
            if ((args.WindowActivationState != WindowActivationState.Deactivated) && (this.hook is null || this.hook.GetDisposedState()))
            {
                // The settings window regained focus/activation - re-enable the keyboard hook so it
                // can catch input again.
                this.hook = new KeyboardCaptureHook(this.Hook_KeyDown, this.Hook_KeyUp, this.Hook_IsActive, this.FilterAccessibleKeyboardEvents);
            }
            else if ((args.WindowActivationState == WindowActivationState.Deactivated) && (this.hook is not null) && !this.hook.GetDisposedState())
            {
                // The settings window lost focus/activation - disable the keyboard hook so keyboard
                // input reaches whatever other window the user switched to.
                this.hook.Dispose();
                this.hook = null;
            }
        }

        private void ShortcutDialog_Closing(ContentDialog sender, ContentDialogClosingEventArgs args)
        {
            this.isActive = false;
        }

        private void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                if (disposing)
                {
                    this.hook?.Dispose();
                    this.hook = null;
                }

                this.disposedValue = true;
            }
        }

        public void Dispose()
        {
            this.Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        private void SetKeys()
        {
            var keys = HotkeyCapture.FromKeystroke(this.Keystroke).GetKeysList();

            if (keys.Count > 0)
            {
                VisualStateManager.GoToState(this, "Configured", true);
                this.PreviewKeysControl.ItemsSource = keys;
                AutomationProperties.SetHelpText(this.EditButton, this.Keystroke?.ToString() ?? string.Empty);
            }
            else
            {
                VisualStateManager.GoToState(this, "Normal", true);
                AutomationProperties.SetHelpText(this.EditButton, ShortcutControl.ResourceLoader.GetString("ConfigureShortcut"));
            }
        }
    }
}
