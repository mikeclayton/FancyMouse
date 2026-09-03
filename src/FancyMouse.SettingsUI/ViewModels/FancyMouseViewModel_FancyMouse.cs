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

#nullable disable

using System.Drawing;
using System.Drawing.Imaging;
using System.Reflection;
using System.Runtime.CompilerServices;

using FancyMouse.Common.Capture;
using FancyMouse.Common.Helpers;
using FancyMouse.HotKeys;
using FancyMouse.Models.Display;
using FancyMouse.Models.Drawing;
using FancyMouse.Models.Styles;
using FancyMouse.Settings.V2;
using FancyMouse.SettingsUI.Helpers;
using FancyMouse.SettingsUI.Models;

using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;

namespace FancyMouse.SettingsUI.ViewModels
{
    public sealed partial class FancyMouseViewModel
    {
        internal FancyMouseSettingsConfig FancyMouseSettingsConfig
        {
            get;
            set;
        }

        public Keystroke FancyMouseActivationShortcut
        {
            get
            {
                return Keystroke.TryParse(this.FancyMouseSettingsConfig.Properties.ActivationShortcut ?? string.Empty, out var keystroke)
                    ? keystroke
                    : null;
            }

            set
            {
                var text = value?.ToString();
                if (!string.Equals(text, this.FancyMouseSettingsConfig.Properties.ActivationShortcut, StringComparison.Ordinal))
                {
                    this.FancyMouseSettingsConfig.Properties.ActivationShortcut = text;
                    this.NotifyFancyMousePropertyChanged();
                }
            }
        }

        public FancyMouseThumbnailSize FancyMouseThumbnailSize
        {
            get
            {
                return FancyMouseSettingsConfig.Properties.ThumbnailSize;
            }

            set
            {
                // mirrors MouseUtilsViewModel.MouseJumpThumbnailSize's own (odd-looking - should
                // arguably be || not &&) change check verbatim, for parity with upstream.
                if ((FancyMouseSettingsConfig.Properties.ThumbnailSize.Width != value?.Width)
                    && (FancyMouseSettingsConfig.Properties.ThumbnailSize.Height != value?.Height))
                {
                    FancyMouseSettingsConfig.Properties.ThumbnailSize = value!;
                    NotifyFancyMousePropertyChanged();
                }
            }
        }

        private static Bitmap LoadImageResource(string filename)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var assemblyName = new AssemblyName(assembly.FullName ?? throw new InvalidOperationException());

            // Build the fully-qualified manifest resource name. Historically, subtle casing differences
            // (e.g. folder names or the assembly name) caused exact (case-sensitive) lookup failures on
            // some developer machines when the embedded resource's actual name differed only by case.
            // Manifest resource name comparison here does not need to be case-sensitive, so we resolve
            // the actual name using an OrdinalIgnoreCase match, then use the real casing for the stream.
            var resourceName = $"{assemblyName.Name}.{filename.Replace("/", ".")}";
            var resourceNames = assembly.GetManifestResourceNames();
            var actualResourceName = resourceNames.FirstOrDefault(n => string.Equals(n, resourceName, StringComparison.OrdinalIgnoreCase));
            if (actualResourceName is null)
            {
                throw new InvalidOperationException($"Embedded resource '{resourceName}' (case-insensitive) does not exist.");
            }

            var stream = assembly.GetManifestResourceStream(actualResourceName)
                ?? throw new InvalidOperationException();
            var image = (Bitmap)Image.FromStream(stream);
            return image;
        }

#pragma warning disable SA1306 // Field should begin with lower-case letter
        private static Lazy<Bitmap> FancyMouseDesktopImage = new(
            () => FancyMouseViewModel.LoadImageResource("Assets/PreviewDesktop.png"));
#pragma warning restore SA1306

        public ImageSource FancyMousePreviewImage
        {
            get
            {
                // build the display info used to generate the preview image in the settings dialog
                // (keep the values in sync with the layout of "Assets/PreviewDesktop.png")
                var displayInfo = new DisplayInfo([
                    new DeviceInfo(
                        hostname: "FakeDisplay1",
                        localhost: true,
                        screens: [
                            /*
                                these magic numbers are the pixel dimensions of the individual screens on the
                                fake desktop image - "Images\MouseJump-Desktop.png" - used to generate the
                                preview image in the Settings UI properties page for Mouse Jump. if you update
                                the fake desktop image be sure to update these values as well.
                            */
                            new(
                                handle: 0,
                                primary: false,
                                displayArea: new RectangleInfo(635, 172, 272, 168),
                                workingArea: new RectangleInfo(635, 172, 272, 168)),
                            new(
                                handle: 0,
                                primary: true,
                                displayArea: new RectangleInfo(0, 0, 635, 339),
                                workingArea: new RectangleInfo(0, 0, 635, 339)),
                        ]),
                ]);

                var desktopSize = LayoutHelper.GetCombinedScreenBounds(
                        displayInfo.Devices[0].Screens
                            .Select(screen => screen.DisplayArea)
                            .ToList())
                    .Size;

                /*
                    magic number 283 is the content height left in the settings card after removing the top and bottom chrome:

                        300px settings card height - 1px top border - 7px top margin - 8px bottom margin - 1px bottom border = 283px image height

                    this ensures we get a preview image scaled at 100% so borders, etc., are shown at exact pixel sizes in the preview
                */
                const int settingsCardHeight = 300;
                const int settingsCardTopBorder = 1;
                const int settingsCardTopMargin = 7;
                const int settingsCardBottomMargin = 8;
                const int settingsCardBottomBorder = 1;
                const int settingsCardContentHeight = // 283
                    settingsCardHeight - settingsCardTopBorder - settingsCardTopMargin - settingsCardBottomBorder - settingsCardBottomMargin;

                var canvasSize = new SizeInfo(desktopSize.Width, settingsCardContentHeight).Clamp(desktopSize);

                var previewType = Enum.TryParse<PreviewType>(this.FancyMousePreviewType, ignoreCase: true, out var previewTypeResult)
                    ? previewTypeResult
                    : PreviewType.Bezelled;

                var previewStyle = previewType switch
                {
                    PreviewType.Compact => StyleHelper.CompactPreviewStyle.WithCanvasSize(canvasSize),
                    PreviewType.Bezelled => StyleHelper.BezelledPreviewStyle.WithCanvasSize(canvasSize),
                    PreviewType.Custom => this.ToPreviewStyle(extraColors: []).WithCanvasSize(canvasSize),
                    _ => this.ToPreviewStyle(extraColors: []).WithCanvasSize(canvasSize),
                };

                // StaticScreenshotCaptureProvider does no real async work (it just crops/scales
                // an in-memory Bitmap), so blocking on it here is safe - this getter is a
                // synchronous XAML binding and has no natural async path of its own.
                var captureProvider = new StaticScreenshotCaptureProvider(FancyMouseViewModel.FancyMouseDesktopImage.Value);
                using var previewBitmap = PreviewHelper.ComposePreviewAsync(previewStyle, captureProvider, displayInfo).GetAwaiter().GetResult();

                // save the image to a memory stream
                using var stream = new MemoryStream();
                previewBitmap.Save(stream, ImageFormat.Png);
                stream.Position = 0;

                // load the memory stream into a bitmap image
                var bitmapImage = new BitmapImage();
                var rnd = stream.AsRandomAccessStream();

                // DecodePixelWidth/Height default to physical pixels (DecodePixelType.Physical),
                // so on a display scaled above 100% the decoded bitmap shows smaller in DIPs than
                // the DIP-sized SettingsCard/Grid around it, rather than filling it - Logical
                // treats these as DIPs instead, letting the system decode at the right physical
                // resolution for the current display scale.
                bitmapImage.DecodePixelType = DecodePixelType.Logical;
                bitmapImage.DecodePixelWidth = previewBitmap.Width;
                bitmapImage.DecodePixelHeight = previewBitmap.Height;
                bitmapImage.SetSource(rnd);
                return bitmapImage;
            }
        }

        public PreviewStyle ToPreviewStyle(IEnumerable<Color> extraColors)
        {
            return new PreviewStyle(
                canvasSize: new SizeInfo((decimal)this.FancyMouseThumbnailSize.Width, (decimal)this.FancyMouseThumbnailSize.Height),
                canvasStyle: new BoxStyle(
                    marginStyle: MarginStyle.Empty,
                    borderStyle: new BorderStyle(
                        color: ColorHelper.DeserializeFromConfigColorString(this.FancyMouseBorderColor),
                        all: (decimal)this.FancyMouseBorderThickness,
                        depth: (decimal)this.FancyMouseBorder3dDepth),
                    paddingStyle: new PaddingStyle(all: (decimal)this.FancyMouseBorderPadding),
                    backgroundStyle: new BackgroundStyle(
                        color1: ColorHelper.DeserializeFromConfigColorString(this.FancyMouseBackgroundColor1),
                        color2: ColorHelper.DeserializeFromConfigColorString(this.FancyMouseBackgroundColor2))),
                screenStyle: new BoxStyle(
                    marginStyle: new MarginStyle(all: (decimal)this.FancyMouseScreenMargin),
                    borderStyle: new BorderStyle(
                        color: ColorHelper.DeserializeFromConfigColorString(this.FancyMouseBezelColor),
                        all: (decimal)this.FancyMouseBezelThickness,
                        depth: (decimal)this.FancyMouseBezel3dDepth),
                    paddingStyle: PaddingStyle.Empty,
                    backgroundStyle: new BackgroundStyle(
                        color1: ColorHelper.DeserializeFromConfigColorString(this.FancyMouseScreenColor1),
                        color2: ColorHelper.DeserializeFromConfigColorString(this.FancyMouseScreenColor2))),
                extraColors: extraColors);
        }

        public string FancyMousePreviewType
        {
            get
            {
                return FancyMouseSettingsConfig.Properties.PreviewType;
            }

            set
            {
                if (value != FancyMouseSettingsConfig.Properties.PreviewType)
                {
                    FancyMouseSettingsConfig.Properties.PreviewType = value;
                    NotifyFancyMousePropertyChanged();
                    NotifyFancyMousePropertyChanged(nameof(this.FancyMousePreviewImage));
                }
            }
        }

        public string FancyMouseBackgroundColor1
        {
            get
            {
                var value = FancyMouseSettingsConfig.Properties.BackgroundColor1;
                value = (value != null) ? SettingsUtilities.ToRGBHex(value) : "#000000";
                return value;
            }

            set
            {
                value = (value != null) ? SettingsUtilities.ToRGBHex(value) : "#000000";
                if (!value.Equals(FancyMouseSettingsConfig.Properties.BackgroundColor1, StringComparison.OrdinalIgnoreCase))
                {
                    FancyMouseSettingsConfig.Properties.BackgroundColor1 = value;
                    NotifyFancyMousePropertyChanged();
                    NotifyFancyMousePropertyChanged(nameof(this.FancyMousePreviewImage));
                }
            }
        }

        public string FancyMouseBackgroundColor2
        {
            get
            {
                var value = FancyMouseSettingsConfig.Properties.BackgroundColor2;
                value = (value != null) ? SettingsUtilities.ToRGBHex(value) : "#000000";
                return value;
            }

            set
            {
                value = (value != null) ? SettingsUtilities.ToRGBHex(value) : "#000000";
                if (!value.Equals(FancyMouseSettingsConfig.Properties.BackgroundColor2, StringComparison.OrdinalIgnoreCase))
                {
                    FancyMouseSettingsConfig.Properties.BackgroundColor2 = value;
                    NotifyFancyMousePropertyChanged();
                    NotifyFancyMousePropertyChanged(nameof(this.FancyMousePreviewImage));
                }
            }
        }

        public double FancyMouseBorderThickness
        {
            get
            {
                return FancyMouseSettingsConfig.Properties.BorderThickness;
            }

            set
            {
                if (value != FancyMouseSettingsConfig.Properties.BorderThickness)
                {
                    FancyMouseSettingsConfig.Properties.BorderThickness = value;
                    NotifyFancyMousePropertyChanged();
                    NotifyFancyMousePropertyChanged(nameof(this.FancyMousePreviewImage));
                }
            }
        }

        public string FancyMouseBorderColor
        {
            get
            {
                var value = FancyMouseSettingsConfig.Properties.BorderColor;
                value = (value != null) ? SettingsUtilities.ToRGBHex(value) : "#000000";
                return value;
            }

            set
            {
                value = (value != null) ? SettingsUtilities.ToRGBHex(value) : "#000000";
                if (!value.Equals(FancyMouseSettingsConfig.Properties.BorderColor, StringComparison.OrdinalIgnoreCase))
                {
                    FancyMouseSettingsConfig.Properties.BorderColor = value;
                    NotifyFancyMousePropertyChanged();
                    NotifyFancyMousePropertyChanged(nameof(this.FancyMousePreviewImage));
                }
            }
        }

        public double FancyMouseBorder3dDepth
        {
            get
            {
                return FancyMouseSettingsConfig.Properties.Border3dDepth;
            }

            set
            {
                if (value != FancyMouseSettingsConfig.Properties.Border3dDepth)
                {
                    FancyMouseSettingsConfig.Properties.Border3dDepth = value;
                    NotifyFancyMousePropertyChanged();
                    NotifyFancyMousePropertyChanged(nameof(this.FancyMousePreviewImage));
                }
            }
        }

        public double FancyMouseBorderPadding
        {
            get
            {
                return FancyMouseSettingsConfig.Properties.BorderPadding;
            }

            set
            {
                if (value != FancyMouseSettingsConfig.Properties.BorderPadding)
                {
                    FancyMouseSettingsConfig.Properties.BorderPadding = value;
                    NotifyFancyMousePropertyChanged();
                    NotifyFancyMousePropertyChanged(nameof(this.FancyMousePreviewImage));
                }
            }
        }

        public double FancyMouseBezelThickness
        {
            get
            {
                return this.FancyMouseSettingsConfig.Properties.BezelThickness;
            }

            set
            {
                if (value != FancyMouseSettingsConfig.Properties.BezelThickness)
                {
                    FancyMouseSettingsConfig.Properties.BezelThickness = value;
                    NotifyFancyMousePropertyChanged();
                    NotifyFancyMousePropertyChanged(nameof(this.FancyMousePreviewImage));
                }
            }
        }

        public string FancyMouseBezelColor
        {
            get
            {
                var value = FancyMouseSettingsConfig.Properties.BezelColor;
                value = (value != null) ? SettingsUtilities.ToRGBHex(value) : "#000000";
                return value;
            }

            set
            {
                value = (value != null) ? SettingsUtilities.ToRGBHex(value) : "#000000";
                if (!value.Equals(FancyMouseSettingsConfig.Properties.BezelColor, StringComparison.OrdinalIgnoreCase))
                {
                    FancyMouseSettingsConfig.Properties.BezelColor = value;
                    NotifyFancyMousePropertyChanged();
                    NotifyFancyMousePropertyChanged(nameof(this.FancyMousePreviewImage));
                }
            }
        }

        public double FancyMouseBezel3dDepth
        {
            get
            {
                return FancyMouseSettingsConfig.Properties.Bezel3dDepth;
            }

            set
            {
                if (value != FancyMouseSettingsConfig.Properties.Bezel3dDepth)
                {
                    FancyMouseSettingsConfig.Properties.Bezel3dDepth = value;
                    NotifyFancyMousePropertyChanged();
                    NotifyFancyMousePropertyChanged(nameof(this.FancyMousePreviewImage));
                }
            }
        }

        public double FancyMouseScreenMargin
        {
            get
            {
                return FancyMouseSettingsConfig.Properties.ScreenMargin;
            }

            set
            {
                if (value != FancyMouseSettingsConfig.Properties.ScreenMargin)
                {
                    FancyMouseSettingsConfig.Properties.ScreenMargin = value;
                    NotifyFancyMousePropertyChanged();
                    NotifyFancyMousePropertyChanged(nameof(this.FancyMousePreviewImage));
                }
            }
        }

        public string FancyMouseScreenColor1
        {
            get
            {
                var value = FancyMouseSettingsConfig.Properties.ScreenColor1;
                value = (value != null) ? SettingsUtilities.ToRGBHex(value) : "#000000";
                return value;
            }

            set
            {
                value = (value != null) ? SettingsUtilities.ToRGBHex(value) : "#000000";
                if (!value.Equals(FancyMouseSettingsConfig.Properties.ScreenColor1, StringComparison.OrdinalIgnoreCase))
                {
                    FancyMouseSettingsConfig.Properties.ScreenColor1 = value;
                    NotifyFancyMousePropertyChanged();
                    NotifyFancyMousePropertyChanged(nameof(this.FancyMousePreviewImage));
                }
            }
        }

        public string FancyMouseScreenColor2
        {
            get
            {
                var value = FancyMouseSettingsConfig.Properties.ScreenColor2;
                value = (value != null) ? SettingsUtilities.ToRGBHex(value) : "#000000";
                return value;
            }

            set
            {
                value = (value != null) ? SettingsUtilities.ToRGBHex(value) : "#000000";
                if (!value.Equals(FancyMouseSettingsConfig.Properties.ScreenColor2, StringComparison.OrdinalIgnoreCase))
                {
                    FancyMouseSettingsConfig.Properties.ScreenColor2 = value;
                    NotifyFancyMousePropertyChanged();
                    NotifyFancyMousePropertyChanged(nameof(this.FancyMousePreviewImage));
                }
            }
        }

        public void FancyMouseThumbnailSizePropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            NotifyFancyMousePropertyChanged(nameof(FancyMouseThumbnailSize));
        }

        public void NotifyFancyMousePropertyChanged([CallerMemberName] string propertyName = null)
        {
            OnPropertyChanged(propertyName);
        }
    }
}
