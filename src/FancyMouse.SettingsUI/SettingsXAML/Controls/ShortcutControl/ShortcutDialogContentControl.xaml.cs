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

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace FancyMouse.Settings.UI.Controls
{
    public sealed partial class ShortcutDialogContentControl : UserControl
    {
        public static readonly DependencyProperty KeysProperty = DependencyProperty.Register(nameof(Keys), typeof(List<object>), typeof(ShortcutDialogContentControl), new PropertyMetadata(default(string)));
        public static readonly DependencyProperty IsErrorProperty = DependencyProperty.Register(nameof(IsError), typeof(bool), typeof(ShortcutDialogContentControl), new PropertyMetadata(false));
        public static readonly DependencyProperty IsWarningAltGrProperty = DependencyProperty.Register(nameof(IsWarningAltGr), typeof(bool), typeof(ShortcutDialogContentControl), new PropertyMetadata(false));

        public event RoutedEventHandler? ResetClick;

        public event RoutedEventHandler? ClearClick;

        public ShortcutDialogContentControl()
        {
            this.InitializeComponent();
        }

        public List<object> Keys
        {
            get => (List<object>)GetValue(KeysProperty);
            set => SetValue(KeysProperty, value);
        }

        public bool IsError
        {
            get => (bool)GetValue(IsErrorProperty);
            set => SetValue(IsErrorProperty, value);
        }

        public bool IsWarningAltGr
        {
            get => (bool)GetValue(IsWarningAltGrProperty);
            set => SetValue(IsWarningAltGrProperty, value);
        }

        private void ResetBtn_Click(object sender, RoutedEventArgs e)
        {
            ResetClick?.Invoke(this, new RoutedEventArgs());
        }

        private void ClearBtn_Click(object sender, RoutedEventArgs e)
        {
            ClearClick?.Invoke(this, new RoutedEventArgs());
        }
    }
}
