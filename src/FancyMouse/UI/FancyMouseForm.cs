using System.Diagnostics;
using System.Drawing.Imaging;
using FancyMouse.Helpers;
using FancyMouse.Models.Drawing;
using FancyMouse.Models.Layout;
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

        if (((e.KeyCode >= Keys.D1) && (e.KeyCode <= Keys.D9))
            || ((e.KeyCode >= Keys.NumPad1) && (e.KeyCode <= Keys.NumPad9)))
        {
            // number keys 1-9 or numpad keys 1-9 - move to the numbered screen
            var screenNumber = e.KeyCode - Keys.D0;
            if (screenNumber <= screens.Count)
            {
                targetScreenNumber = screenNumber;
            }
        }
        else if (e.KeyCode == Keys.P)
        {
            // "P" - move to the primary screen
            targetScreenNumber = screens.Single(item => item.Screen.Primary).Number;
        }
        else if (e.KeyCode == Keys.Left)
        {
            // move to the previous screen
            targetScreenNumber = currentScreen.Number == 1
                ? screens.Count
                : currentScreen.Number - 1;
        }
        else if (e.KeyCode == Keys.Right)
        {
            // move to the next screen
            targetScreenNumber = currentScreen.Number == screens.Count
                ? 1
                : currentScreen.Number + 1;
        }
        else if (e.KeyCode == Keys.Home)
        {
            // move to the first screen
            targetScreenNumber = 1;
        }
        else if (e.KeyCode == Keys.End)
        {
            // move to the last screen
            targetScreenNumber = screens.Count;
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
            $"Reporting mouse event args",
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

        var layoutInfo = FancyMouseForm.GetLayoutInfo(logger, this);
        LayoutHelper.PositionForm(this, layoutInfo.FormBounds);
        FancyMouseForm.RenderPreview(this, layoutInfo);

        // we have to activate the form to make sure the deactivate event fires
        this.Activate();
    }

    private static LayoutInfo GetLayoutInfo(
        NLog.ILogger logger, FancyMouseForm form)
    {
        // map screens to their screen number in "System > Display"
        var screens = ScreenHelper.GetAllScreens()
            .Select((screen, index) => new { Screen = screen, Index = index, Number = index + 1 })
            .ToList();
        foreach (var screen in screens)
        {
            logger.Info(string.Join(
                '\n',
                $"screen[{screen.Number}]",
                $"\tprimary      = {screen.Screen.Primary}",
                $"\tdisplay area = {screen.Screen.DisplayArea}",
                $"\tworking area = {screen.Screen.WorkingArea}"));
        }

        // collect together some values that we need for calculating layout
        var activatedLocation = MouseHelper.GetCursorPosition();
        var activatedScreenHandle = ScreenHelper.MonitorFromPoint(activatedLocation);
        var activatedScreenIndex = screens
            .Single(item => item.Screen.Handle == activatedScreenHandle.Value)
            .Index;

        var layoutConfig = new LayoutConfig(
            virtualScreenBounds: ScreenHelper.GetVirtualScreen(),
            screens: screens.Select(item => item.Screen).ToList(),
            activatedLocation: activatedLocation,
            activatedScreenIndex: activatedScreenIndex,
            activatedScreenNumber: activatedScreenIndex + 1,
            maximumFormSize: form.Options.MaximumThumbnailImageSize,
            formPadding: new PaddingInfo(
                form.panel1.Padding.Left,
                form.panel1.Padding.Top,
                form.panel1.Padding.Right,
                form.panel1.Padding.Bottom),
            previewPadding: new(0));
        logger.Info(string.Join(
            '\n',
            $"Layout config",
            $"-------------",
            $"virtual screen          = {layoutConfig.VirtualScreenBounds}",
            $"activated location      = {layoutConfig.ActivatedLocation}",
            $"activated screen index  = {layoutConfig.ActivatedScreenIndex}",
            $"activated screen number = {layoutConfig.ActivatedScreenNumber}",
            $"maximum form size       = {layoutConfig.MaximumFormSize}",
            $"form padding            = {layoutConfig.FormPadding}",
            $"preview padding         = {layoutConfig.PreviewPadding}"));

        // calculate the layout coordinates for everything
        var layoutInfo = LayoutHelper.CalculateLayoutInfo(layoutConfig);
        logger.Info(string.Join(
            '\n',
            $"Layout info",
            $"-----------",
            $"form bounds      = {layoutInfo.FormBounds}",
            $"preview bounds   = {layoutInfo.PreviewBounds}",
            $"activated screen = {layoutInfo.ActivatedScreenBounds}"));

        return layoutInfo;
    }

    private static void RenderPreview(
        FancyMouseForm form, LayoutInfo layoutInfo)
    {
        var layoutConfig = layoutInfo.LayoutConfig;

        // initialize the preview image
        var preview = new Bitmap(
            (int)layoutInfo.PreviewBounds.Width,
            (int)layoutInfo.PreviewBounds.Height,
            PixelFormat.Format32bppArgb);
        form.Thumbnail.Image = preview;

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
            var activatedScreenStopwatch = Stopwatch.StartNew();
            DrawingHelper.EnsurePreviewDeviceContext(previewGraphics, ref previewHdc);
            DrawingHelper.DrawPreviewScreen(
                desktopHdc,
                previewHdc,
                layoutConfig.Screens[layoutConfig.ActivatedScreenIndex].Bounds,
                layoutInfo.ScreenBounds[layoutConfig.ActivatedScreenIndex]);
            activatedScreenStopwatch.Stop();

            // show the placeholder images if it looks like it might take a while
            // to capture the remaining screenshot images
            if (activatedScreenStopwatch.ElapsedMilliseconds > 250)
            {
                var activatedArea = layoutConfig.Screens[layoutConfig.ActivatedScreenIndex].Bounds.Area;
                var totalArea = layoutConfig.Screens.Sum(screen => screen.Bounds.Area);
                if ((activatedArea / totalArea) < 0.5M)
                {
                    // we need to release the device context handle before we can draw the placeholders
                    // using the Graphics object otherwise we'll get an error from GDI saying
                    // "Object is currently in use elsewhere"
                    DrawingHelper.FreePreviewDeviceContext(previewGraphics, ref previewHdc);
                    DrawingHelper.DrawPreviewScreenPlaceholders(
                        previewGraphics,
                        layoutInfo.ScreenBounds.Where((_, idx) => idx != layoutConfig.ActivatedScreenIndex));
                    FancyMouseForm.RefreshPreview(form);
                }
            }

            // draw the remaining screen captures (if any) on the preview image
            var sourceScreens = layoutConfig.Screens
                .Where((_, idx) => idx != layoutConfig.ActivatedScreenIndex)
                .Select(screen => screen.Bounds)
                .ToList();
            if (sourceScreens.Any())
            {
                var stopwatch = Stopwatch.StartNew();
                var targetScreens = layoutInfo.ScreenBounds
                    .Where((_, idx) => idx != layoutConfig.ActivatedScreenIndex)
                    .ToList();
                DrawingHelper.EnsurePreviewDeviceContext(previewGraphics, ref previewHdc);
                DrawingHelper.DrawPreviewScreens(
                    desktopHdc,
                    previewHdc,
                    sourceScreens,
                    targetScreens);
                FancyMouseForm.RefreshPreview(form);
                stopwatch.Stop();
            }
        }
        finally
        {
            DrawingHelper.FreeDesktopDeviceContext(ref desktopHwnd, ref desktopHdc);
            DrawingHelper.FreePreviewDeviceContext(previewGraphics, ref previewHdc);
        }

        FancyMouseForm.RefreshPreview(form);

        // we have to activate the form to make sure the deactivate event fires
        form.Activate();
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
