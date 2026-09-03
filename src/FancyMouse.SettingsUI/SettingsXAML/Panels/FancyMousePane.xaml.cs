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

using CommunityToolkit.WinUI;
using CommunityToolkit.WinUI.Controls;

using FancyMouse.Common.Helpers;
using FancyMouse.Settings.V2;
using FancyMouse.SettingsUI.Models;
using FancyMouse.SettingsUI.ViewModels;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace FancyMouse.SettingsUI.Panels
{
    public sealed partial class FancyMousePane : UserControl
    {
        internal FancyMouseViewModel? ViewModel { get; private set; }

        public FancyMousePane()
        {
            InitializeComponent();
        }

        internal void SetConfig(FancyMouseSettingsConfig config)
        {
            this.ViewModel = new FancyMouseViewModel(config);
            this.Bindings.Update();
        }

        private void PreviewImage_Loaded(object sender, RoutedEventArgs e)
        {
            bool TryFindFrameworkElement(SettingsCard settingsCard, string partName, [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out FrameworkElement? result)
            {
                result = settingsCard.FindDescendants()
                    .OfType<FrameworkElement>()
                    .FirstOrDefault(
                        x => x.Name == partName);
                return result is not null;
            }

            /*
                apply a variation of the "Left" VisualState for SettingsCards
                to center the preview image in the true center of the card
                see https://github.com/CommunityToolkit/Windows/blob/9c7642ff35eaaa51a404f9bcd04b10c7cf851921/components/SettingsControls/src/SettingsCard/SettingsCard.xaml#L334-L347
            */

            var settingsCard = (SettingsCard)sender;

            var partNames = new List<string>
            {
                "PART_HeaderIconPresenterHolder",
                "PART_DescriptionPresenter",
                "PART_HeaderPresenter",
                "PART_ActionIconPresenter",
            };
            foreach (var partName in partNames)
            {
                if (!TryFindFrameworkElement(settingsCard, partName, out var element))
                {
                    continue;
                }

                element.Visibility = Visibility.Collapsed;
            }

            if (TryFindFrameworkElement(settingsCard, "PART_ContentPresenter", out var content))
            {
                Grid.SetRow(content, 1);
                Grid.SetColumn(content, 1);
                content.HorizontalAlignment = HorizontalAlignment.Center;
            }
        }

        private void PreviewTypeSetting_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // The Segmented control can fire SelectionChanged transiently with SelectedIndex == -1
            // (e.g. during template apply or when items are being initialized/refreshed before the
            // x:Bind two-way binding restores the persisted value). Ignore those intermediate states
            // instead of throwing.
            if (this.PreviewTypeSetting.SelectedIndex < 0)
            {
                return;
            }

            // hide or display controls based on whether the "Custom" preview type is selected
            var selectedPreviewType = this.GetSelectedPreviewType();
            var customPreviewTypeSelected = selectedPreviewType == PreviewType.Custom;
            this.CopyStyleToCustom.IsEnabled = !customPreviewTypeSelected;
            var customControlVisibility = customPreviewTypeSelected
                ? Visibility.Visible
                : Visibility.Collapsed;
            this.FancyMouse_BackgroundColor1.Visibility = customControlVisibility;
            this.FancyMouse_BackgroundColor2.Visibility = customControlVisibility;
            this.FancyMouse_BorderThickness.Visibility = customControlVisibility;
            this.FancyMouse_BorderColor.Visibility = customControlVisibility;
            this.FancyMouse_Border3dDepth.Visibility = customControlVisibility;
            this.FancyMouse_BorderPadding.Visibility = customControlVisibility;
            this.FancyMouse_BezelThickness.Visibility = customControlVisibility;
            this.FancyMouse_BezelColor.Visibility = customControlVisibility;
            this.FancyMouse_Bezel3dDepth.Visibility = customControlVisibility;
            this.FancyMouse_ScreenMargin.Visibility = customControlVisibility;
            this.FancyMouse_ScreenColor1.Visibility = customControlVisibility;
            this.FancyMouse_ScreenColor2.Visibility = customControlVisibility;
        }

        private /* async */ void CopyStyleToCustom_Click(object sender, RoutedEventArgs e)
        {
            this.FancyMouse_CopyToCustomStyle_MessageBox_PrimaryButtonCommand();
        }

        private void FancyMouse_CopyToCustomStyle_MessageBox_PrimaryButtonCommand()
        {
            var selectedPreviewType = this.GetSelectedPreviewType();
            var selectedPreviewStyle = selectedPreviewType switch
            {
                PreviewType.Compact => StyleHelper.CompactPreviewStyle,
                PreviewType.Bezelled => StyleHelper.BezelledPreviewStyle,
                PreviewType.Custom => StyleHelper.BezelledPreviewStyle,
                _ => throw new InvalidOperationException(),
            };

            // convert the color into a string.
            // note that we have to replace Named and System colors with their ARGB equivalents
            // so that serialization returns an ARGB string rather than the Named or System color *name*.
            this.ViewModel!.FancyMousePreviewType = selectedPreviewType.ToString();
            this.ViewModel!.FancyMouseBackgroundColor1 = ColorHelper.SerializeToConfigColorString(
                ColorHelper.ToUnnamedColor(selectedPreviewStyle.CanvasStyle.BackgroundStyle.Color1));
            this.ViewModel!.FancyMouseBackgroundColor2 = ColorHelper.SerializeToConfigColorString(
                ColorHelper.ToUnnamedColor(selectedPreviewStyle.CanvasStyle.BackgroundStyle.Color2));
            this.ViewModel!.FancyMouseBorderThickness = (double)selectedPreviewStyle.CanvasStyle.BorderStyle.Top;
            this.ViewModel!.FancyMouseBorderColor = ColorHelper.SerializeToConfigColorString(
                ColorHelper.ToUnnamedColor(selectedPreviewStyle.CanvasStyle.BorderStyle.Color));
            this.ViewModel!.FancyMouseBorder3dDepth = (double)selectedPreviewStyle.CanvasStyle.BorderStyle.Depth;
            this.ViewModel!.FancyMouseBorderPadding = (double)selectedPreviewStyle.CanvasStyle.PaddingStyle.Top;
            this.ViewModel!.FancyMouseBezelThickness = (double)selectedPreviewStyle.ScreenStyle.BorderStyle.Top;
            this.ViewModel!.FancyMouseBezelColor = ColorHelper.SerializeToConfigColorString(
                ColorHelper.ToUnnamedColor(selectedPreviewStyle.ScreenStyle.BorderStyle.Color));
            this.ViewModel!.FancyMouseBezel3dDepth = (double)selectedPreviewStyle.ScreenStyle.BorderStyle.Depth;
            this.ViewModel!.FancyMouseScreenMargin = (double)selectedPreviewStyle.ScreenStyle.MarginStyle.Top;
            this.ViewModel!.FancyMouseScreenColor1 = ColorHelper.SerializeToConfigColorString(
                ColorHelper.ToUnnamedColor(selectedPreviewStyle.ScreenStyle.BackgroundStyle.Color1));
            this.ViewModel!.FancyMouseScreenColor2 = ColorHelper.SerializeToConfigColorString(
                ColorHelper.ToUnnamedColor(selectedPreviewStyle.ScreenStyle.BackgroundStyle.Color2));
        }

        private PreviewType GetSelectedPreviewType()
        {
            // this needs to match the order of the SegmentedItems in the "Preview Type" Segmented control
            var previewTypeOrder = new PreviewType[]
            {
                PreviewType.Compact, PreviewType.Bezelled, PreviewType.Custom,
            };

            var selectedIndex = this.PreviewTypeSetting.SelectedIndex;
            if ((selectedIndex < 0) || (selectedIndex >= previewTypeOrder.Length))
            {
                throw new InvalidOperationException();
            }

            return previewTypeOrder[selectedIndex];
        }
    }
}
