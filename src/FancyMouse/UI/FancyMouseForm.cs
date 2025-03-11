using System.Diagnostics;
using FancyMouse.Common.Helpers;
using FancyMouse.Common.Imaging;
using FancyMouse.Common.Models.Display;
using FancyMouse.Common.Models.Drawing;
using FancyMouse.Common.Models.ViewModel;
using NLog;

namespace FancyMouse.UI;

internal sealed partial class FancyMouseForm : Form
{
    public FancyMouseForm(ILogger logger)
    {
        this.Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.InitializeComponent();
    }

    private ILogger Logger
    {
        get;
    }

    private FormViewModel? FormLayout
    {
        get;
        set;
    }

    private void FancyMouseForm_Load(object sender, EventArgs e)
    {
    }

    private void FancyMouseForm_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Escape)
        {
            this.OnDeactivate(EventArgs.Empty);
            return;
        }

        var screens = ScreenHelper.GetAllScreens().ToList();
        if (screens.Count == 0)
        {
            return;
        }

        var currentLocation = MouseHelper.GetCursorPosition();
        var currentScreen = ScreenHelper.GetScreenFromPoint(screens, currentLocation);
        var currentScreenIndex = screens.IndexOf(currentScreen);
        var targetScreen = default(ScreenInfo?);

        switch (e.KeyCode)
        {
            case >= Keys.D1 and <= Keys.D9:
                {
                    // number keys 1-9 - move to the numbered screen
                    var screenNumber = e.KeyCode - Keys.D0;
                    /* note - screen *numbers* are 1-based, screen *indexes* are 0-based */
                    targetScreen = (screenNumber <= screens.Count)
                        ? targetScreen = screens[screenNumber - 1]
                        : null;
                    break;
                }

            case >= Keys.NumPad1 and <= Keys.NumPad9:
                {
                    // numpad keys 1-9 - move to the numbered screen
                    var screenNumber = e.KeyCode - Keys.NumPad0;
                    /* note - screen *numbers* are 1-based, screen *indexes* are 0-based */
                    targetScreen = (screenNumber <= screens.Count)
                        ? targetScreen = screens[screenNumber - 1]
                        : null;
                    break;
                }

            case Keys.P:
                // "P" - move to the primary screen
                targetScreen = screens.Single(screen => screen.Primary);
                break;
            case Keys.Left:
                // move to the previous screen, looping back to the end if needed
                var prevIndex = (currentScreenIndex - 1 + screens.Count) % screens.Count;
                targetScreen = screens[prevIndex];
                break;
            case Keys.Right:
                // move to the next screen, looping round to the start if needed
                var nextIndex = (currentScreenIndex + 1) % screens.Count;
                targetScreen = screens[nextIndex];
                break;
            case Keys.Home:
                // move to the first screen
                targetScreen = screens.First();
                break;
            case Keys.End:
                // move to the last screen
                targetScreen = screens.Last();
                break;
        }

        if (targetScreen is not null)
        {
            MouseHelper.SetCursorPosition(targetScreen.DisplayArea.Midpoint);
            this.OnDeactivate(EventArgs.Empty);
        }
    }

    private void FancyMouseForm_Deactivate(object sender, EventArgs e)
    {
        this.Hide();
        this.ClearPreview();
    }

    private void Thumbnail_Click(object sender, EventArgs e)
    {
        var logger = this.Logger;

        logger.Info(string.Join(
            '\n',
            "-----------",
            nameof(FancyMouseForm.Thumbnail_Click),
            "-----------"));

        var mouseEventArgs = (MouseEventArgs)e;
        logger.Info(string.Join(
            '\n',
            "Reporting mouse event args",
            $"\tbutton   = {mouseEventArgs.Button}",
            $"\tlocation = {mouseEventArgs.Location}"));

        if (mouseEventArgs.Button == MouseButtons.Left)
        {
            if (this.FormLayout is null)
            {
                // there's no layout data so we can't work out what screen was clicked
                return;
            }

            // work out which screenshot was clicked
            var devicePairings = this.FormLayout.CanvasLayout.DeviceLayouts
                .SelectMany(
                    deviceLayout => deviceLayout.ScreenLayouts,
                    (deviceLayout, screenLayout) => new { DeviceLayout = deviceLayout, ScreenLayout = screenLayout })
                .ToList();
            var clickedPairing = devicePairings
                .FirstOrDefault(
                    deviceParing => deviceParing.ScreenLayout.ScreenBounds.OuterBounds.Contains(mouseEventArgs.X, mouseEventArgs.Y));
            if (clickedPairing is null)
            {
                return;
            }

            // scale up the click onto the physical screen - the aspect ratio of the screenshot
            // might be distorted compared to the physical screen due to the borders around the
            // screenshot, so we need to work out the target location on the physical screen first
            var clickedScreen = clickedPairing.ScreenLayout;
            var clickedDisplayArea = clickedScreen.ScreenInfo.DisplayArea;
            var clickedLocation = new PointInfo(mouseEventArgs.Location)
                .Stretch(
                    source: clickedScreen.ScreenBounds.ContentBounds,
                    target: clickedDisplayArea)
                .Clamp(
                    new(
                        x: clickedDisplayArea.X + 1,
                        y: clickedDisplayArea.Y + 1,
                        width: clickedDisplayArea.Width - 1,
                        height: clickedDisplayArea.Height - 1
                    ))
                .Truncate();

            // move mouse pointer
            logger.Info($"clicked location = {clickedLocation}");
            MouseHelper.SetCursorPosition(clickedLocation);
        }

        this.OnDeactivate(EventArgs.Empty);
    }

    public void ShowPreview()
    {
        var logger = this.Logger;

        logger.Info(string.Join(
            '\n',
            "-----------",
            nameof(FancyMouseForm.ShowPreview),
            "-----------"));

        // hide the form while we redraw it...
        this.Visible = false;

        var stopwatch = Stopwatch.StartNew();

        var appSettings = Internal.Helpers.ConfigHelper.AppSettings ?? throw new InvalidOperationException();
        var activatedLocation = MouseHelper.GetCursorPosition();
        var displayInfo = new DisplayInfo(
            devices: new List<DeviceInfo>()
            {
                new(
                    hostname: Environment.MachineName,
                    localhost: true,
                    screens: ScreenHelper.GetAllScreens()),
            });
        var activatedScreen = displayInfo.Devices
            .Single(device => device.Localhost)
            .Screens
            .Single(screen => screen.DisplayArea.Contains(activatedLocation));

        var formLayout = LayoutHelper.GetFormLayout(
            previewStyle: appSettings.PreviewStyle,
            displayInfo,
            activatedScreen: activatedScreen,
            activatedLocation: activatedLocation);

        this.FormLayout = formLayout;

        this.PositionForm(formLayout.FormBounds);

        var imageCopyService = new DesktopImageRegionCopyService();

        DrawingHelper.RenderPreview(
            this.FormLayout.CanvasLayout,
            activatedScreen,
            imageCopyService,
            this.OnPreviewImageCreated,
            this.OnPreviewImageUpdated);

        stopwatch.Stop();

        // we have to activate the form to make sure the deactivate event fires
        this.Activate();
    }

    private void ClearPreview()
    {
        if (this.Thumbnail.Image is null)
        {
            return;
        }

        var tmp = this.Thumbnail.Image;
        this.Thumbnail.Image = null;
        tmp.Dispose();

        // force preview image memory to be released, otherwise
        // all the disposed images can pile up without being GC'ed
        GC.Collect();
    }

    /// <summary>
    /// Resize and position the specified form.
    /// </summary>
    private void PositionForm(RectangleInfo bounds)
    {
        // note - do this in two steps rather than "this.Bounds = formBounds" as there
        // appears to be an issue in WinForms with dpi scaling even when using PerMonitorV2,
        // where the form scaling uses either the *primary* screen scaling or the *previous*
        // screen's scaling when the form is moved to a different screen. i've got no idea
        // *why*, but the exact sequence of calls below seems to be a workaround...
        // see https://github.com/mikeclayton/FancyMouse/issues/2
        var rect = bounds.ToRectangle();
        this.Location = rect.Location;
        _ = this.PointToScreen(Point.Empty);
        this.Size = rect.Size;
    }

    private void OnPreviewImageCreated(Bitmap preview)
    {
        this.ClearPreview();
        this.Thumbnail.Image = preview;
    }

    private void OnPreviewImageUpdated()
    {
        if (!this.Visible)
        {
            // we seem to need to turn off topmost and then re-enable it again
            // when we show the form, otherwise it doesn't always get shown topmost...
            this.TopMost = false;
            this.TopMost = true;
            this.Show();
        }

        this.Thumbnail.Refresh();
    }
}
