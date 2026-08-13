using System.Drawing;
using System.Drawing.Imaging;

using FancyMouse.Common.Helpers;
using FancyMouse.Models.Drawing;
using FancyMouse.Models.Layout;

using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;

using Image = Microsoft.UI.Xaml.Controls.Image;

namespace FancyMouse.WinUI3.UI;

/// <summary>
/// Encapsulates the preview pane's own content - the background rectangle and the
/// bezels/screenshots on top of it. Deliberately excludes the outer border, which is the
/// hosting window's responsibility (see <see cref="Common.Helpers.LayoutHelper.GetHostBoxStyle"/>).
/// The hosting window supplies the pre-computed <see cref="Layout"/> (this control doesn't
/// calculate its own size); the background image and each screen's bezel are rendered
/// internally from <see cref="Layout"/> as soon as it's set, and each screen starts out
/// showing a placeholder fill until the hosting window backfills its real screenshot via
/// <see cref="SetScreenshot"/> (screenshot capture needs the host's own capture pipeline,
/// which this control has no access to).
/// </summary>
public sealed partial class PreviewPane : UserControl
{
    public static readonly DependencyProperty LayoutProperty = DependencyProperty.Register(
        nameof(PreviewPane.Layout),
        typeof(PreviewLayout),
        typeof(PreviewPane),
        new PropertyMetadata(null, PreviewPane.OnLayoutChanged));

    private List<ScreenSlot> screenSlots = new();

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
    /// Raised when the pointer clicks a location that maps onto one of the screen bezels -
    /// <see cref="ScreenshotClickedEventArgs.Location"/> is already resolved to the
    /// corresponding physical location on that screen's own display area, so the host only
    /// needs to forward it to <c>MouseHelper.SetCursorPosition</c>.
    /// </summary>
    public event EventHandler<ScreenshotClickedEventArgs>? ScreenshotClicked;

    /// <summary>
    /// Backfills the real screenshot for a single screen, replacing its placeholder fill -
    /// called by the hosting window once its capture pipeline has produced the image for that
    /// screen. If <paramref name="screenLayout"/> isn't part of the current <see cref="Layout"/>
    /// any more (e.g. a newer activation replaced it while this capture was still in flight),
    /// the call is silently ignored rather than throwing - a stale, superseded capture result
    /// has nowhere left to go.
    /// </summary>
    public void SetScreenshot(ScreenLayout screenLayout, Bitmap image)
    {
        ArgumentNullException.ThrowIfNull(screenLayout);
        ArgumentNullException.ThrowIfNull(image);

        var slot = this.screenSlots.SingleOrDefault(
            s => object.ReferenceEquals(s.ScreenLayout, screenLayout));
        if (slot is null)
        {
            return;
        }

        slot.ContentImage.Source = PreviewPane.ToBitmapImage(image);
    }

    private static void OnLayoutChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        => ((PreviewPane)sender).ApplyLayout((PreviewLayout?)e.NewValue);

    private void ApplyLayout(PreviewLayout? layout)
    {
        this.ScreensCanvas.Children.Clear();
        this.screenSlots = new List<ScreenSlot>();

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

        this.screenSlots = layout.CanvasLayout.DeviceLayouts
            .SelectMany(deviceLayout => deviceLayout.ScreenLayouts)
            .Select(screenLayout => this.CreateScreenSlot(screenLayout, scale))
            .ToList();
    }

    /// <summary>
    /// Creates the visual elements for a single screen - a bezel (rendered the same way as the
    /// host's own outer border, see <see cref="DrawingHelper.RenderBorder"/>), a placeholder
    /// fill shown until the real screenshot arrives, and a content <see cref="Image"/> that
    /// starts blank and is populated later via <see cref="SetScreenshot"/>. The bezel and
    /// placeholder/content don't overlap - a screen's border ring and its content area are
    /// disjoint (see <see cref="Models.Drawing.BoxBounds"/>) - so paint order between them
    /// doesn't matter.
    /// </summary>
    private ScreenSlot CreateScreenSlot(ScreenLayout screenLayout, double scale)
    {
        var screenBounds = screenLayout.ScreenBounds;

        using (var bezelBitmap = DrawingHelper.RenderBorder(
            screenBounds.MoveTo(new PointInfo(0, 0)), screenLayout.ScreenStyle))
        {
            var bezelImage = new Image
            {
                Source = PreviewPane.ToBitmapImage(bezelBitmap),
                Stretch = Stretch.Fill,
            };
            PreviewPane.PositionElement(bezelImage, screenBounds.OuterBounds, scale);
            this.ScreensCanvas.Children.Add(bezelImage);
        }

        var placeholderColor = screenLayout.ScreenStyle.BackgroundStyle.Color1;
        if (placeholderColor is not null)
        {
            var placeholder = new Microsoft.UI.Xaml.Shapes.Rectangle
            {
                Fill = new SolidColorBrush(PreviewPane.ToWindowsColor(placeholderColor.Value)),
            };
            PreviewPane.PositionElement(placeholder, screenBounds.PaddingBounds, scale);
            this.ScreensCanvas.Children.Add(placeholder);
        }

        var contentImage = new Image
        {
            Stretch = Stretch.Fill,
        };
        PreviewPane.PositionElement(contentImage, screenBounds.ContentBounds, scale);
        this.ScreensCanvas.Children.Add(contentImage);

        return new ScreenSlot(screenLayout, contentImage);
    }

    private double GetRasterizationScale()
        => this.XamlRoot?.RasterizationScale ?? 1.0;

    private static void PositionElement(FrameworkElement element, RectangleInfo bounds, double scale)
    {
        element.Width = (double)bounds.Width / scale;
        element.Height = (double)bounds.Height / scale;
        Canvas.SetLeft(element, (double)bounds.X / scale);
        Canvas.SetTop(element, (double)bounds.Y / scale);
    }

    private static BitmapImage ToBitmapImage(Bitmap bitmap)
    {
        var bitmapImage = new BitmapImage();
        using var stream = new MemoryStream();
        bitmap.Save(stream, ImageFormat.Png);
        stream.Position = 0;
        bitmapImage.SetSource(stream.AsRandomAccessStream());
        return bitmapImage;
    }

    private static Windows.UI.Color ToWindowsColor(Color color)
        => Windows.UI.Color.FromArgb(color.A, color.R, color.G, color.B);

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

    private sealed class ScreenSlot
    {
        public ScreenSlot(ScreenLayout screenLayout, Image contentImage)
        {
            this.ScreenLayout = screenLayout;
            this.ContentImage = contentImage;
        }

        public ScreenLayout ScreenLayout
        {
            get;
        }

        public Image ContentImage
        {
            get;
        }
    }
}
