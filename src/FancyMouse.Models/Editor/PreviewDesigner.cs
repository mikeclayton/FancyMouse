using FancyMouse.Models.Display;
using FancyMouse.Models.Drawing;
using FancyMouse.Models.Styles;

namespace FancyMouse.Models.Editor;

/// <summary>
/// The screen layout editor's behaviour, independent of any UI framework - a view forwards the
/// pointer input it receives (in its own units), and redraws from <see cref="Camera"/> and
/// <see cref="Design"/> afterwards.
/// </summary>
/// <remarks>
/// Gestures map onto <see cref="PreviewDesign"/>/<see cref="EditorCamera"/> commands: pressing a
/// screen selects it and starts a move, pressing a resize handle starts a resize, pressing the
/// background starts a pan (or, if the pointer doesn't move, is a click that clears the
/// selection), and the wheel zooms about the pointer. The view is responsible for pointer
/// capture, and for only forwarding input from the pointer that started the gesture.
/// </remarks>
public sealed class PreviewDesigner
{
    /// <summary>
    /// The size, in preview pixels, of the virtual canvas the camera can roam over - the largest
    /// preview size the settings allow, with the preview centred in it.
    /// </summary>
    public const double VirtualCanvasSize = 9999;

    /// <summary>
    /// Space, in view units, kept clear around the preview when zooming to fit it.
    /// </summary>
    public const double ViewPadding = 24;

    /// <summary>
    /// How far, in view units, the pointer has to move after pressing on the background before
    /// it counts as a pan rather than a click.
    /// </summary>
    public const double PanThreshold = 4;

    /// <summary>
    /// How much one notch of the mouse wheel zooms by.
    /// </summary>
    public const double WheelZoomFactor = 1.2;

    /// <summary>
    /// How much the zoom in / zoom out commands zoom by - two steps double or halve the zoom.
    /// </summary>
    public static readonly double ZoomStepFactor = Math.Sqrt(2);

    private Gesture? gesture;

    public PreviewDesigner(IEnumerable<ScreenInfo> screens, PreviewStyle previewStyle)
    {
        this.Design = new PreviewDesign(screens, previewStyle);

        var preview = this.Design.PreviewBounds.OuterBounds;
        var canvasBounds = new RectangleInfo(
            (preview.Width - (decimal)PreviewDesigner.VirtualCanvasSize) / 2,
            (preview.Height - (decimal)PreviewDesigner.VirtualCanvasSize) / 2,
            (decimal)PreviewDesigner.VirtualCanvasSize,
            (decimal)PreviewDesigner.VirtualCanvasSize);
        this.Camera = new EditorCamera(canvasBounds, preview, PreviewDesigner.ViewPadding);
    }

    public PreviewDesign Design
    {
        get;
    }

    public EditorCamera Camera
    {
        get;
    }

    /// <summary>
    /// Gets a value indicating whether a gesture (move, resize or pan) is in progress.
    /// </summary>
    public bool IsGestureInProgress => this.gesture is not null;

    /// <summary>
    /// Raised when <see cref="PreviewDesign.SelectedScreen"/> changes.
    /// </summary>
    public event EventHandler? SelectionChanged;

    public void Select(EditableScreen? screen)
    {
        if (this.Design.Select(screen))
        {
            this.SelectionChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Selects <paramref name="screen"/> and starts moving it.
    /// </summary>
    /// <remarks>
    /// Drags are tracked in preview pixels rather than view units, so the screen still follows
    /// the pointer if the user zooms with the wheel mid-drag.
    /// </remarks>
    public void PressScreen(EditableScreen screen, double viewX, double viewY)
    {
        ArgumentNullException.ThrowIfNull(screen);
        this.Select(screen);
        this.gesture = new MoveGesture(screen, this.Camera.ToPreview(viewX, viewY), screen.X, screen.Y);
    }

    /// <summary>
    /// Starts resizing the selected screen by its <paramref name="corner"/> handle.
    /// </summary>
    public void PressResizeHandle(ResizeCorner corner, double viewX, double viewY)
    {
        if (this.Design.SelectedScreen is not { } screen)
        {
            return;
        }

        this.gesture = new ResizeGesture(screen, corner, PreviewDesign.GetResizeAnchor(screen, corner));
    }

    public void PressBackground(double viewX, double viewY)
        => this.gesture = new PanGesture((viewX, viewY), (viewX, viewY), Moved: false);

    public void MovePointer(double viewX, double viewY)
    {
        switch (this.gesture)
        {
            case MoveGesture move:
                var (x, y) = this.Camera.ToPreview(viewX, viewY);
                this.Design.MoveScreen(move.Screen, move.StartX + (x - move.Start.X), move.StartY + (y - move.Start.Y));
                break;
            case ResizeGesture resize:
                var (pointerX, pointerY) = this.Camera.ToPreview(viewX, viewY);
                this.Design.ResizeScreen(resize.Screen, resize.Corner, resize.Anchor.X, resize.Anchor.Y, pointerX, pointerY);
                break;
            case PanGesture pan:
                var moved = pan.Moved
                    || (Math.Abs(viewX - pan.Start.X) > PreviewDesigner.PanThreshold)
                    || (Math.Abs(viewY - pan.Start.Y) > PreviewDesigner.PanThreshold);
                if (moved)
                {
                    this.Camera.PanBy(viewX - pan.Last.X, viewY - pan.Last.Y);
                    this.gesture = pan with { Last = (viewX, viewY), Moved = true };
                }

                break;
        }
    }

    /// <summary>
    /// Finishes the gesture in progress - a press and release on the background without moving
    /// is a click, which clears the selection.
    /// </summary>
    public void ReleasePointer()
    {
        if (this.gesture is PanGesture { Moved: false })
        {
            this.Select(null);
        }

        this.gesture = null;
    }

    /// <summary>
    /// Abandons the gesture in progress (e.g. the view lost pointer capture to Alt+Tab) - whatever
    /// it had done so far is kept, but a background press doesn't count as a click.
    /// </summary>
    public void CancelGesture()
        => this.gesture = null;

    /// <summary>
    /// Zooms about the pointer - positive <paramref name="notches"/> zoom in, negative zoom out.
    /// </summary>
    public void TurnWheel(double viewX, double viewY, double notches)
        => this.Camera.ZoomAt(viewX, viewY, Math.Pow(PreviewDesigner.WheelZoomFactor, notches));

    public void ZoomIn()
        => this.Camera.ZoomTo(this.Camera.Zoom * PreviewDesigner.ZoomStepFactor);

    public void ZoomOut()
        => this.Camera.ZoomTo(this.Camera.Zoom / PreviewDesigner.ZoomStepFactor);

    /// <summary>
    /// Zooms so one preview pixel is one physical pixel - i.e. the size the preview will actually
    /// be on screen - given how many view units a physical pixel is (e.g. 1 / 1.5 for a view in
    /// DIPs on a monitor at 150% scaling).
    /// </summary>
    public void ZoomToActualSize(double viewUnitsPerPixel)
        => this.Camera.ZoomTo(viewUnitsPerPixel);

    private abstract record Gesture;

    private sealed record MoveGesture(EditableScreen Screen, (double X, double Y) Start, double StartX, double StartY) : Gesture;

    private sealed record ResizeGesture(EditableScreen Screen, ResizeCorner Corner, (double X, double Y) Anchor) : Gesture;

    private sealed record PanGesture((double X, double Y) Start, (double X, double Y) Last, bool Moved) : Gesture;
}
