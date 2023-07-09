using System.Diagnostics;
using System.Drawing.Imaging;
using FancyMouse.Helpers;
using FancyMouse.Models.Drawing;
using FancyMouse.Models.Layout;
using FancyMouse.Models.Screen;
using NLog;
using static FancyMouse.NativeMethods.Core;

namespace FancyMouse.UI;

internal partial class FancyMouseForm : Form
{
    public FancyMouseForm(FancyMouseDialogOptions options)
    {
        this.Options = options ?? throw new ArgumentNullException(nameof(options));
        this.InitializeComponent();
    }

    private FancyMouseDialogOptions Options
    {
        get;
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
                screens[targetScreenNumber.Value - 1].Screen.Bounds.Midpoint);
            this.OnDeactivate(EventArgs.Empty);
        }
    }

    private void FancyMouseForm_Deactivate(object sender, EventArgs e)
    {
        this.Hide();

        if (this.Thumbnail.Image is not null)
        {
            var tmp = this.Thumbnail.Image;
            this.Thumbnail.Image = null;
            tmp.Dispose();
        }
    }

    private void Thumbnail_Click(object sender, EventArgs e)
    {
        var logger = this.Options.Logger;

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
            // plain click - move mouse pointer
            var virtualScreen = ScreenHelper.GetVirtualScreen();
            var scaledLocation = MouseHelper.GetJumpLocation(
                new PointInfo(mouseEventArgs.X, mouseEventArgs.Y),
                new SizeInfo(this.Thumbnail.Size),
                virtualScreen);
            logger.Info($"scaled location = {scaledLocation}");
            MouseHelper.SetCursorPosition(scaledLocation);
        }

        this.OnDeactivate(EventArgs.Empty);
    }

    public void ShowThumbnail()
    {
        var logger = this.Options.Logger;

        logger.Info(string.Join(
            '\n',
            "-----------",
            nameof(FancyMouseForm.ShowThumbnail),
            "-----------"));

        var stopwatch = Stopwatch.StartNew();

        var screens = ScreenHelper.GetAllScreens();
        var activatedLocation = MouseHelper.GetCursorPosition();
        var previewLayout = LayoutHelper.GetPreviewLayout(
            previewSettings: this.Options.PreviewSettings,
            screens: screens,
            activatedLocation: activatedLocation);

        LayoutHelper.PositionForm(this, previewLayout.FormBounds);
        FancyMouseForm.RenderPreview(this, previewLayout);
        stopwatch.Stop();

        // we have to activate the form to make sure the deactivate event fires
        this.Activate();
    }

    private static void RenderPreview(
        FancyMouseForm form, PreviewLayout previewLayout)
    {
        var stopwatch = Stopwatch.StartNew();

        // initialize the preview image
        var previewImage = new Bitmap(
            (int)previewLayout.PreviewBounds.OuterBounds.Width,
            (int)previewLayout.PreviewBounds.OuterBounds.Height,
            PixelFormat.Format32bppArgb);
        form.Thumbnail.Image = previewImage;

        using var previewGraphics = Graphics.FromImage(previewImage);

        DrawingHelper.DrawBoxBorder(previewGraphics, previewLayout.PreviewStyle, previewLayout.PreviewBounds);
        DrawingHelper.DrawBoxBackground(
            previewGraphics,
            previewLayout.PreviewStyle,
            previewLayout.PreviewBounds,
            Enumerable.Empty<RectangleInfo>());

        var desktopHwnd = HWND.Null;
        var desktopHdc = HDC.Null;
        var previewHdc = HDC.Null;
        try
        {
            // sort the source and target screen areas, putting the activated screen first
            // (we need to capture and draw the activated screen before we show the form
            // because otherwise we'll capture the form as part of the screenshot!)
            var activatedScreenIndex = previewLayout.Screens.IndexOf(previewLayout.ActivatedScreen);
            var sourceScreens = new List<ScreenInfo> { previewLayout.ActivatedScreen }
                .Union(previewLayout.Screens.Where((_, idx) => idx != activatedScreenIndex))
                .Select(screen => screen.Bounds)
                .ToList();
            var targetScreens = previewLayout.ScreenshotBounds
                .Where((_, idx) => idx == activatedScreenIndex)
                .Union(previewLayout.ScreenshotBounds.Where((_, idx) => idx != activatedScreenIndex))
                .ToList();

            DrawingHelper.EnsureDesktopDeviceContext(ref desktopHwnd, ref desktopHdc);

            // draw all the screenshot bezels
            foreach (var screenshotBounds in previewLayout.ScreenshotBounds)
            {
                DrawingHelper.DrawBoxBorder(
                    previewGraphics, previewLayout.ScreenshotStyle, screenshotBounds);
            }

            var placeholdersDrawn = false;
            for (var i = 0; i < sourceScreens.Count; i++)
            {
                DrawingHelper.EnsurePreviewDeviceContext(previewGraphics, ref previewHdc);
                DrawingHelper.DrawScreenshot(
                    desktopHdc, previewHdc, sourceScreens[i], targetScreens[i]);

                // show the placeholder images and show the form if it looks like it might take
                // a while to capture the remaining screenshot images (but only if there are any)
                if ((i >= (sourceScreens.Count - 1)) || (stopwatch.ElapsedMilliseconds <= 250))
                {
                    continue;
                }

                // we need to release the device context handle before we draw anything
                // using the Graphics object otherwise we'll get an error from GDI saying
                // "Object is currently in use elsewhere"
                DrawingHelper.FreePreviewDeviceContext(previewGraphics, ref previewHdc);

                if (!placeholdersDrawn)
                {
                    // draw placeholders for any undrawn screens
                    DrawingHelper.DrawPlaceholders(
                        previewGraphics,
                        previewLayout.ScreenshotStyle,
                        targetScreens.Where((_, idx) => idx > i).ToList());
                    placeholdersDrawn = true;
                }

                FancyMouseForm.RefreshPreview(form);
            }
        }
        finally
        {
            DrawingHelper.FreeDesktopDeviceContext(ref desktopHwnd, ref desktopHdc);
            DrawingHelper.FreePreviewDeviceContext(previewGraphics, ref previewHdc);
        }

        FancyMouseForm.RefreshPreview(form);
        stopwatch.Stop();
    }

    private static void RefreshPreview(FancyMouseForm form)
    {
        if (!form.Visible)
        {
            form.Show();
        }

        form.Thumbnail.Refresh();
    }
}
