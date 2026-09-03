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

using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace FancyMouse.SettingsUI.Models
{
    public sealed class FancyMouseThumbnailSize : INotifyPropertyChanged
    {
        private double _width;
        private double _height;

        [JsonPropertyName("width")]
        public double Width
        {
            get
            {
                return _width;
            }

            set
            {
                var newWidth = Math.Max(0, value);
                if (newWidth != _width)
                {
                    _width = newWidth;
                    OnPropertyChanged();
                }
            }
        }

        [JsonPropertyName("height")]
        public double Height
        {
            get
            {
                return _height;
            }

            set
            {
                var newHeight = Math.Max(0, value);
                if (newHeight != _height)
                {
                    _height = newHeight;
                    OnPropertyChanged();
                }
            }
        }

        public FancyMouseThumbnailSize()
        {
            Width = 1600;
            Height = 1200;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
