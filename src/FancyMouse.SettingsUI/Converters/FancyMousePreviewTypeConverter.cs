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

using FancyMouse.Settings.V2;

using Microsoft.UI.Xaml.Data;

namespace FancyMouse.SettingsUI.Converters
{
    public sealed partial class FancyMousePreviewTypeConverter : IValueConverter
    {
        private static readonly PreviewType[] PreviewTypeOrder =
        [
            PreviewType.Compact, PreviewType.Bezelled, PreviewType.Custom,
        ];

        private static readonly PreviewType DefaultPreviewType = PreviewType.Bezelled;

        /// <summary>
        /// Receives a string and returns an int representing the index to select in the Segmented
        /// control on the FancyMouse settings page.
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var previewType = FancyMousePreviewTypeConverter.DefaultPreviewType;

            if (value is not string previewTypeName)
            {
                // the value isn't a string so just use the default preview type
            }
            else if (Enum.IsDefined(typeof(PreviewType), previewTypeName))
            {
                // there's a case-sensitive match for the value
                previewType = Enum.Parse<PreviewType>(previewTypeName);
            }
            else if (Enum.TryParse<PreviewType>(previewTypeName, true, out var previewTypeResult))
            {
                // there's a case-insensitive match for the value
                previewType = previewTypeResult;
            }

            return Array.IndexOf(
                FancyMousePreviewTypeConverter.PreviewTypeOrder,
                previewType);
        }

        /// <summary>
        /// Receives an int representing the selected index in the Segmented control on the FancyMouse
        /// settings page, and returns the name of the <see cref="PreviewType"/> for that index.
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            var previewType = FancyMousePreviewTypeConverter.DefaultPreviewType;

            if (value is not int segmentedIndex)
            {
                // the value isn't an int so just use the default preview type
            }
            else if ((segmentedIndex < 0) || (segmentedIndex > FancyMousePreviewTypeConverter.PreviewTypeOrder.Length))
            {
                // not a valid selected index so just use the default preview type
            }
            else
            {
                previewType = FancyMousePreviewTypeConverter.PreviewTypeOrder[segmentedIndex];
            }

            return previewType.ToString();
        }
    }
}
