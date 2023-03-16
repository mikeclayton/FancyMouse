using System.Diagnostics;
using System.Drawing.Imaging;
using FancyMouse.Drawing.Models;
using FancyMouse.Helpers;
using FancyMouse.NativeMethods.Core;

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

        var cursorPosition = Cursor.Position;

        // map screens to their screen number in "System > Display"
        var screens = Screen.AllScreens.Select((screen, index) => new { Screen = screen, Index = index + 1 }).ToList();
        var currentScreen = screens.Single(item => item.Screen.Bounds.Contains(cursorPosition));
        var targetScreen = default(int?);

        if ((e.KeyCode >= Keys.D1) && (e.KeyCode <= Keys.D9))
        {
            // number keys 1-9 - move to the numbered screen
            var screenNumber = e.KeyCode - Keys.D0;
            if (screenNumber <= screens.Count)
            {
                targetScreen = screenNumber;
            }
        }
        else if ((e.KeyCode >= Keys.NumPad1) && (e.KeyCode <= Keys.NumPad9))
        {
            // num-pad keys 1-9 - move to the numbered screen
            var screenNumber = e.KeyCode - Keys.NumPad0;
            if (screenNumber <= screens.Count)
            {
                targetScreen = screenNumber;
            }
        }
        else if (e.KeyCode == Keys.P)
        {
            // "P" - move to the primary screen
            targetScreen = screens.Single(item => item.Screen.Primary).Index;
        }
        else if (e.KeyCode == Keys.Left)
        {
            // move to the previous screen
            targetScreen = currentScreen.Index == 1
                ? screens.Count
                : currentScreen.Index - 1;
        }
        else if (e.KeyCode == Keys.Right)
        {
            // move to the next screen
            targetScreen = currentScreen.Index == screens.Count
                ? 1
                : currentScreen.Index + 1;
        }
        else if (e.KeyCode == Keys.Home)
        {
            // move to the first screen
            targetScreen = 1;
        }
        else if (e.KeyCode == Keys.End)
        {
            // move to the last screen
            targetScreen = screens.Count;
        }

        if (targetScreen.HasValue)
        {
            MouseHelper.JumpCursor(
                new RectangleInfo(screens[targetScreen.Value - 1].Screen.Bounds).Midpoint);
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
            $"Reporting mouse event args",
            $"\tbutton   = {mouseEventArgs.Button}",
            $"\tlocation = {mouseEventArgs.Location}"));

        if (mouseEventArgs.Button == MouseButtons.Left)
        {
            // plain click - move mouse pointer
            var scaledLocation = MouseHelper.GetJumpLocation(
                new PointInfo(mouseEventArgs.X, mouseEventArgs.Y),
                new SizeInfo(this.Thumbnail.Size),
                new RectangleInfo(SystemInformation.VirtualScreen));
            logger.Info($"scaled location = {scaledLocation}");
            MouseHelper.JumpCursor(scaledLocation);
            MouseHelper.SimulateMouseMovementEvent(scaledLocation.ToPoint());
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

        var screens = Screen.AllScreens;
        foreach (var i in Enumerable.Range(0, screens.Length))
        {
            var screen = screens[i];
            logger.Info(string.Join(
                '\n',
                $"screen[{i}] = \"{screen.DeviceName}\"",
                $"\tprimary      = {screen.Primary}",
                $"\tbounds       = {screen.Bounds}",
                $"\tworking area = {screen.WorkingArea}"));
        }

        // collect together some values that we need for calculating layout
        var activatedLocation = Cursor.Position;
        var layoutConfig = new LayoutConfig(
            virtualScreen: SystemInformation.VirtualScreen,
            screenBounds: Screen.AllScreens.Select(screen => screen.Bounds),
            activatedLocation: activatedLocation,
            activatedScreen: Array.IndexOf(Screen.AllScreens, Screen.FromPoint(activatedLocation)),
            maximumFormSize: this.Options.MaximumThumbnailImageSize,
            formPadding: this.panel1.Padding,
            previewPadding: new Padding(0));
        logger.Info(string.Join(
            '\n',
            $"Layout config",
            $"-------------",
            $"virtual screen     = {layoutConfig.VirtualScreen}",
            $"activated location = {layoutConfig.ActivatedLocation}",
            $"activated screen   = {layoutConfig.ActivatedScreen}",
            $"maximum form size  = {layoutConfig.MaximumFormSize}",
            $"form padding       = {layoutConfig.FormPadding}",
            $"preview padding    = {layoutConfig.PreviewPadding}"));

        // calculate the layout coordinates for everything
        var layoutInfo = DrawingHelper.CalculateLayoutInfo(layoutConfig);
        logger.Info(string.Join(
            '\n',
            $"Layout info",
            $"-----------",
            $"form bounds      = {layoutInfo.FormBounds}",
            $"preview bounds   = {layoutInfo.PreviewBounds}",
            $"activated screen = {layoutInfo.ActivatedScreen}"));

        DrawingHelper.PositionForm(this, layoutInfo.FormBounds);

        // initialize the preview image
        var preview = new Bitmap(
            (int)layoutInfo.PreviewBounds.Width,
            (int)layoutInfo.PreviewBounds.Height,
            PixelFormat.Format32bppArgb);
        this.Thumbnail.Image = preview;

        using var previewGraphics = Graphics.FromImage(preview);

        DrawingHelper.DrawPreviewBackground(previewGraphics, layoutInfo.PreviewBounds, layoutInfo.ScreenBounds);

        var desktopHwnd = HWND.Null;
        var desktopHdc = HDC.Null;
        var previewHdc = HDC.Null;
        try
        {
            DrawingHelper.EnsureDesktopDeviceContext(ref desktopHwnd, ref desktopHdc);

            // we have to capture the screen where we're going to show the form first
            // as the form will obscure the screen as soon as it's visible
            var activatedStopwatch = Stopwatch.StartNew();
            DrawingHelper.EnsurePreviewDeviceContext(previewGraphics, ref previewHdc);
            DrawingHelper.DrawPreviewScreen(
                desktopHdc,
                previewHdc,
                layoutConfig.ScreenBounds[layoutConfig.ActivatedScreen],
                layoutInfo.ScreenBounds[layoutConfig.ActivatedScreen]);
            activatedStopwatch.Stop();

            // show the placeholder images if it looks like it might take a while
            // to capture the remaining screenshot images
            if (activatedStopwatch.ElapsedMilliseconds > 250)
            {
                var activatedArea = layoutConfig.ScreenBounds[layoutConfig.ActivatedScreen].Area;
                var totalArea = layoutConfig.ScreenBounds.Sum(screen => screen.Area);
                if ((activatedArea / totalArea) < 0.5M)
                {
                    // we need to release the device context handle before we can draw the placeholders
                    // using the Graphics object otherwise we'll get an error from GDI saying
                    // "Object is currently in use elsewhere"
                    DrawingHelper.FreePreviewDeviceContext(previewGraphics, ref previewHdc);
                    DrawingHelper.DrawPreviewPlaceholders(
                        previewGraphics,
                        layoutInfo.ScreenBounds.Where((_, idx) => idx != layoutConfig.ActivatedScreen));
                    FancyMouseForm.ShowPreview(this);
                }
            }

            // draw the remaining screen captures (if any) on the preview image
            var sourceScreens = layoutConfig.ScreenBounds.Where((_, idx) => idx != layoutConfig.ActivatedScreen).ToList();
            if (sourceScreens.Any())
            {
                DrawingHelper.EnsurePreviewDeviceContext(previewGraphics, ref previewHdc);
                DrawingHelper.DrawPreviewScreens(
                    desktopHdc,
                    previewHdc,
                    sourceScreens,
                    layoutInfo.ScreenBounds.Where((_, idx) => idx != layoutConfig.ActivatedScreen).ToList());
                FancyMouseForm.ShowPreview(this);
            }
        }
        finally
        {
            DrawingHelper.FreeDesktopDeviceContext(ref desktopHwnd, ref desktopHdc);
            DrawingHelper.FreePreviewDeviceContext(previewGraphics, ref previewHdc);
        }

        // we have to activate the form to make sure the deactivate event fires
        FancyMouseForm.ShowPreview(this);
        this.Activate();
    }

    private static void ShowPreview(FancyMouseForm form)
    {
        if (!form.Visible)
        {
            form.Show();
        }

        form.Thumbnail.Refresh();
    }
}
