using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;

using FancyMouse.Common.Helpers;
using FancyMouse.Models.Display;
using FancyMouse.Models.Drawing;
using FancyMouse.Models.Layout;
using FancyMouse.Models.Styles;

using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;

using Windows.System;

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
/// which this control has no access to). This control also owns turning raw mouse/keyboard
/// input into navigation intent - see <see cref="NavigateTo"/>/<see cref="Cancel"/> - so the
/// host doesn't need its own copy of "which screen is that" or "which screen is next" logic.
/// </summary>
public sealed partial class PreviewPane : UserControl
{
    public static readonly DependencyProperty LayoutProperty = DependencyProperty.Register(
        nameof(PreviewPane.Layout),
        typeof(PreviewLayout),
        typeof(PreviewPane),
        new PropertyMetadata(null, PreviewPane.OnLayoutChanged));

    public static readonly DependencyProperty ActiveScreenProperty = DependencyProperty.Register(
        nameof(PreviewPane.ActiveScreen),
        typeof(ScreenInfo),
        typeof(PreviewPane),
        new PropertyMetadata(null));

    /// <summary>
    /// How strongly <see cref="GetHistoricalPlaceholder"/>'s stand-in screenshots are blurred -
    /// see <see cref="BlurHelper.CreateBlurredCopy"/> for what the value means.
    /// </summary>
    private const decimal BlurIntensity = 0.15m;

    /// <summary>
    /// How long a screen's last real screenshot remains a reasonable stand-in for a fresh one -
    /// see <see cref="GetHistoricalPlaceholder"/>.
    /// </summary>
    private static readonly TimeSpan ScreenshotHistoryMaxAge = TimeSpan.FromMinutes(2);

    private List<ScreenSlot> screenSlots = new();

    /// <summary>
    /// Each screen's last real screenshot, blurred, for use as a stand-in placeholder while a
    /// fresh one is loading (see <see cref="GetHistoricalPlaceholder"/>/<see cref="SetScreenshot"/>).
    /// Indexed the same way as <see cref="screenSlots"/> - position within the flattened
    /// device/screen list - rather than tied to any particular <see cref="ScreenSlot"/> instance,
    /// so a screen's history survives being hidden/reshown even though <see cref="ApplyLayout"/>
    /// throws <see cref="screenSlots"/> itself away every time the window closes.
    /// </summary>
    private List<ScreenshotHistoryEntry?> screenshotHistory = new();

    public PreviewPane()
    {
        this.InitializeComponent();
        this.PointerPressed += this.PreviewPane_PointerPressed;
        this.PreviewKeyDown += this.PreviewPane_PreviewKeyDown;
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
    /// Gets or sets the screen that keyboard navigation (Left/Right in particular) is relative
    /// to - set by the hosting window alongside <see cref="Layout"/>, normally to whichever
    /// screen was activated. This control never changes it itself: every navigation key acts
    /// immediately and the host is expected to close the preview afterwards (see
    /// <see cref="NavigateTo"/>), so there's no in-preview "browse mode" that would need this to
    /// track a moving selection.
    /// </summary>
    public ScreenInfo? ActiveScreen
    {
        get => (ScreenInfo?)this.GetValue(PreviewPane.ActiveScreenProperty);
        set => this.SetValue(PreviewPane.ActiveScreenProperty, value);
    }

    /// <summary>
    /// Raised when the pointer clicks a screen, or a keyboard shortcut that means the same
    /// thing (1-9, arrow keys relative to <see cref="ActiveScreen"/>, P for primary, Home/End)
    /// resolves to one. <see cref="NavigateToEventArgs.Location"/> is already resolved to the
    /// corresponding physical location on that screen's own display area.
    /// </summary>
    public event EventHandler<NavigateToEventArgs>? NavigateTo;

    /// <summary>
    /// Raised on a right-click or Escape - the host is expected to just close the preview
    /// without moving the pointer anywhere.
    /// </summary>
    public event EventHandler? Cancel;

    /// <summary>
    /// Backfills the real screenshot for a single screen, replacing its placeholder fill -
    /// called by the hosting window once its capture pipeline has produced the image for that
    /// screen. If <paramref name="screenLayout"/> isn't part of the current <see cref="Layout"/>
    /// any more (e.g. a newer activation replaced it while this capture was still in flight),
    /// the call is silently ignored rather than throwing - a stale, superseded capture result
    /// has nowhere left to go.
    /// </summary>
    /// <remarks>
    /// Also stashes a blurred copy of <paramref name="image"/> in <see cref="screenshotHistory"/>
    /// at this screen's position, for <see cref="GetHistoricalPlaceholder"/> to use as the next
    /// activation's initial placeholder while its own fresh screenshot is still loading. Blurring
    /// is done inline here, on the UI thread - at thumbnail sizes it's fast enough (see
    /// <see cref="BlurHelper.CreateBlurredCopy"/>) not to be worth moving off it.
    /// </remarks>
    public void SetScreenshot(ScreenLayout screenLayout, Bitmap image)
    {
        ArgumentNullException.ThrowIfNull(screenLayout);
        ArgumentNullException.ThrowIfNull(image);

        var index = this.screenSlots.FindIndex(
            s => object.ReferenceEquals(s.ScreenLayout, screenLayout));
        if (index < 0)
        {
            return;
        }

        this.screenSlots[index].ContentImage.Source = PreviewPane.ToBitmapImage(image);

        using var blurredImage = BlurHelper.CreateBlurredCopy(image, PreviewPane.BlurIntensity);
        this.screenshotHistory[index] = new ScreenshotHistoryEntry(
            PreviewPane.ToBitmapImage(blurredImage), DateTime.UtcNow);
    }

    /// <summary>
    /// Returns this position's last real screenshot, blurred, if it's still fresh enough to be a
    /// reasonable stand-in for a screen whose current activation hasn't captured yet - or
    /// <see langword="null"/> (falling back to the plain placeholder fill) if there isn't one or
    /// it's older than <see cref="ScreenshotHistoryMaxAge"/>.
    /// </summary>
    private ImageSource? GetHistoricalPlaceholder(int index)
    {
        if (index >= this.screenshotHistory.Count)
        {
            return null;
        }

        var entry = this.screenshotHistory[index];
        if (entry is null || (DateTime.UtcNow - entry.CapturedAt) > PreviewPane.ScreenshotHistoryMaxAge)
        {
            return null;
        }

        return entry.BlurredImage;
    }

    private static void OnLayoutChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        => ((PreviewPane)sender).ApplyLayout((PreviewLayout?)e.NewValue);

    private void ApplyLayout(PreviewLayout? layout)
    {
        if (layout is null)
        {
            this.ScreensCanvas.Children.Clear();
            this.screenSlots = [];
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

        var newScreenLayouts = layout.CanvasLayout.DeviceLayouts
            .SelectMany(deviceLayout => deviceLayout.ScreenLayouts)
            .ToList();

        // keep screenshotHistory sized to match, indexed the same way as newSlots below. Entries
        // within range survive regardless of whether their slot gets reused or rebuilt this pass
        // (see the screenshotHistory field remarks) - only entries past the new count (e.g. a
        // monitor was unplugged) get dropped.
        while (this.screenshotHistory.Count < newScreenLayouts.Count)
        {
            this.screenshotHistory.Add(null);
        }

        if (this.screenshotHistory.Count > newScreenLayouts.Count)
        {
            this.screenshotHistory.RemoveRange(
                newScreenLayouts.Count, this.screenshotHistory.Count - newScreenLayouts.Count);
        }

        var newSlots = new List<ScreenSlot>(newScreenLayouts.Count);
        for (var i = 0; i < newScreenLayouts.Count; i++)
        {
            var screenLayout = newScreenLayouts[i];
            var previousSlot = (i < this.screenSlots.Count) ? this.screenSlots[i] : null;

            if (previousSlot is not null && PreviewPane.CanReuse(previousSlot.ScreenLayout, screenLayout))
            {
                // pixel-identical bezel/placeholder to what's already on screen at this
                // position - keep the visuals, just point the slot at the new ScreenLayout
                // instance (SetScreenshot looks slots up by reference) and clear the content
                // image (falling back to this position's blurred screenshot history, if any -
                // see GetHistoricalPlaceholder) since the screenshot it was showing is now stale
                // desktop state regardless of whether the bezel itself changed
                previousSlot.ContentImage.Source = this.GetHistoricalPlaceholder(i);

                // CanReuse only guarantees the *physical-pixel* bounds are unchanged - the
                // DIP-space Width/Height/Canvas.Left/Top these elements were last positioned
                // at also depend on scale (see GetRasterizationScale), which can differ from
                // last time if this activation's window landed on a different-DPI monitor
                // than the previous one. Reapplying position/size here is cheap (a handful of
                // property writes, no re-render) and keeps that in sync even though the
                // visuals themselves are being reused as-is.
                var screenBounds = screenLayout.ScreenBounds;
                PreviewPane.PositionElement(previousSlot.BezelImage, screenBounds.OuterBounds, scale);
                if (previousSlot.PlaceholderRectangle is not null)
                {
                    PreviewPane.PositionElement(previousSlot.PlaceholderRectangle, screenBounds.PaddingBounds, scale);
                }

                PreviewPane.PositionElement(previousSlot.ContentImage, screenBounds.ContentBounds, scale);

                newSlots.Add(new ScreenSlot(
                    screenLayout, previousSlot.BezelImage, previousSlot.PlaceholderRectangle, previousSlot.ContentImage));
            }
            else
            {
                this.RemoveScreenSlot(previousSlot);
                newSlots.Add(this.CreateScreenSlot(screenLayout, scale, i));
            }
        }

        // trim any slots left over from a layout that had more screens than this one does
        // (e.g. a monitor was unplugged) - everything up to newScreenLayouts.Count was
        // already handled (reused or replaced) above
        for (var i = newScreenLayouts.Count; i < this.screenSlots.Count; i++)
        {
            this.RemoveScreenSlot(this.screenSlots[i]);
        }

        this.screenSlots = newSlots;
    }

    /// <summary>
    /// Reports whether <paramref name="current"/> would produce a pixel-identical bezel and
    /// placeholder to <paramref name="previous"/> - if so, <see cref="ApplyLayout"/> reuses the
    /// existing visuals instead of re-rendering them. Deliberately narrow: only the fields that
    /// actually feed <see cref="CreateScreenSlot"/> are compared (the three bounds rectangles
    /// used for positioning, the border style used for the bezel, and the background color used
    /// for the placeholder) - margin/padding style and the rest of <see cref="BoxStyle"/> only
    /// matter insofar as they already show up in the bounds, so comparing bounds directly covers
    /// them without needing to compare every style field individually.
    /// </summary>
    private static bool CanReuse(ScreenLayout previous, ScreenLayout current)
        => PreviewPane.CanReuse(previous.ScreenBounds, current.ScreenBounds)
        && PreviewPane.CanReuse(previous.ScreenStyle.BorderStyle, current.ScreenStyle.BorderStyle)
        && previous.ScreenStyle.BackgroundStyle.Color1 == current.ScreenStyle.BackgroundStyle.Color1;

    private static bool CanReuse(BoxBounds previous, BoxBounds current)
        => PreviewPane.CanReuse(previous.OuterBounds, current.OuterBounds)
        && PreviewPane.CanReuse(previous.PaddingBounds, current.PaddingBounds)
        && PreviewPane.CanReuse(previous.ContentBounds, current.ContentBounds);

    private static bool CanReuse(RectangleInfo previous, RectangleInfo current)
        => previous.X == current.X
        && previous.Y == current.Y
        && previous.Width == current.Width
        && previous.Height == current.Height;

    private static bool CanReuse(BorderStyle previous, BorderStyle current)
        => previous.Color == current.Color
        && previous.Left == current.Left
        && previous.Top == current.Top
        && previous.Right == current.Right
        && previous.Bottom == current.Bottom
        && previous.Depth == current.Depth;

    private void RemoveScreenSlot(ScreenSlot? slot)
    {
        if (slot is null)
        {
            return;
        }

        this.ScreensCanvas.Children.Remove(slot.BezelImage);
        if (slot.PlaceholderRectangle is not null)
        {
            this.ScreensCanvas.Children.Remove(slot.PlaceholderRectangle);
        }

        this.ScreensCanvas.Children.Remove(slot.ContentImage);
    }

    /// <summary>
    /// Creates the visual elements for a single screen - a bezel (rendered the same way as the
    /// host's own outer border, see <see cref="DrawingHelper.RenderBorder"/>), a placeholder
    /// fill shown until the real screenshot arrives, and a content <see cref="Image"/> that
    /// starts either blank or showing this position's last real screenshot, blurred (see
    /// <see cref="GetHistoricalPlaceholder"/>), until it's populated for real via
    /// <see cref="SetScreenshot"/>. The bezel and placeholder/content don't overlap - a screen's
    /// border ring and its content area are disjoint (see <see cref="Models.Drawing.BoxBounds"/>)
    /// - so paint order between them doesn't matter.
    /// </summary>
    private ScreenSlot CreateScreenSlot(ScreenLayout screenLayout, double scale, int index)
    {
        var screenBounds = screenLayout.ScreenBounds;

        Image bezelImage;
        using (var bezelBitmap = DrawingHelper.RenderBorder(
            screenBounds.MoveTo(new PointInfo(0, 0)), screenLayout.ScreenStyle))
        {
            bezelImage = new Image
            {
                Source = PreviewPane.ToBitmapImage(bezelBitmap),
                Stretch = Stretch.Fill,
            };
            PreviewPane.PositionElement(bezelImage, screenBounds.OuterBounds, scale);
            this.ScreensCanvas.Children.Add(bezelImage);
        }

        Microsoft.UI.Xaml.Shapes.Rectangle? placeholder = null;
        var placeholderColor = screenLayout.ScreenStyle.BackgroundStyle.Color1;
        if (placeholderColor is not null)
        {
            placeholder = new Microsoft.UI.Xaml.Shapes.Rectangle
            {
                Fill = new SolidColorBrush(PreviewPane.ToWindowsColor(placeholderColor.Value)),
            };
            PreviewPane.PositionElement(placeholder, screenBounds.PaddingBounds, scale);
            this.ScreensCanvas.Children.Add(placeholder);
        }

        var contentImage = new Image
        {
            Source = this.GetHistoricalPlaceholder(index),
            Stretch = Stretch.Fill,
        };
        PreviewPane.PositionElement(contentImage, screenBounds.ContentBounds, scale);
        this.ScreensCanvas.Children.Add(contentImage);

        return new ScreenSlot(screenLayout, bezelImage, placeholder, contentImage);
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

    /// <summary>
    /// Copies <paramref name="bitmap"/>'s pixels directly into a <see cref="WriteableBitmap"/>,
    /// rather than round-tripping through a PNG encode (GDI+) + decode (WinUI) - every screen
    /// slot creates two of these (bezel and content) per activation, so the encode/decode cost
    /// that was previously paid once or twice for a whole composited canvas was instead being
    /// paid per screen, which added up. This works as a straight byte copy because every
    /// bitmap this control receives is already <see cref="PixelFormat.Format32bppPArgb"/>
    /// (see <see cref="DrawingHelper.RenderBorder"/>/<c>IScreenshotCaptureProvider</c>
    /// implementations), which is byte-for-byte the same layout as
    /// <see cref="WriteableBitmap"/>'s own pixel buffer (BGRA8, premultiplied).
    /// </summary>
    private static WriteableBitmap ToBitmapImage(Bitmap bitmap)
    {
        var writeableBitmap = new WriteableBitmap(bitmap.Width, bitmap.Height);

        var bounds = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
        var bitmapData = bitmap.LockBits(bounds, ImageLockMode.ReadOnly, PixelFormat.Format32bppPArgb);
        try
        {
            var byteCount = bitmapData.Stride * bitmapData.Height;
            var buffer = new byte[byteCount];
            Marshal.Copy(bitmapData.Scan0, buffer, 0, byteCount);

            using var pixelStream = writeableBitmap.PixelBuffer.AsStream();
            pixelStream.Write(buffer, 0, byteCount);
        }
        finally
        {
            bitmap.UnlockBits(bitmapData);
        }

        writeableBitmap.Invalidate();
        return writeableBitmap;
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
        if (pointerPoint.Properties.IsRightButtonPressed)
        {
            this.Cancel?.Invoke(this, EventArgs.Empty);
            return;
        }

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

        // work out which screenshot was clicked, keeping the owning device too
        var clickedEntry = layout.CanvasLayout.DeviceLayouts
            .SelectMany(deviceLayout => deviceLayout.ScreenLayouts.Select(
                screenLayout => (DeviceLayout: deviceLayout, ScreenLayout: screenLayout)))
            .SingleOrDefault(
                entry => entry.ScreenLayout.ScreenBounds.OuterBounds.Contains(pointerLocation));
        if (clickedEntry.ScreenLayout is null)
        {
            return;
        }

        // scale up the click onto the physical screen - the aspect ratio of the screenshot
        // might be distorted compared to the physical screen due to the borders around the
        // screenshot, so we need to work out the target location on the physical screen first
        var clickedDisplayArea = clickedEntry.ScreenLayout.ScreenInfo.DisplayArea;
        var clickedLocation = pointerLocation
            .Stretch(
                source: clickedEntry.ScreenLayout.ScreenBounds.ContentBounds,
                target: clickedDisplayArea)
            .Clamp(
                new(
                    x: clickedDisplayArea.X + 1,
                    y: clickedDisplayArea.Y + 1,
                    width: clickedDisplayArea.Width - 1,
                    height: clickedDisplayArea.Height - 1))
            .Truncate();

        this.NavigateTo?.Invoke(
            this, new NavigateToEventArgs(clickedEntry.DeviceLayout.DeviceInfo, clickedLocation));
    }

    /// <summary>
    /// Maps 1-9/numpad/P/Left/Right/Home/End to <see cref="NavigateTo"/> and Escape to
    /// <see cref="Cancel"/>, entirely from <see cref="Layout"/> and <see cref="ActiveScreen"/> -
    /// no live desktop/cursor state needed. Screen numbering and Home/End use the flattened
    /// device/screen order (<see cref="GetOrderedScreens"/>), which matches
    /// <c>ScreenHelper.GetAllScreens()</c>'s order since <c>LayoutHelper.GetPreviewLayout</c>
    /// builds <see cref="Layout"/> from that same list via order-preserving projections.
    /// </summary>
    private void PreviewPane_PreviewKeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == VirtualKey.Escape)
        {
            this.Cancel?.Invoke(this, EventArgs.Empty);
            return;
        }

        var layout = this.Layout;
        var activeScreen = this.ActiveScreen;
        if (layout is null || activeScreen is null)
        {
            return;
        }

        var orderedScreens = this.GetOrderedScreens(layout);
        if (orderedScreens.Count == 0)
        {
            return;
        }

        var currentIndex = orderedScreens.FindIndex(
            entry => entry.ScreenLayout.ScreenInfo.Equals(activeScreen));

        var target = e.Key switch
        {
            >= VirtualKey.Number1 and <= VirtualKey.Number9 =>
                PreviewPane.ElementAtOrDefault(orderedScreens, (e.Key - VirtualKey.Number0) - 1),
            >= VirtualKey.NumberPad1 and <= VirtualKey.NumberPad9 =>
                PreviewPane.ElementAtOrDefault(orderedScreens, (e.Key - VirtualKey.NumberPad0) - 1),
            VirtualKey.P =>
                orderedScreens.SingleOrDefault(entry => entry.ScreenLayout.ScreenInfo.Primary),
            VirtualKey.Left when currentIndex >= 0 =>
                orderedScreens[(currentIndex - 1 + orderedScreens.Count) % orderedScreens.Count],
            VirtualKey.Right when currentIndex >= 0 =>
                orderedScreens[(currentIndex + 1) % orderedScreens.Count],
            VirtualKey.Home => orderedScreens[0],
            VirtualKey.End => orderedScreens[^1],
            _ => default,
        };

        if (target.ScreenLayout is null)
        {
            return;
        }

        this.NavigateTo?.Invoke(
            this,
            new NavigateToEventArgs(
                target.DeviceLayout.DeviceInfo, target.ScreenLayout.ScreenInfo.DisplayArea.Midpoint));
    }

    private List<(DeviceLayout DeviceLayout, ScreenLayout ScreenLayout)> GetOrderedScreens(PreviewLayout layout)
        => layout.CanvasLayout.DeviceLayouts
            .SelectMany(deviceLayout => deviceLayout.ScreenLayouts.Select(
                screenLayout => (DeviceLayout: deviceLayout, ScreenLayout: screenLayout)))
            .ToList();

    private static (DeviceLayout DeviceLayout, ScreenLayout ScreenLayout) ElementAtOrDefault(
        List<(DeviceLayout DeviceLayout, ScreenLayout ScreenLayout)> screens, int index)
        => (index >= 0 && index < screens.Count) ? screens[index] : default;

    private sealed class ScreenSlot
    {
        public ScreenSlot(
            ScreenLayout screenLayout, Image bezelImage, Microsoft.UI.Xaml.Shapes.Rectangle? placeholderRectangle, Image contentImage)
        {
            this.ScreenLayout = screenLayout;
            this.BezelImage = bezelImage;
            this.PlaceholderRectangle = placeholderRectangle;
            this.ContentImage = contentImage;
        }

        public ScreenLayout ScreenLayout
        {
            get;
        }

        public Image BezelImage
        {
            get;
        }

        public Microsoft.UI.Xaml.Shapes.Rectangle? PlaceholderRectangle
        {
            get;
        }

        public Image ContentImage
        {
            get;
        }
    }

    /// <summary>
    /// See the <see cref="screenshotHistory"/> field remarks.
    /// </summary>
    private sealed class ScreenshotHistoryEntry
    {
        public ScreenshotHistoryEntry(ImageSource blurredImage, DateTime capturedAt)
        {
            this.BlurredImage = blurredImage;
            this.CapturedAt = capturedAt;
        }

        public ImageSource BlurredImage
        {
            get;
        }

        public DateTime CapturedAt
        {
            get;
        }
    }
}
