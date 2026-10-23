using FancyMouse.Drawing.Helpers;
using FancyMouse.Models.Drawing;
using FancyMouse.Models.Editor;
using FancyMouse.Models.Styles;
using FancyMouse.WinUI3.Controls.Helpers;

using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;

using Windows.Foundation;

using Color = System.Drawing.Color;
using Image = Microsoft.UI.Xaml.Controls.Image;
using Rectangle = Microsoft.UI.Xaml.Shapes.Rectangle;

namespace FancyMouse.WinUI3.Controls.Editor;

public sealed partial class ScreenLayoutEditor
{
    /// <summary>
    /// The largest bitmap, in pixels along its longest side, a border is rendered at - a preview
    /// can be up to 9999 x 9999, and a full-size bitmap of that would be ~400MB. Larger borders
    /// are rendered proportionally smaller and stretched, which looks the same on screen.
    /// </summary>
    private const double MaximumRenderSize = 4096;

    private const double ResizeHandleSize = 12;

    /// <summary>
    /// The sizes, in preview pixels, <see cref="FrameBorderImage"/> and <see cref="FrameBackgroundImage"/>
    /// were last rendered at.
    /// </summary>
    private (int Width, int Height) renderedFrameBorderSize;

    private (int Width, int Height) renderedFrameBackgroundSize;

    /// <summary>
    /// Redraws everything from the designer's current state - called after every input and command.
    /// </summary>
    private void Render()
    {
        if (this.designer is null)
        {
            return;
        }

        var camera = this.designer.Camera;
        var design = this.designer.Design;

        var frame = design.FrameBounds;
        ScreenLayoutEditor.SetBounds(this.VirtualCanvasRect, camera.ToView(camera.CanvasBounds));

        // the background gradient fills the padding box, as the real preview draws it - with a
        // solid underlay reaching half-way under the border ring, since if the two just met their
        // anti-aliased edges would leave a hairline seam that the window backdrop shows through
        // (half-way keeps the underlay clear of the ring's rounded outer corners)
        var padding = frame.PaddingBounds;
        var border = design.PreviewStyle.CanvasStyle.BorderStyle;
        ScreenLayoutEditor.SetBounds(this.FrameUnderlayRect, camera.ToView(
            (double)(padding.X - (border.Left / 2)),
            (double)(padding.Y - (border.Top / 2)),
            (double)(padding.Width + (border.Horizontal / 2)),
            (double)(padding.Height + (border.Vertical / 2))));
        ScreenLayoutEditor.SetBounds(this.FrameBackgroundImage, camera.ToView(padding));
        ScreenLayoutEditor.SetBounds(this.FrameBorderImage, camera.ToView(frame.BorderBounds));
        ScreenLayoutEditor.SetBounds(this.PreviewSizeRect, camera.ToView(design.PreviewBounds.OuterBounds));
        this.RenderFrame(frame, design.PreviewStyle.CanvasStyle);

        for (var index = 0; index < design.Screens.Count; index++)
        {
            var visual = this.visuals[design.Screens[index]];
            Canvas.SetZIndex(visual.Container, index);
            this.PositionScreenVisual(visual, camera, design.ScreenStyle);
        }

        this.UpdateAdorner(camera, design.SelectedScreen);
    }

    private static void SetBounds(FrameworkElement element, ViewRect bounds)
    {
        Canvas.SetLeft(element, bounds.X);
        Canvas.SetTop(element, bounds.Y);
        element.Width = bounds.Width;
        element.Height = bounds.Height;
    }

    /// <summary>
    /// Gets insets in preview pixels as a <see cref="Thickness"/> in the editor's DIPs.
    /// </summary>
    private static Thickness ToViewThickness(EditorCamera camera, decimal left, decimal top, decimal right, decimal bottom)
        => new(
            (double)left * camera.Zoom,
            (double)top * camera.Zoom,
            (double)right * camera.Zoom,
            (double)bottom * camera.Zoom);

    /// <summary>
    /// Gets ready to draw a newly loaded <paramref name="previewStyle"/> - forgets the frame's
    /// rendered bitmaps (a new style can need different ones at the same size), and sets the
    /// underlay to the style's background colour.
    /// </summary>
    private void ApplyPreviewStyle(PreviewStyle previewStyle)
    {
        this.renderedFrameBorderSize = default;
        this.renderedFrameBackgroundSize = default;
        var background = previewStyle.CanvasStyle.BackgroundStyle;
        this.FrameUnderlayRect.Fill = ((background.Color1 ?? background.Color2) is { } color)
            ? new SolidColorBrush(ScreenLayoutEditor.ToWindowsColor(color))
            : null;
    }

    /// <summary>
    /// Renders the preview window's border ring and background at their real sizes in preview
    /// pixels, in the style's own colours - only when a size has changed, since the frame follows
    /// the screens around as they're edited.
    /// </summary>
    private void RenderFrame(BoxBounds frame, BoxStyle canvasStyle)
    {
        var borderSize = ScreenLayoutEditor.ToPixelSize(frame.BorderBounds.Size);
        if (borderSize != this.renderedFrameBorderSize)
        {
            this.renderedFrameBorderSize = borderSize;
            this.FrameBorderImage.Source = ScreenLayoutEditor.RenderBorderImage(
                borderSize.Width, borderSize.Height, canvasStyle.BorderStyle);
        }

        var backgroundSize = ScreenLayoutEditor.ToPixelSize(frame.PaddingBounds.Size);
        if (backgroundSize != this.renderedFrameBackgroundSize)
        {
            this.renderedFrameBackgroundSize = backgroundSize;
            this.FrameBackgroundImage.Source = ScreenLayoutEditor.RenderBackgroundImage(
                backgroundSize.Width, backgroundSize.Height, canvasStyle.BackgroundStyle);
        }
    }

    private static (int Width, int Height) ToPixelSize(SizeInfo size)
        => ((int)Math.Round(size.Width), (int)Math.Round(size.Height));

    private static Windows.UI.Color ToWindowsColor(Color color)
        => Windows.UI.Color.FromArgb(color.A, color.R, color.G, color.B);

    /// <summary>
    /// Gets black or white, whichever reads better on <paramref name="background"/>.
    /// </summary>
    private static SolidColorBrush GetContrastingTextBrush(Color background)
    {
        // perceived brightness - near enough for choosing between black and white
        var brightness = ((0.299 * background.R) + (0.587 * background.G) + (0.114 * background.B)) / 255;
        return new SolidColorBrush((brightness > 0.5) ? Microsoft.UI.Colors.Black : Microsoft.UI.Colors.White);
    }

    private ScreenVisual CreateScreenVisual(EditableScreen screen, BoxStyle screenStyle)
    {
        var container = new CursorGrid(InputSystemCursorShape.SizeAll)
        {
            // a fully transparent background still makes the whole screen hit-testable,
            // including its margin and the bezel ring's transparent corners
            Background = new SolidColorBrush(Microsoft.UI.Colors.Transparent),
            ContextFlyout = this.screenContextMenu,
        };

        var bezelImage = new Image { Stretch = Stretch.Fill, IsHitTestVisible = false };

        // the same solid placeholder the real preview shows until a screenshot arrives - the
        // style's first background colour - or a neutral grey if the style doesn't set one
        var placeholderColor = screenStyle.BackgroundStyle.Color1;
        var fill = new Rectangle
        {
            Fill = (placeholderColor is { } color)
                ? new SolidColorBrush(ScreenLayoutEditor.ToWindowsColor(color))
                : (Brush)this.Resources["ScreenFillBrush"],
            IsHitTestVisible = false,
        };

        var culture = System.Globalization.CultureInfo.CurrentCulture;
        var number = new TextBlock
        {
            Text = screen.Number.ToString(culture),
            Style = (Style)this.Resources["ScreenNumberStyle"],
        };
        var caption = new TextBlock
        {
            Text = string.Create(culture, $"{screen.NativeWidth} × {screen.NativeHeight}"),
            Style = (Style)this.Resources["ScreenCaptionStyle"],
        };
        if (placeholderColor is { } textBackground)
        {
            number.Foreground = ScreenLayoutEditor.GetContrastingTextBrush(textBackground);
            caption.Foreground = number.Foreground;
        }

        var labels = new StackPanel
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            IsHitTestVisible = false,
            Children = { number, caption },
        };

        container.Children.Add(bezelImage);
        container.Children.Add(fill);
        container.Children.Add(labels);

        container.PointerPressed += this.Screen_PointerPressed;
        container.PointerMoved += this.Element_PointerMoved;
        container.PointerReleased += this.Element_PointerReleased;
        container.PointerCaptureLost += this.Element_PointerCaptureLost;

        var visual = new ScreenVisual(screen, container, bezelImage, fill);
        container.Tag = visual;
        return visual;
    }

    /// <summary>
    /// Positions <paramref name="visual"/> and lays out its box model the way the real preview
    /// does - the margin is left empty, the bezel fills the border box, and the placeholder fill
    /// covers the padding box - all at their real size in preview pixels, scaled by the camera's zoom.
    /// </summary>
    private void PositionScreenVisual(ScreenVisual visual, EditorCamera camera, BoxStyle screenStyle)
    {
        var margin = screenStyle.MarginStyle;
        var border = screenStyle.BorderStyle;

        ScreenLayoutEditor.SetBounds(visual.Container, camera.ToView(visual.Screen));
        visual.BezelImage.Margin = ScreenLayoutEditor.ToViewThickness(camera, margin.Left, margin.Top, margin.Right, margin.Bottom);
        visual.Fill.Margin = ScreenLayoutEditor.ToViewThickness(
            camera,
            margin.Left + border.Left,
            margin.Top + border.Top,
            margin.Right + border.Right,
            margin.Bottom + border.Bottom);
        ScreenLayoutEditor.RenderBezel(visual, margin, border);
    }

    /// <summary>
    /// Renders <paramref name="visual"/>'s bezel ring at the size it will actually be drawn in the
    /// preview - so its thickness is exactly what the user will see, however far they've zoomed -
    /// only when that size has changed.
    /// </summary>
    private static void RenderBezel(ScreenVisual visual, MarginStyle margin, BorderStyle border)
    {
        var width = (int)Math.Round(visual.Screen.Width - (double)margin.Horizontal);
        var height = (int)Math.Round(visual.Screen.Height - (double)margin.Vertical);
        if ((width, height) == visual.RenderedBezelSize)
        {
            return;
        }

        visual.RenderedBezelSize = (width, height);
        visual.BezelImage.Source = ScreenLayoutEditor.RenderBorderImage(width, height, border);
    }

    /// <summary>
    /// Renders a border ring for a box <paramref name="width"/> x <paramref name="height"/> preview
    /// pixels in size with the shared drawing library, or returns null if there's nothing to draw.
    /// </summary>
    private static WriteableBitmap? RenderBorderImage(double width, double height, BorderStyle borderStyle)
    {
        // a box shrunk smaller than its own border has no room left to draw one
        if ((borderStyle.Horizontal < 1) || (borderStyle.Vertical < 1)
            || (width < 2 * (double)borderStyle.Horizontal) || (height < 2 * (double)borderStyle.Vertical))
        {
            return null;
        }

        var scale = (decimal)Math.Min(1, ScreenLayoutEditor.MaximumRenderSize / Math.Max(width, height));
        var scaledStyle = new BorderStyle(
            borderStyle.Color,
            borderStyle.Left * scale,
            borderStyle.Top * scale,
            borderStyle.Right * scale,
            borderStyle.Bottom * scale,
            borderStyle.Depth * scale);
        var size = new SizeInfo(Math.Round((decimal)width * scale), Math.Round((decimal)height * scale));
        using var bitmap = DrawingHelper.RenderBorder(size, scaledStyle);
        return MediaHelper.ToBitmapImage(bitmap);
    }

    /// <summary>
    /// Renders the background fill for a box <paramref name="width"/> x <paramref name="height"/>
    /// preview pixels in size with the shared drawing library, or returns null if it's empty.
    /// </summary>
    private static WriteableBitmap? RenderBackgroundImage(double width, double height, BackgroundStyle backgroundStyle)
    {
        if ((width < 1) || (height < 1))
        {
            return null;
        }

        // a gradient scaled down and stretched back up looks the same, so cap it like the borders
        var scale = Math.Min(1, ScreenLayoutEditor.MaximumRenderSize / Math.Max(width, height));
        var size = new SizeInfo(Math.Max(1, Math.Round((decimal)(width * scale))), Math.Max(1, Math.Round((decimal)(height * scale))));
        using var bitmap = DrawingHelper.RenderBackground(size, backgroundStyle);
        return MediaHelper.ToBitmapImage(bitmap);
    }

    /// <summary>
    /// Positions the selection outline and corner resize handles around the selected screen, or
    /// hides them if nothing is selected.
    /// </summary>
    private void UpdateAdorner(EditorCamera camera, EditableScreen? selected)
    {
        var visibility = (selected is null) ? Visibility.Collapsed : Visibility.Visible;
        this.SelectionOutline.Visibility = visibility;
        foreach (var handle in this.resizeHandles)
        {
            handle.Visibility = visibility;
        }

        if (selected is null)
        {
            return;
        }

        var bounds = camera.ToView(selected);
        Canvas.SetLeft(this.SelectionOutline, bounds.X - 2);
        Canvas.SetTop(this.SelectionOutline, bounds.Y - 2);
        this.SelectionOutline.Width = bounds.Width + 4;
        this.SelectionOutline.Height = bounds.Height + 4;

        foreach (var handle in this.resizeHandles)
        {
            var (x, y) = (ResizeCorner)handle.Tag switch
            {
                ResizeCorner.TopLeft => (bounds.X, bounds.Y),
                ResizeCorner.TopRight => (bounds.Right, bounds.Y),
                ResizeCorner.BottomLeft => (bounds.X, bounds.Bottom),
                _ => (bounds.Right, bounds.Bottom),
            };
            Canvas.SetLeft(handle, x - (ScreenLayoutEditor.ResizeHandleSize / 2));
            Canvas.SetTop(handle, y - (ScreenLayoutEditor.ResizeHandleSize / 2));
        }
    }

    private void RootGrid_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        // canvases don't clip their children, so without this a zoomed-in preview would draw
        // right over the rest of the window
        this.RootGrid.Clip = new RectangleGeometry { Rect = new Rect(0, 0, e.NewSize.Width, e.NewSize.Height) };

        this.designer?.Camera.SetViewportSize(e.NewSize.Width, e.NewSize.Height);
        this.Render();
    }
}
