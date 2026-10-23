using FancyMouse.Models.Editor;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;

namespace FancyMouse.WinUI3.Controls.Editor;

/// <remarks>
/// Pointer handling is just plumbing - each handler captures the pointer, forwards its position
/// to <see cref="PreviewDesigner"/> (which decides what the gesture means), and redraws.
/// </remarks>
public sealed partial class ScreenLayoutEditor
{
    /// <summary>
    /// The pointer driving the gesture in progress, if any - input from any other pointer is ignored.
    /// </summary>
    private uint? activePointerId;

    private void Screen_PointerPressed(object sender, PointerRoutedEventArgs e)
    {
        // overlapping screens are fine - the pointer is delivered to the topmost screen under
        // it, so this is always the one that's visible where the user clicked
        var visual = (ScreenVisual)((FrameworkElement)sender).Tag;

        // a right-click selects the screen without starting a drag, so its context menu (shown
        // on release, see CreateScreenContextMenu) acts on the screen that was clicked
        if (e.GetCurrentPoint(this.RootGrid).Properties.IsRightButtonPressed)
        {
            e.Handled = true;
            if ((this.designer is not null) && (this.activePointerId is null))
            {
                this.designer.Select(visual.Screen);
                this.Render();
            }

            return;
        }

        this.BeginGesture(sender, e, (designer, x, y) => designer.PressScreen(visual.Screen, x, y));
    }

    private void ResizeHandle_PointerPressed(object sender, PointerRoutedEventArgs e)
    {
        var corner = (ResizeCorner)((FrameworkElement)sender).Tag;
        this.BeginGesture(sender, e, (designer, x, y) => designer.PressResizeHandle(corner, x, y));
    }

    private void RootGrid_PointerPressed(object sender, PointerRoutedEventArgs e)
    {
        // only reached when the press wasn't on a screen or a handle (both mark it handled)
        this.BeginGesture(sender, e, (designer, x, y) => designer.PressBackground(x, y));
    }

    private void BeginGesture(object sender, PointerRoutedEventArgs e, Action<PreviewDesigner, double, double> press)
    {
        e.Handled = true;
        var point = e.GetCurrentPoint(this.RootGrid);
        if ((this.designer is null) || (this.activePointerId is not null)
            || !point.Properties.IsLeftButtonPressed || !((UIElement)sender).CapturePointer(e.Pointer))
        {
            return;
        }

        this.activePointerId = e.Pointer.PointerId;
        press(this.designer, point.Position.X, point.Position.Y);
        this.Render();
    }

    private void Element_PointerMoved(object sender, PointerRoutedEventArgs e)
    {
        if ((this.designer is null) || (this.activePointerId != e.Pointer.PointerId))
        {
            return;
        }

        var position = e.GetCurrentPoint(this.RootGrid).Position;
        this.designer.MovePointer(position.X, position.Y);
        this.Render();
        e.Handled = true;
    }

    private void Element_PointerReleased(object sender, PointerRoutedEventArgs e)
    {
        if ((this.designer is null) || (this.activePointerId != e.Pointer.PointerId))
        {
            return;
        }

        // finish the gesture before releasing the capture, so the capture-lost handler that
        // follows doesn't treat it as abandoned
        this.activePointerId = null;
        this.designer.ReleasePointer();
        ((UIElement)sender).ReleasePointerCapture(e.Pointer);
        this.Render();
        e.Handled = true;
    }

    private void Element_PointerCaptureLost(object sender, PointerRoutedEventArgs e)
    {
        // an interrupted gesture (e.g. Alt+Tab) - a normal release has already cleared activePointerId
        if ((this.designer is null) || (this.activePointerId != e.Pointer.PointerId))
        {
            return;
        }

        this.activePointerId = null;
        this.designer.CancelGesture();
        this.Render();
    }

    private void RootGrid_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
    {
        // wheel events bubble up from the screens too, so this zooms wherever the pointer is
        var point = e.GetCurrentPoint(this.RootGrid);
        if ((this.designer is null) || point.Properties.IsHorizontalMouseWheel)
        {
            return;
        }

        this.designer.TurnWheel(point.Position.X, point.Position.Y, point.Properties.MouseWheelDelta / 120.0);
        this.Render();
        e.Handled = true;
    }
}
