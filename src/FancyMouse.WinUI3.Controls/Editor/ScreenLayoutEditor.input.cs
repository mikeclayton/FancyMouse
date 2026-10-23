using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;

using Windows.Foundation;

namespace FancyMouse.WinUI3.Controls.Editor;

public sealed partial class ScreenLayoutEditor
{
    /// <summary>
    /// The smallest and largest sizes a screen can be resized to, relative to its native resolution.
    /// </summary>
    private const double MinimumScale = 0.1;

    private const double MaximumScale = 10;

    /// <summary>
    /// The drag in progress, if any - started by pressing on a screen (move) or one of the
    /// selected screen's corner handles (resize), and ended when the pointer is released.
    /// </summary>
    private DragState? drag;

    private void Screen_PointerPressed(object sender, PointerRoutedEventArgs e)
    {
        var element = (FrameworkElement)sender;
        var visual = (ScreenVisual)element.Tag;

        // overlapping screens are fine - the pointer is delivered to the topmost screen under
        // it, so this is always the one that's visible where the user clicked
        this.Select(visual);
        e.Handled = true;

        if (!e.GetCurrentPoint(this.ScreensCanvas).Properties.IsLeftButtonPressed || !element.CapturePointer(e.Pointer))
        {
            return;
        }

        this.drag = new DragState(
            PointerId: e.Pointer.PointerId,
            Start: e.GetCurrentPoint(this.ScreensCanvas).Position,
            StartX: visual.Screen.X,
            StartY: visual.Screen.Y,
            Corner: null,
            AnchorX: 0,
            AnchorY: 0);
    }

    private void ResizeHandle_PointerPressed(object sender, PointerRoutedEventArgs e)
    {
        var element = (FrameworkElement)sender;
        e.Handled = true;

        if ((this.selected is null) || !e.GetCurrentPoint(this.ScreensCanvas).Properties.IsLeftButtonPressed || !element.CapturePointer(e.Pointer))
        {
            return;
        }

        // the corner diagonally opposite the dragged one stays where it is - its position (in
        // desktop pixels) is the anchor the new size is measured from
        var screen = this.selected.Screen;
        var corner = (ResizeCorner)element.Tag;
        var anchorX = corner is ResizeCorner.TopLeft or ResizeCorner.BottomLeft ? screen.X + screen.Width : screen.X;
        var anchorY = corner is ResizeCorner.TopLeft or ResizeCorner.TopRight ? screen.Y + screen.Height : screen.Y;

        this.drag = new DragState(
            PointerId: e.Pointer.PointerId,
            Start: e.GetCurrentPoint(this.ScreensCanvas).Position,
            StartX: screen.X,
            StartY: screen.Y,
            Corner: corner,
            AnchorX: anchorX,
            AnchorY: anchorY);
    }

    private void Element_PointerMoved(object sender, PointerRoutedEventArgs e)
    {
        if ((this.drag is not { } drag) || (drag.PointerId != e.Pointer.PointerId) || (this.selected is null))
        {
            return;
        }

        var position = e.GetCurrentPoint(this.ScreensCanvas).Position;
        var screen = this.selected.Screen;

        if (drag.Corner is { } corner)
        {
            ScreenLayoutEditor.Resize(screen, corner, drag.AnchorX, drag.AnchorY, this.ToDesktop(position));
        }
        else
        {
            screen.X = drag.StartX + ((position.X - drag.Start.X) / this.view.Scale);
            screen.Y = drag.StartY + ((position.Y - drag.Start.Y) / this.view.Scale);
        }

        this.PositionScreenVisual(this.selected);
        this.UpdateAdorner();
        e.Handled = true;
    }

    private void Element_PointerReleased(object sender, PointerRoutedEventArgs e)
    {
        if (this.drag?.PointerId == e.Pointer.PointerId)
        {
            ((UIElement)sender).ReleasePointerCapture(e.Pointer);
            e.Handled = true;
        }
    }

    private void Element_PointerCaptureLost(object sender, PointerRoutedEventArgs e)
    {
        // raised for a normal release as well as an interrupted drag (e.g. Alt+Tab), so this is
        // the one place a drag finishes - re-fit everything once the screen has been let go
        if (this.drag?.PointerId == e.Pointer.PointerId)
        {
            this.drag = null;
            this.FitToView();
        }
    }

    private void RootGrid_PointerPressed(object sender, PointerRoutedEventArgs e)
    {
        // only reached when the press wasn't on a screen or a handle (both mark it handled)
        this.Select(null);
    }

    /// <summary>
    /// Converts a point in the editor's DIPs to desktop pixels.
    /// </summary>
    private (double X, double Y) ToDesktop(Point position)
        => ((position.X - this.view.OffsetX) / this.view.Scale, (position.Y - this.view.OffsetY) / this.view.Scale);

    /// <summary>
    /// Resizes <paramref name="screen"/> uniformly so the dragged <paramref name="corner"/> follows
    /// <paramref name="pointer"/> while the opposite corner stays at the anchor - using whichever
    /// axis the pointer has moved furthest along, so the corner tracks the pointer as closely as
    /// a locked aspect ratio allows.
    /// </summary>
    private static void Resize(EditableScreen screen, ResizeCorner corner, double anchorX, double anchorY, (double X, double Y) pointer)
    {
        var scale = Math.Max(
            Math.Abs(pointer.X - anchorX) / screen.NativeWidth,
            Math.Abs(pointer.Y - anchorY) / screen.NativeHeight);
        screen.Scale = Math.Clamp(scale, ScreenLayoutEditor.MinimumScale, ScreenLayoutEditor.MaximumScale);

        screen.X = corner is ResizeCorner.TopLeft or ResizeCorner.BottomLeft ? anchorX - screen.Width : anchorX;
        screen.Y = corner is ResizeCorner.TopLeft or ResizeCorner.TopRight ? anchorY - screen.Height : anchorY;
    }

    /// <summary>
    /// A drag in progress - <see cref="Corner"/> is null for a move, or the corner being dragged
    /// for a resize, in which case <see cref="AnchorX"/>/<see cref="AnchorY"/> are the opposite
    /// corner's fixed position in desktop pixels.
    /// </summary>
    private sealed record DragState(
        uint PointerId,
        Point Start,
        double StartX,
        double StartY,
        ResizeCorner? Corner,
        double AnchorX,
        double AnchorY);
}
