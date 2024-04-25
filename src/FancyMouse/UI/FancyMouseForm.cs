using System.Diagnostics;
using FancyMouse.Common.Helpers;
using FancyMouse.Common.Imaging;
using FancyMouse.Common.Models.Drawing;
using FancyMouse.Common.Models.Layout;
using FancyMouse.Helpers;
using NLog;

namespace FancyMouse.UI;

internal partial class FancyMouseForm : Form
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

    private PreviewLayout? PreviewLayout
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

        // map screens to their screen number in "System > Display"
        var screens = ScreenHelper.GetAllScreens()
            .Select((screen, index) => new { Screen = screen, Index = index, Number = index + 1 })
            .ToList();

        var currentLocation = MouseHelper.GetCursorPosition();
        var currentScreenHandle = ScreenHelper.MonitorFromPoint(currentLocation);
        var currentScreen = screens
            .Single(item => item.Screen.Handle == currentScreenHandle.Value);
        var targetScreenNumber = default(int?);

        switch (e.KeyCode)
        {
            case (>= Keys.D1 and <= Keys.D9) or (>= Keys.NumPad1 and <= Keys.NumPad9):
                // number keys 1-9 or numpad keys 1-9 - move to the numbered screen
                var screenNumber = e.KeyCode - Keys.D0;
                if (screenNumber <= screens.Count)
                {
                    targetScreenNumber = screenNumber;
                }

                break;
            case Keys.P:
                // "P" - move to the primary screen
                targetScreenNumber = screens.Single(item => item.Screen.Primary).Number;
                break;
            case Keys.Left:
                // move to the previous screen
                targetScreenNumber = currentScreen.Number == 1
                    ? screens.Count
                    : currentScreen.Number - 1;
                break;
            case Keys.Right:
                // move to the next screen
                targetScreenNumber = currentScreen.Number == screens.Count
                    ? 1
                    : currentScreen.Number + 1;
                break;
            case Keys.Home:
                // move to the first screen
                targetScreenNumber = 1;
                break;
            case Keys.End:
                // move to the last screen
                targetScreenNumber = screens.Count;
                break;
        }

        if (targetScreenNumber.HasValue)
        {
            MouseHelper.SetCursorPosition(
                screens[targetScreenNumber.Value - 1].Screen.DisplayArea.Midpoint);
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
            // work out which screenshot was clicked
            var clickedScreenshot = (this.PreviewLayout ?? throw new InvalidOperationException())
                .ScreenshotBounds
                .SingleOrDefault(
                    box => box.BorderBounds.Contains(mouseEventArgs.X, mouseEventArgs.Y));
            if (clickedScreenshot is null)
            {
                return;
            }

            // scale up the click onto the physical screen - the aspect ratio of the screenshot
            // might be distorted compared to the physical screen due to the borders around the
            // screenshot, so we need to work out the target location on the physical screen first
            var clickedScreen =
                this.PreviewLayout.Screens[this.PreviewLayout.ScreenshotBounds.IndexOf(clickedScreenshot)];
            var clickedLocation = new PointInfo(mouseEventArgs.Location)
                .Stretch(
                    source: clickedScreenshot.ContentBounds,
                    target: clickedScreen)
                .Clamp(
                    new(
                        x: clickedScreen.X + 1,
                        y: clickedScreen.Y + 1,
                        width: clickedScreen.Width - 1,
                        height: clickedScreen.Height - 1
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

        var appSettings = ConfigHelper.AppSettings ?? throw new InvalidOperationException();
        var screens = ScreenHelper.GetAllScreens().Select(screen => screen.DisplayArea).ToList();
        var activatedLocation = MouseHelper.GetCursorPosition();
        this.PreviewLayout = LayoutHelper.GetPreviewLayout(
            previewStyle: appSettings.PreviewStyle,
            screens: screens,
            activatedLocation: activatedLocation);

        this.PositionForm(this.PreviewLayout.FormBounds);

        var imageCopyService = new DesktopImageRegionCopyService();
        DrawingHelper.RenderPreview(
            this.PreviewLayout,
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
            this.Show();
        }

        this.Thumbnail.Refresh();
    }
}
