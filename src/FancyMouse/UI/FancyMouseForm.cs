using System.Diagnostics;

using FancyMouse.Common.Helpers;
using FancyMouse.Common.Imaging;
using FancyMouse.Internal.Helpers;
using FancyMouse.Models.Display;
using FancyMouse.Models.Drawing;
using FancyMouse.Models.ViewModel;
using FancyMouse.PlatformServices.Abstractions;
using NLog;

namespace FancyMouse.UI;

internal sealed partial class FancyMouseForm : Form
{
    public FancyMouseForm(ILogger logger, IPlatformServices platformServices)
    {
        this.Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.PlatformServices = platformServices ?? throw new ArgumentNullException(nameof(platformServices));
        this.Screens = new();
        this.InitializeComponent();
    }

    private ILogger Logger
    {
        get;
    }

    private IPlatformServices PlatformServices
    {
        get;
    }

    private List<ScreenInfo> Screens
    {
        get;
        set;
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

        // filter out any keys we don't handle below
        var handledKeys = new List<Keys>
        {
            Keys.D1, Keys.D2, Keys.D3, Keys.D4, Keys.D5, Keys.D6, Keys.D7, Keys.D8, Keys.D9,
            Keys.NumPad1, Keys.NumPad2, Keys.NumPad3, Keys.NumPad4, Keys.NumPad5, Keys.NumPad6, Keys.NumPad7, Keys.NumPad8, Keys.NumPad9,
            Keys.P, Keys.Left, Keys.Right, Keys.Home, Keys.End,
        };
        if (!handledKeys.Contains(e.KeyCode))
        {
            return;
        }

        // calculate some values used in the key handlers
        var screens = this.Screens;
        var currentLocation = this.PlatformServices.Mouse.GetCursorPosition();
        var currentScreen = ScreenHelper.GetScreenFromPoint(this.Screens, new(currentLocation.X, currentLocation.Y));
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
            var midpoint = targetScreen.DisplayArea.Midpoint;
            this.PlatformServices.Mouse.SetCursorPosition(
                new((int)midpoint.X, (int)midpoint.Y));
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
                // no screenshot was clicked - must have clicked the background or window border
                // so we'll do nothing, and leave the form visible
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
            this.PlatformServices.Mouse.SetCursorPosition(
                new((int)clickedLocation.X, (int)clickedLocation.Y));
        }

        this.OnDeactivate(EventArgs.Empty);
    }

    public async Task ShowPreview()
    {
        var logger = this.Logger;

        logger.Info(string.Join(
            '\n',
            "-----------",
            nameof(FancyMouseForm.ShowPreview),
            "-----------"));

        // hide the form while we redraw it...
        this.Visible = false;

        // snapshot the current screen configuraiton. we'll
        // assume it doesn't change for the duration of the preview,
        // and we'll verify any click is still valid before we try
        // to move the mouse to the clicked location
        this.Screens = ScreenHelper.GetAllScreens().ToList();

        // don't show the preview if there are no screens connected
        if (this.Screens.Count == 0)
        {
            return;
        }

        var stopwatch = Stopwatch.StartNew();

        // capture this first so we get an accurate mouse location
        // (in case the user moves it a few pixels while the form is rendered)
        var cursorPosition = this.PlatformServices.Mouse.GetCursorPosition();
        var activatedLocation = new PointInfo(cursorPosition.X, cursorPosition.Y);

        var appSettings = ConfigHelper.AppSettings ?? throw new InvalidOperationException();
        var displayInfo = DeviceHelper.GetDisplayInfo();
        var activatedScreen = DeviceHelper.GetActivatedScreen(displayInfo.Devices[0], activatedLocation);

        var formLayout = LayoutHelper.GetFormLayout(
            previewStyle: appSettings.PreviewStyle,
            displayInfo,
            activatedScreen: activatedScreen,
            activatedLocation: activatedLocation);

        // remember this so we can map the mouse clicks back to
        // the appropriate device and screen location
        this.FormLayout = formLayout;

        await this.PositionFormAsync(formLayout.FormBounds);

        var imageCopyServices = displayInfo.Devices
            .Select(
                deviceInfo => (IImageRegionCopyService)new DesktopImageRegionCopyService())
            .ToList();

        await DrawingHelper.RenderPreviewAsync(
            this.FormLayout.CanvasLayout,
            activatedScreen,
            imageCopyServices,
            this.OnPreviewImageCreatedAsync,
            this.OnPreviewImageUpdatedAsync);

        stopwatch.Stop();

        // we have to activate the form to make sure the deactivate event fires
        await this.ActivateAsync();
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
    private async Task PositionFormAsync(RectangleInfo bounds)
    {
        await this.InvokeAsync(
            () =>
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
            });
    }

    private async Task ActivateAsync()
    {
        await this.InvokeAsync(
            () =>
            {
                this.Activate();
            });
    }

    private async Task OnPreviewImageCreatedAsync(Bitmap preview)
    {
        await this.InvokeAsync(
            () =>
            {
                this.ClearPreview();
                this.Thumbnail.Image = preview;
            });
    }

    private async Task OnPreviewImageUpdatedAsync(Bitmap preview)
    {
        await this.InvokeAsync(
            () =>
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
            });
    }
}
