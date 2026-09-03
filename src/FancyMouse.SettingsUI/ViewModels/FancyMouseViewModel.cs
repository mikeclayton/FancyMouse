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

using FancyMouse.SettingsUI.Models;

namespace FancyMouse.SettingsUI.ViewModels;

/// <summary>
/// Local equivalent of PowerToys' <c>MouseUtilsViewModel</c>.
public sealed partial class FancyMouseViewModel : PageViewModelBase
{
    public FancyMouseViewModel(FancyMouseSettingsConfig fancyMouseSettingsConfig)
    {
        this.FancyMouseSettingsConfig = fancyMouseSettingsConfig ?? throw new ArgumentNullException(nameof(fancyMouseSettingsConfig));

        // relay FancyMouseThumbnailSize's own PropertyChanged (Width/Height edits) up to this
        // ViewModel's PropertyChanged, same as MouseUtilsViewModel's InitializeMouseJumpSettings
        // does for MouseJumpThumbnailSize.
        this.FancyMouseSettingsConfig.Properties.ThumbnailSize.PropertyChanged += this.FancyMouseThumbnailSizePropertyChanged;
    }
}
