using FancyMouse.Models.Display;
using FancyMouse.Models.Editor;
using FancyMouse.Models.Styles;

using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace FancyMouse.WinUI3.Controls.Editor;

/// <summary>
/// A designer for the preview image itself - the preview's frame (margin, border and padding,
/// from the current <see cref="PreviewStyle"/>) is drawn at its real size in preview pixels, and
/// the user drags screens around inside its content area and resizes them by their corner
/// handles (aspect ratio locked), with z-order commands for overlapping screens. The mouse wheel
/// zooms and dragging the background pans, the way a drawing app's canvas does.
/// </summary>
/// <remarks>
/// A thin view over <see cref="PreviewDesigner"/>, which holds all of the editor's behaviour
/// (camera, placement rules, gestures) with no XAML dependency so it can be unit tested - this
/// control only forwards pointer input to it, then redraws from its state (see <c>Render</c>).
/// <para/>
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
    private readonly Dictionary<EditableScreen, ScreenVisual> visuals = [];

    private readonly List<CursorGrid> resizeHandles = [];

    /// <summary>
    /// The context menu shared by every screen - see <see cref="CreateScreenContextMenu"/>.
    /// </summary>
    private readonly MenuFlyout screenContextMenu;

    private PreviewDesigner? designer;

    public ScreenLayoutEditor()
    {
        this.InitializeComponent();
        this.screenContextMenu = this.CreateScreenContextMenu();
        this.RootGrid.ContextFlyout = this.CreateBackgroundContextMenu();
        this.CreateResizeHandles();
        this.RootGrid.PointerPressed += this.RootGrid_PointerPressed;
        this.RootGrid.PointerMoved += this.Element_PointerMoved;
        this.RootGrid.PointerReleased += this.Element_PointerReleased;
        this.RootGrid.PointerCaptureLost += this.Element_PointerCaptureLost;
        this.RootGrid.PointerWheelChanged += this.RootGrid_PointerWheelChanged;
        this.RootGrid.SizeChanged += this.RootGrid_SizeChanged;
        this.UpdateCommandStates();
    }

    /// <summary>
    /// Gets the screens' current placements, back to front.
    /// </summary>
    public IReadOnlyList<EditableScreen> Screens
        => (IReadOnlyList<EditableScreen>?)this.designer?.Design.Screens ?? [];

    /// <summary>
    /// Gets the currently selected screen, if any - the one the z-order and reset commands act on.
    /// </summary>
    public EditableScreen? SelectedScreen
        => this.designer?.Design.SelectedScreen;

    /// <summary>
    /// Raised when <see cref="SelectedScreen"/> changes.
    /// </summary>
    public event EventHandler? SelectionChanged;

    /// <summary>
    /// Replaces the editor's contents with <paramref name="screens"/> drawn in a preview styled by
    /// <paramref name="previewStyle"/>, each at its automatic layout position, stacked in the order
    /// given (first at the back).
    /// </summary>
    public void LoadScreens(IEnumerable<ScreenInfo> screens, PreviewStyle previewStyle)
    {
        if (this.designer is not null)
        {
            this.designer.SelectionChanged -= this.Designer_SelectionChanged;
        }

        this.ScreensCanvas.Children.Clear();
        this.visuals.Clear();

        this.designer = new PreviewDesigner(screens, previewStyle);
        this.designer.SelectionChanged += this.Designer_SelectionChanged;
        foreach (var screen in this.designer.Design.Screens)
        {
            var visual = this.CreateScreenVisual(screen, this.designer.Design.ScreenStyle);
            this.ScreensCanvas.Children.Add(visual.Container);
            this.visuals.Add(screen, visual);
        }

        this.ApplyPreviewStyle(previewStyle);
        this.designer.Camera.SetViewportSize(this.RootGrid.ActualWidth, this.RootGrid.ActualHeight);
        this.Render();

        // the new design starts with nothing selected
        this.UpdateCommandStates();
        this.SelectionChanged?.Invoke(this, EventArgs.Empty);
    }

    public void BringToFront()
        => this.Execute(design => design.BringToFront());

    public void BringForward()
        => this.Execute(design => design.BringForward());

    public void SendBackward()
        => this.Execute(design => design.SendBackward());

    public void SendToBack()
        => this.Execute(design => design.SendToBack());

    /// <summary>
    /// Puts the selected screen back where the automatic layout has it, without restacking it.
    /// </summary>
    public void ResetSelectedScreen()
        => this.Execute(design => design.ResetSelectedScreen());

    /// <summary>
    /// Puts every screen back where the automatic layout has it, in its original stacking order.
    /// </summary>
    public void ResetAllScreens()
        => this.Execute(design => design.ResetAllScreens());

    public void ZoomIn()
        => this.Zoom(designer => designer.ZoomIn());

    public void ZoomOut()
        => this.Zoom(designer => designer.ZoomOut());

    /// <summary>
    /// Zooms so the preview is shown at the size it will actually be on screen - one preview pixel
    /// per physical pixel, whatever the monitor's scaling.
    /// </summary>
    public void ZoomToActualSize()
        => this.Zoom(designer => designer.ZoomToActualSize(1 / (this.XamlRoot?.RasterizationScale ?? 1)));

    /// <summary>
    /// Points the camera at the whole preview, as large as fits in the editor.
    /// </summary>
    public void ZoomToFit()
        => this.Zoom(designer => designer.Camera.ZoomToFit());

    private void Zoom(Action<PreviewDesigner> zoom)
    {
        if (this.designer is null)
        {
            return;
        }

        zoom(this.designer);
        this.Render();
    }

    private void Execute(Action<PreviewDesign> command)
    {
        if (this.designer is null)
        {
            return;
        }

        command(this.designer.Design);
        this.Render();
    }

    /// <summary>
    /// Enables the commands that act on the selected screen only while one is selected.
    /// </summary>
    private void UpdateCommandStates()
    {
        var hasSelection = this.SelectedScreen is not null;
        this.SendToBackButton.IsEnabled = hasSelection;
        this.SendBackwardButton.IsEnabled = hasSelection;
        this.BringForwardButton.IsEnabled = hasSelection;
        this.BringToFrontButton.IsEnabled = hasSelection;
        this.ResetScreenButton.IsEnabled = hasSelection;
    }

    private void Designer_SelectionChanged(object? sender, EventArgs e)
    {
        this.UpdateCommandStates();
        this.SelectionChanged?.Invoke(this, EventArgs.Empty);
    }

    private void SendToBackButton_Click(object sender, RoutedEventArgs e)
        => this.SendToBack();

    private void SendBackwardButton_Click(object sender, RoutedEventArgs e)
        => this.SendBackward();

    private void BringForwardButton_Click(object sender, RoutedEventArgs e)
        => this.BringForward();

    private void BringToFrontButton_Click(object sender, RoutedEventArgs e)
        => this.BringToFront();

    private void ZoomInButton_Click(object sender, RoutedEventArgs e)
        => this.ZoomIn();

    private void ZoomOutButton_Click(object sender, RoutedEventArgs e)
        => this.ZoomOut();

    private void ZoomActualSizeButton_Click(object sender, RoutedEventArgs e)
        => this.ZoomToActualSize();

    private void ZoomToFitButton_Click(object sender, RoutedEventArgs e)
        => this.ZoomToFit();

    private void ResetScreenButton_Click(object sender, RoutedEventArgs e)
        => this.ResetSelectedScreen();

    private void ResetAllButton_Click(object sender, RoutedEventArgs e)
        => this.ResetAllScreens();

    /// <summary>
    /// Creates the screens' context menu - the tool strip's layer and reset commands, acting on
    /// the selected screen, which is the one that was right-clicked (see <c>Screen_PointerPressed</c>).
    /// </summary>
    private MenuFlyout CreateScreenContextMenu()
        => ScreenLayoutEditor.CreateContextMenu(
            ("Send to back", ScreenLayoutEditor.CreateIcon(""), this.SendToBack),
            ("Send backward", ScreenLayoutEditor.CreateIcon(""), this.SendBackward),
            ("Bring forward", ScreenLayoutEditor.CreateIcon(""), this.BringForward),
            ("Bring to front", ScreenLayoutEditor.CreateIcon(""), this.BringToFront),
            null,
            ("Use system position", ScreenLayoutEditor.CreateIcon(""), this.ResetSelectedScreen),
            ("Reset all", ScreenLayoutEditor.CreateIcon(""), this.ResetAllScreens));

    /// <summary>
    /// Creates the background's context menu - the tool strip's zoom commands. A screen's own
    /// context menu takes precedence over this one when a screen is right-clicked.
    /// </summary>
    private MenuFlyout CreateBackgroundContextMenu()
        => ScreenLayoutEditor.CreateContextMenu(
            ("Zoom in", ScreenLayoutEditor.CreateIcon(""), this.ZoomIn),
            ("Zoom out", ScreenLayoutEditor.CreateIcon(""), this.ZoomOut),
            ("Actual size (100%)", new FontIcon { FontFamily = new FontFamily("Segoe UI"), FontSize = 12, Glyph = "1:1" }, this.ZoomToActualSize),
            ("Zoom to fit", ScreenLayoutEditor.CreateIcon(""), this.ZoomToFit));

    /// <summary>
    /// Creates a context menu with an item per command, and a separator for each null.
    /// </summary>
    private static MenuFlyout CreateContextMenu(params (string Text, IconElement Icon, Action Execute)?[] commands)
    {
        var menu = new MenuFlyout();
        foreach (var command in commands)
        {
            if (command is not { } item)
            {
                menu.Items.Add(new MenuFlyoutSeparator());
                continue;
            }

            var menuItem = new MenuFlyoutItem { Text = item.Text, Icon = item.Icon };
            menuItem.Click += (sender, e) => item.Execute();
            menu.Items.Add(menuItem);
        }

        return menu;
    }

    private static FontIcon CreateIcon(string glyph)
        => new() { Glyph = glyph };

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
