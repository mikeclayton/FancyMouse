using FancyMouse.Drawing.Helpers;
using FancyMouse.Models.Drawing;
using FancyMouse.Models.Styles;
using FancyMouse.WinUI3.Controls.Helpers;

using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

using Color = System.Drawing.Color;
using Image = Microsoft.UI.Xaml.Controls.Image;
using Rectangle = Microsoft.UI.Xaml.Shapes.Rectangle;

namespace FancyMouse.WinUI3.Controls.Editor;

public sealed partial class ScreenLayoutEditor
{
    /// <summary>
    /// Space, in DIPs, kept clear around the screens when fitting them into the editor.
    /// </summary>
    private const double ViewPadding = 24;

    /// <summary>
    /// The bezel's thickness and 3-D depth, in DIPs - constant on screen whatever the screen's
    /// size, the same way Display Settings draws its monitor outlines.
    /// </summary>
    private const double BezelThickness = 6;

    private const double BezelDepth = 2;

    private const double ResizeHandleSize = 12;

    private static readonly Color BezelColor = Color.FromArgb(0xFF, 0x20, 0x20, 0x20);

    /// <summary>
    /// Gets the mapping from desktop pixels to the editor's DIPs - a uniform scale plus an
    /// offset, recalculated by <see cref="FitToView"/>.
    /// </summary>
    private (double Scale, double OffsetX, double OffsetY) view = (1, 0, 0);

    /// <summary>
    /// The <see cref="XamlRoot"/> whose <see cref="XamlRoot.Changed"/> event the editor is
    /// listening to for DPI changes, if any - see <see cref="ScreenLayoutEditor_Loaded"/>.
    /// </summary>
    private XamlRoot? subscribedXamlRoot;

    /// <summary>
    /// Scales and centres the screens so they all fit in the editor - recalculated after every
    /// edit (and when the editor resizes), the same way Display Settings re-fits its monitors
    /// once you let go of one. Not called *during* a drag, so the screen being dragged stays
    /// under the pointer.
    /// </summary>
    public void FitToView()
    {
        if ((this.zOrder.Count == 0) || (this.ActualWidth <= 0) || (this.ActualHeight <= 0))
        {
            return;
        }

        var screens = this.zOrder.Select(visual => visual.Screen).ToList();
        var left = screens.Min(screen => screen.X);
        var top = screens.Min(screen => screen.Y);
        var right = screens.Max(screen => screen.X + screen.Width);
        var bottom = screens.Max(screen => screen.Y + screen.Height);

        var availableWidth = Math.Max(1, this.ActualWidth - (2 * ScreenLayoutEditor.ViewPadding));
        var availableHeight = Math.Max(1, this.ActualHeight - (2 * ScreenLayoutEditor.ViewPadding));
        var scale = Math.Min(availableWidth / (right - left), availableHeight / (bottom - top));

        this.view = (
            scale,
            ScreenLayoutEditor.ViewPadding + ((availableWidth - ((right - left) * scale)) / 2) - (left * scale),
            ScreenLayoutEditor.ViewPadding + ((availableHeight - ((bottom - top) * scale)) / 2) - (top * scale));

        foreach (var visual in this.zOrder)
        {
            this.PositionScreenVisual(visual);
        }

        this.UpdateAdorner();
    }

    /// <summary>
    /// Gets <paramref name="screen"/>'s bounds in the editor's DIPs.
    /// </summary>
    private RectangleInfo ToView(EditableScreen screen)
        => new(
            (decimal)(this.view.OffsetX + (screen.X * this.view.Scale)),
            (decimal)(this.view.OffsetY + (screen.Y * this.view.Scale)),
            (decimal)(screen.Width * this.view.Scale),
            (decimal)(screen.Height * this.view.Scale));

    private ScreenVisual CreateScreenVisual(EditableScreen screen)
    {
        var container = new CursorGrid(InputSystemCursorShape.SizeAll)
        {
            // a fully transparent background still makes the whole screen hit-testable,
            // including the bezel ring's transparent corners
            Background = new SolidColorBrush(Microsoft.UI.Colors.Transparent),
        };

        var bezelImage = new Image { Stretch = Stretch.Fill, IsHitTestVisible = false };
        var fill = new Rectangle { Margin = new Thickness(ScreenLayoutEditor.BezelThickness) };
        var culture = System.Globalization.CultureInfo.CurrentCulture;
        var number = new TextBlock { Text = screen.Number.ToString(culture) };
        var caption = new TextBlock
        {
            Text = string.Create(culture, $"{screen.NativeWidth} × {screen.NativeHeight}"),
        };
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

        var visual = new ScreenVisual(screen, container, bezelImage, fill, number, caption);
        container.Tag = visual;
        visual.SetSelected(false, this.Resources);
        return visual;
    }

    private void PositionScreenVisual(ScreenVisual visual)
    {
        var bounds = this.ToView(visual.Screen);
        Canvas.SetLeft(visual.Container, (double)bounds.X);
        Canvas.SetTop(visual.Container, (double)bounds.Y);
        visual.Container.Width = (double)bounds.Width;
        visual.Container.Height = (double)bounds.Height;
        this.RenderBezel(visual, (double)bounds.Width, (double)bounds.Height);
    }

    /// <summary>
    /// Renders <paramref name="visual"/>'s rounded bezel ring with the shared drawing library, at
    /// the physical pixel size it's shown at so it stays crisp - only when that size has changed.
    /// </summary>
    private void RenderBezel(ScreenVisual visual, double width, double height)
    {
        var rasterizationScale = this.XamlRoot?.RasterizationScale ?? 1.0;
        var pixelWidth = (int)Math.Round(width * rasterizationScale);
        var pixelHeight = (int)Math.Round(height * rasterizationScale);
        if ((pixelWidth, pixelHeight) == visual.RenderedBezelSize)
        {
            return;
        }

        visual.RenderedBezelSize = (pixelWidth, pixelHeight);

        // a screen shrunk smaller than its own bezel has no room left to draw one
        var thickness = (int)Math.Round(ScreenLayoutEditor.BezelThickness * rasterizationScale);
        if ((pixelWidth < 4 * thickness) || (pixelHeight < 4 * thickness))
        {
            visual.BezelImage.Source = null;
            return;
        }

        var borderStyle = new BorderStyle(
            ScreenLayoutEditor.BezelColor,
            all: thickness,
            depth: (int)Math.Round(ScreenLayoutEditor.BezelDepth * rasterizationScale));
        using var bezelBitmap = DrawingHelper.RenderBorder(new SizeInfo(pixelWidth, pixelHeight), borderStyle);
        visual.BezelImage.Source = MediaHelper.ToBitmapImage(bezelBitmap);
    }

    /// <summary>
    /// Positions the selection outline and corner resize handles around the selected screen, or
    /// hides them if nothing is selected.
    /// </summary>
    private void UpdateAdorner()
    {
        var visibility = (this.selected is null) ? Visibility.Collapsed : Visibility.Visible;
        this.SelectionOutline.Visibility = visibility;
        foreach (var handle in this.resizeHandles)
        {
            handle.Visibility = visibility;
        }

        if (this.selected is null)
        {
            return;
        }

        var bounds = this.ToView(this.selected.Screen);
        var left = (double)bounds.X;
        var top = (double)bounds.Y;
        var right = (double)(bounds.X + bounds.Width);
        var bottom = (double)(bounds.Y + bounds.Height);

        Canvas.SetLeft(this.SelectionOutline, left - 2);
        Canvas.SetTop(this.SelectionOutline, top - 2);
        this.SelectionOutline.Width = (right - left) + 4;
        this.SelectionOutline.Height = (bottom - top) + 4;

        foreach (var handle in this.resizeHandles)
        {
            var (x, y) = (ResizeCorner)handle.Tag switch
            {
                ResizeCorner.TopLeft => (left, top),
                ResizeCorner.TopRight => (right, top),
                ResizeCorner.BottomLeft => (left, bottom),
                _ => (right, bottom),
            };
            Canvas.SetLeft(handle, x - (ScreenLayoutEditor.ResizeHandleSize / 2));
            Canvas.SetTop(handle, y - (ScreenLayoutEditor.ResizeHandleSize / 2));
        }
    }

    private void ScreenLayoutEditor_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (this.drag is null)
        {
            this.FitToView();
        }
    }

    private void ScreenLayoutEditor_Loaded(object sender, RoutedEventArgs e)
    {
        // re-render the bezels at the new physical size if the window moves to a monitor with a
        // different DPI scaling setting
        this.subscribedXamlRoot = this.XamlRoot;
        if (this.subscribedXamlRoot is not null)
        {
            this.subscribedXamlRoot.Changed += this.XamlRoot_Changed;
        }

        this.FitToView();
    }

    private void ScreenLayoutEditor_Unloaded(object sender, RoutedEventArgs e)
    {
        if (this.subscribedXamlRoot is not null)
        {
            this.subscribedXamlRoot.Changed -= this.XamlRoot_Changed;
            this.subscribedXamlRoot = null;
        }
    }

    private void XamlRoot_Changed(XamlRoot sender, XamlRootChangedEventArgs args)
    {
        if (this.drag is null)
        {
            this.FitToView();
        }
    }
}
