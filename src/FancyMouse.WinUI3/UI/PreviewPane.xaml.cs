using System.Drawing;
using System.Drawing.Imaging;

using FancyMouse.Common.Helpers;
using FancyMouse.Models.Drawing;
using FancyMouse.Models.Layout;

using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Imaging;

namespace FancyMouse.WinUI3.UI;

/// <summary>
/// Encapsulates the preview pane's own content - the background rectangle and the
/// bezels/screenshots on top of it. Deliberately excludes the outer border, which is the
/// hosting window's responsibility (see <see cref="Common.Helpers.LayoutHelper.GetHostBoxStyle"/>).
/// The hosting window supplies the pre-computed <see cref="Layout"/> (this control doesn't
/// calculate its own size) and the already-rendered <see cref="ScreenshotsImage"/> (screenshot
/// capture needs the host's own capture-service pipeline); the background image is rendered
/// internally from <see cref="Layout"/>.
/// </summary>
public sealed partial class PreviewPane : UserControl
{
    public static readonly DependencyProperty LayoutProperty = DependencyProperty.Register(
        nameof(PreviewPane.Layout),
        typeof(PreviewLayout),
        typeof(PreviewPane),
        new PropertyMetadata(null, PreviewPane.OnLayoutChanged));

    public static readonly DependencyProperty ScreenshotsImageProperty = DependencyProperty.Register(
        nameof(PreviewPane.ScreenshotsImage),
        typeof(Bitmap),
        typeof(PreviewPane),
        new PropertyMetadata(null, PreviewPane.OnScreenshotsImageChanged));

    public PreviewPane()
    {
        this.InitializeComponent();
        this.PointerPressed += this.PreviewPane_PointerPressed;
    }

    /// <summary>
    /// Gets or sets the mathematical model describing this pane's own size and the position/style of
    /// every device/screen bezel within it. Set by the hosting window - this control doesn't
    /// calculate its own size from anything else.
    /// </summary>
    public PreviewLayout? Layout
    {
        get => (PreviewLayout?)this.GetValue(PreviewPane.LayoutProperty);
        set => this.SetValue(PreviewPane.LayoutProperty, value);
    }

    /// <summary>
    /// Gets or sets the combined bezels/screenshots image, already rendered by the hosting window (see
    /// <see cref="Common.Helpers.DrawingHelper.RenderPreviewAsync"/>) - screenshot capture
    /// needs the host's own capture-service pipeline, which this control has no access to.
    /// </summary>
    /// <remarks>
    /// STOPGAP - <see cref="Common.Helpers.DrawingHelper.RenderPreviewAsync"/> mutates and
    /// re-passes the *same* <see cref="Bitmap"/> instance across its progressive render
    /// callbacks, but WinUI's DependencyProperty change-detection uses reference equality for
    /// reference types - so re-assigning the same reference would otherwise be silently
    /// treated as "no change" and never re-render, no matter how much the bitmap's actual
    /// pixel content has changed since the last assignment. Toggling through <see langword="null"/>
    /// forces the change to register every time. The real fix is expected to arrive with the
    /// "stage 2" per-screen capture pipeline, which replaces this shared-mutable-bitmap shape
    /// entirely - see the KNOWN ISSUE note in <c>DrawingHelper.RenderCombinedPreviewAsync</c>
    /// for the related (independent, still-unfixed) spam-activation race this same design
    /// causes on the host side.
    /// </remarks>
    public Bitmap? ScreenshotsImage
    {
        get => (Bitmap?)this.GetValue(PreviewPane.ScreenshotsImageProperty);
        set
        {
            this.SetValue(PreviewPane.ScreenshotsImageProperty, null);
            this.SetValue(PreviewPane.ScreenshotsImageProperty, value);
        }
    }

    /// <summary>
    /// Raised when the pointer clicks a location that maps onto one of the screen bezels -
    /// <see cref="ScreenshotClickedEventArgs.Location"/> is already resolved to the
    /// corresponding physical location on that screen's own display area, so the host only
    /// needs to forward it to <c>MouseHelper.SetCursorPosition</c>.
    /// </summary>
    public event EventHandler<ScreenshotClickedEventArgs>? ScreenshotClicked;

    private static void OnLayoutChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        => ((PreviewPane)sender).ApplyLayout((PreviewLayout?)e.NewValue);

    private static void OnScreenshotsImageChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        => ((PreviewPane)sender).ApplyScreenshotsImage((Bitmap?)e.NewValue);

    private void ApplyLayout(PreviewLayout? layout)
    {
        if (layout is null)
        {
            this.Width = 0;
            this.Height = 0;
            this.BackgroundImage.Source = null;
            return;
        }

        var scale = this.GetRasterizationScale();
        var bounds = layout.CanvasLayout.CanvasBounds.OuterBounds;
        this.Width = (double)bounds.Width / scale;
        this.Height = (double)bounds.Height / scale;

        using var backgroundBitmap = DrawingHelper.RenderBackground(layout.CanvasLayout);
        this.BackgroundImage.Source = PreviewPane.ToBitmapImage(backgroundBitmap);
    }

    private void ApplyScreenshotsImage(Bitmap? image)
    {
        this.ScreenshotsImageControl.Source = (image is null)
            ? null
            : PreviewPane.ToBitmapImage(image);
    }

    private double GetRasterizationScale()
        => this.XamlRoot?.RasterizationScale ?? 1.0;

    private static BitmapImage ToBitmapImage(Bitmap bitmap)
    {
        var bitmapImage = new BitmapImage();
        using var stream = new MemoryStream();
        bitmap.Save(stream, ImageFormat.Png);
        stream.Position = 0;
        bitmapImage.SetSource(stream.AsRandomAccessStream());
        return bitmapImage;
    }

    private void PreviewPane_PointerPressed(object sender, PointerRoutedEventArgs e)
    {
        if (!e.Pointer.PointerDeviceType.Equals(PointerDeviceType.Mouse))
        {
            // not a mouse click
            return;
        }

        var pointerPoint = e.GetCurrentPoint(this);
        if (!pointerPoint.Properties.IsLeftButtonPressed)
        {
            return;
        }

        var layout = this.Layout;
        if (layout is null)
        {
            return;
        }

        // the pointer position is in DIPs, relative to this control - scale it up to the
        // same physical-pixel space that Layout's bounds are expressed in
        var scale = this.GetRasterizationScale();
        var pointerLocation = new PointInfo((decimal)pointerPoint.Position.X, (decimal)pointerPoint.Position.Y)
            .Scale((decimal)scale);

        // work out which screenshot was clicked
        var clickedScreen = layout.CanvasLayout.DeviceLayouts
            .SelectMany(deviceLayout => deviceLayout.ScreenLayouts)
            .SingleOrDefault(
                screenLayout => screenLayout.ScreenBounds.OuterBounds.Contains(pointerLocation));
        if (clickedScreen is null)
        {
            return;
        }

        // scale up the click onto the physical screen - the aspect ratio of the screenshot
        // might be distorted compared to the physical screen due to the borders around the
        // screenshot, so we need to work out the target location on the physical screen first
        var clickedDisplayArea = clickedScreen.ScreenInfo.DisplayArea;
        var clickedLocation = pointerLocation
            .Stretch(
                source: clickedScreen.ScreenBounds.ContentBounds,
                target: clickedDisplayArea)
            .Clamp(
                new(
                    x: clickedDisplayArea.X + 1,
                    y: clickedDisplayArea.Y + 1,
                    width: clickedDisplayArea.Width - 1,
                    height: clickedDisplayArea.Height - 1))
            .Truncate();

        this.ScreenshotClicked?.Invoke(this, new ScreenshotClickedEventArgs(clickedLocation));
    }
}
