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

namespace FancyMouse.SettingsUI.Models
{
    public class FancyMouseProperties
    {
        public string DefaultActivationShortcut => "CTRL + ALT + SHIFT + F";

        public string? ActivationShortcut
        {
            get;
            set;
        }

        public FancyMouseThumbnailSize ThumbnailSize
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the preview type.
        /// Allowed values are "compact", "bezelled", "custom"
        /// </summary>
        public string? PreviewType
        {
            get;
            set;
        }

        public string? BackgroundColor1
        {
            get;
            set;
        }

        public string? BackgroundColor2
        {
            get;
            set;
        }

        public double BorderThickness
        {
            get;
            set;
        }

        public string? BorderColor
        {
            get;
            set;
        }

        public double Border3dDepth
        {
            get;
            set;
        }

        public double BorderPadding
        {
            get;
            set;
        }

        public double BezelThickness
        {
            get;
            set;
        }

        public string? BezelColor
        {
            get;
            set;
        }

        public double Bezel3dDepth
        {
            get;
            set;
        }

        public double ScreenMargin
        {
            get;
            set;
        }

        public string? ScreenColor1
        {
            get;
            set;
        }

        public string? ScreenColor2
        {
            get;
            set;
        }

        public FancyMouseProperties()
        {
            ActivationShortcut = DefaultActivationShortcut;
            ThumbnailSize = new FancyMouseThumbnailSize();
        }
    }
}
