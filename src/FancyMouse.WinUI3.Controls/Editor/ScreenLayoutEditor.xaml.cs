using FancyMouse.Models.Display;

using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace FancyMouse.WinUI3.Controls.Editor;

/// <summary>
/// An editor for where screens are drawn, in the style of Windows' Display Settings - drag a
/// screen to move it, drag one of its corner handles to resize it (aspect ratio locked), and
/// change how overlapping screens stack with the z-order commands.
/// </summary>
/// <remarks>
/// A separate control rather than an edit mode on <see cref="PreviewPane"/> - the two only share
/// "draw a bezel for a rectangle" (<c>DrawingHelper.RenderBorder</c> and <c>MediaHelper</c>,
/// which are already shared), while almost everything else conflicts: PreviewPane draws a
/// LayoutHelper-computed <c>PreviewLayout</c> and treats a click as "jump here", whereas the
/// editor draws free positions with a per-screen scale and z-order, and treats a click as
/// "select and drag". An edit mode would put an "if editing" branch in nearly every method of both.
/// <para/>
/// Edits are in-memory only - <see cref="Screens"/> exposes the current placements for a host
/// that wants to keep them.
/// </remarks>
public sealed partial class ScreenLayoutEditor : UserControl
{
    /// <summary>
    /// The screens' visuals, back to front - a screen's index here is its z-order.
    /// </summary>
    private readonly List<ScreenVisual> zOrder = [];

    private readonly List<CursorGrid> resizeHandles = [];

    private ScreenVisual? selected;

    public ScreenLayoutEditor()
    {
        this.InitializeComponent();
        this.CreateResizeHandles();
        this.RootGrid.PointerPressed += this.RootGrid_PointerPressed;
        this.SizeChanged += this.ScreenLayoutEditor_SizeChanged;
        this.Loaded += this.ScreenLayoutEditor_Loaded;
        this.Unloaded += this.ScreenLayoutEditor_Unloaded;
    }

    /// <summary>
    /// Gets the screens' current placements, back to front.
    /// </summary>
    public IReadOnlyList<EditableScreen> Screens
        => this.zOrder.Select(visual => visual.Screen).ToList();

    /// <summary>
    /// Gets the currently selected screen, if any - the one the z-order and reset commands act on.
    /// </summary>
    public EditableScreen? SelectedScreen
        => this.selected?.Screen;

    /// <summary>
    /// Raised when <see cref="SelectedScreen"/> changes.
    /// </summary>
    public event EventHandler? SelectionChanged;

    /// <summary>
    /// Replaces the editor's contents with <paramref name="screens"/>, each at its Display
    /// Settings position and native size, stacked in the order given (first at the back).
    /// </summary>
    public void LoadScreens(IEnumerable<ScreenInfo> screens)
    {
        ArgumentNullException.ThrowIfNull(screens);

        this.Select(null);
        this.ScreensCanvas.Children.Clear();
        this.zOrder.Clear();

        var number = 1;
        foreach (var screenInfo in screens)
        {
            var visual = this.CreateScreenVisual(new EditableScreen(screenInfo, number++));
            this.ScreensCanvas.Children.Add(visual.Container);
            this.zOrder.Add(visual);
        }

        this.ApplyZOrder();
        this.FitToView();
    }

    public void BringToFront()
        => this.MoveSelectedInZOrder(index => this.zOrder.Count - 1);

    public void BringForward()
        => this.MoveSelectedInZOrder(index => index + 1);

    public void SendBackward()
        => this.MoveSelectedInZOrder(index => index - 1);

    public void SendToBack()
        => this.MoveSelectedInZOrder(index => 0);

    /// <summary>
    /// Puts the selected screen back where Display Settings has it, at its native size. Its
    /// z-order is left alone - resetting where a screen is shouldn't silently restack it.
    /// </summary>
    public void ResetSelectedScreen()
    {
        if (this.selected is null)
        {
            return;
        }

        this.selected.Screen.ResetToSystemPosition();
        this.FitToView();
    }

    /// <summary>
    /// Puts every screen back where Display Settings has it, at its native size, in its original
    /// stacking order - i.e. the layout the editor started with.
    /// </summary>
    public void ResetAllScreens()
    {
        foreach (var visual in this.zOrder)
        {
            visual.Screen.ResetToSystemPosition();
        }

        this.zOrder.Sort((left, right) => left.Screen.Number.CompareTo(right.Screen.Number));
        this.ApplyZOrder();
        this.FitToView();
    }

    private void MoveSelectedInZOrder(Func<int, int> getNewIndex)
    {
        if (this.selected is null)
        {
            return;
        }

        var oldIndex = this.zOrder.IndexOf(this.selected);
        var newIndex = Math.Clamp(getNewIndex(oldIndex), 0, this.zOrder.Count - 1);
        if (newIndex == oldIndex)
        {
            return;
        }

        this.zOrder.RemoveAt(oldIndex);
        this.zOrder.Insert(newIndex, this.selected);
        this.ApplyZOrder();
    }

    private void ApplyZOrder()
    {
        for (var index = 0; index < this.zOrder.Count; index++)
        {
            Canvas.SetZIndex(this.zOrder[index].Container, index);
        }
    }

    private void Select(ScreenVisual? visual)
    {
        if (visual == this.selected)
        {
            return;
        }

        this.selected?.SetSelected(false, this.Resources);
        this.selected = visual;
        this.selected?.SetSelected(true, this.Resources);
        this.UpdateAdorner();
        this.SelectionChanged?.Invoke(this, EventArgs.Empty);
    }

    private void CreateResizeHandles()
    {
        foreach (var corner in Enum.GetValues<ResizeCorner>())
        {
            var shape = corner is ResizeCorner.TopLeft or ResizeCorner.BottomRight
                ? InputSystemCursorShape.SizeNorthwestSoutheast
                : InputSystemCursorShape.SizeNortheastSouthwest;
            var handle = new CursorGrid(shape)
            {
                Width = ScreenLayoutEditor.ResizeHandleSize,
                Height = ScreenLayoutEditor.ResizeHandleSize,
                Tag = corner,
                Visibility = Visibility.Collapsed,
            };
            handle.Children.Add(new Microsoft.UI.Xaml.Shapes.Rectangle
            {
                Style = (Style)this.Resources["ResizeHandleStyle"],
            });
            handle.PointerPressed += this.ResizeHandle_PointerPressed;
            handle.PointerMoved += this.Element_PointerMoved;
            handle.PointerReleased += this.Element_PointerReleased;
            handle.PointerCaptureLost += this.Element_PointerCaptureLost;
            this.AdornerCanvas.Children.Add(handle);
            this.resizeHandles.Add(handle);
        }
    }
}
