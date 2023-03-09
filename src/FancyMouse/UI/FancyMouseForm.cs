using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using FancyMouse.Drawing;
using FancyMouse.Drawing.Models;
using FancyMouse.Helpers;
using FancyMouse.NativeMethods.Core;
using FancyMouse.NativeWrappers;

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
        }
    }

    private void FancyMouseForm_Deactivate(object sender, EventArgs e)
    {
        // dispose the existing image if there is one
        if (Thumbnail.Image != null)
        {
            Thumbnail.Image.Dispose();
            Thumbnail.Image = null;
        }

        this.Hide();
    }

    private void Thumbnail_Click(object sender, EventArgs e)
    {
        var logger = this.Options.Logger;

        logger.Debug(string.Join(
            "\r\n",
            "-----------",
            nameof(FancyMouseForm.Thumbnail_Click),
            "-----------"));

        var mouseEventArgs = (MouseEventArgs)e;
        logger.Debug(string.Join(
            "\r\n",
            $"mouse event args = ",
            $"    button   = {mouseEventArgs.Button}",
            $"    location = {mouseEventArgs.Location} "));

        if (mouseEventArgs.Button == MouseButtons.Left)
        {
            // plain click - move mouse pointer
            var desktopBounds = Screen.AllScreens
                .Select(
                    screen => screen.Bounds)
                .ToList()
                .GetBoundingRectangle();
            logger.Debug($"desktop bounds  = {desktopBounds}");

            var mouseEvent = (MouseEventArgs)e;

            var scaledLocation = new PointInfo(mouseEvent.X, mouseEvent.Y)
                    .Scale(new SizeInfo(this.Thumbnail.Size).ScaleToFitRatio(new(desktopBounds.Size)))
                    .ToPoint();
            logger.Debug($"scaled location = {scaledLocation}");

            // set the new cursor position *twice* - the cursor sometimes end up in
            // the wrong place if we try to cross the dead space between non-aligned
            // monitors - e.g. when trying to move the cursor from (a) to (b) we can
            // *sometimes* - for no clear reason - end up at (c) instead.
            //
            //           +----------------+
            //           |(c)    (b)      |
            //           |                |
            //           |                |
            //           |                |
            // +---------+                |
            // |  (a)    |                |
            // +---------+----------------+
            //
            // setting the position a second time seems to fix this and moves the
            // cursor to the expected location (b) - for more details see
            // https://github.com/mikeclayton/FancyMouse/pull/3
            Cursor.Position = scaledLocation;
            Cursor.Position = scaledLocation;
        }

        this.Hide();

        if (this.Thumbnail.Image != null)
        {
            var tmp = this.Thumbnail.Image;
            this.Thumbnail.Image = null;
            tmp.Dispose();
        }
    }

    public void ShowThumbnail()
    {
        var logger = this.Options.Logger;

        logger.Debug(string.Join(
            "\r\n",
            "-----------",
            nameof(FancyMouseForm.ShowThumbnail),
            "-----------"));

        var screens = Screen.AllScreens;
        foreach (var i in Enumerable.Range(0, screens.Length))
        {
            var screen = screens[i];
            logger.Debug(string.Join(
                "\r\n",
                $"screen[{i}] = \"{screen.DeviceName}\"",
                $"    primary      = {screen.Primary}",
                $"    bounds       = {screen.Bounds}",
                $"    working area = {screen.WorkingArea}"));
        }

        var activatedLocation = Cursor.Position;
        var layoutConfig = new LayoutConfig(
            virtualScreen: SystemInformation.VirtualScreen,
            screenBounds: Screen.AllScreens.Select(screen => screen.Bounds),
            activatedLocation: activatedLocation,
            activatedScreen: Array.IndexOf(Screen.AllScreens, Screen.FromPoint(activatedLocation)),
            maximumFormSize: this.Options.MaximumThumbnailImageSize,
            formPadding: this.panel1.Padding,
            previewPadding: new Padding(0));
        logger.Debug(string.Join(
            "\r\n",
            $"Layout config",
            $"-------------",
            $"virtual screen     = {layoutConfig.VirtualScreen}",
            $"activated location = {layoutConfig.ActivatedLocation}",
            $"activated screen   = {layoutConfig.ActivatedScreen}",
            $"maximum form size  = {layoutConfig.MaximumFormSize}",
            $"form padding       = {layoutConfig.FormPadding}",
            $"preview padding    = {layoutConfig.PreviewPadding}"));

        var layoutCoords = PreviewImageComposer.CalculateCoords(layoutConfig);
        logger.Debug(string.Join(
            "\r\n",
            $"Layout coords",
            $"-------------",
            $"form bounds      = {layoutCoords.FormBounds}",
            $"preview bounds   = {layoutCoords.PreviewBounds}",
            $"activated screen = {layoutCoords.ActivatedScreen}"));

        // resize and position the form
        // note - do this in two steps rather than "this.Bounds = formBounds" as there
        // appears to be an issue in WinForms with dpi scaling even when using PerMonitorV2,
        // where the form scaling uses either the *primary* screen scaling or the *previous*
        // screen's scaling when the form is moved to a different screen. i've got no idea
        // *why*, but the exact sequence of calls below seems to be a workaround...
        // see https://github.com/mikeclayton/FancyMouse/issues/2
        this.Location = layoutCoords.FormBounds.Location.ToPoint();
        _ = this.PointToScreen(Point.Empty);
        this.Size = layoutCoords.FormBounds.Size.ToSize();

        if (this.Thumbnail.Image is not null)
        {
            var tmp = this.Thumbnail.Image;
            this.Thumbnail.Image = null;
            tmp.Dispose();
        }

        // initialize the preview image
        var preview = new Bitmap(
            (int)layoutCoords.PreviewBounds.Width,
            (int)layoutCoords.PreviewBounds.Height,
            PixelFormat.Format32bppArgb);
        this.Thumbnail.Image = preview;

        using var previewGraphics = Graphics.FromImage(preview);

        // draw the preview background
        using var backgroundBrush = new LinearGradientBrush(
            new Point(0, 0),
            new Point(preview.Width, preview.Height),
            Color.FromArgb(13, 87, 210),
            Color.FromArgb(3, 68, 192));
        previewGraphics.FillRectangle(backgroundBrush, layoutCoords.PreviewBounds.ToRectangle());

        var previewHdc = HDC.Null;
        var desktopHwnd = HWND.Null;
        var desktopHdc = HDC.Null;
        try
        {
            desktopHwnd = User32.GetDesktopWindow();
            desktopHdc = User32.GetWindowDC(desktopHwnd);

            // we have to capture the screen where we're going to show the form first
            // as the form will obscure the screen as soon as it's visible
            var stopwatch = Stopwatch.StartNew();
            previewHdc = new HDC(previewGraphics.GetHdc());
            PreviewImageComposer.CopyFromScreen(
                desktopHdc,
                previewHdc,
                layoutConfig.ScreenBounds.Where((_, idx) => idx == layoutConfig.ActivatedScreen).ToList(),
                layoutCoords.ScreenBounds.Where((_, idx) => idx == layoutConfig.ActivatedScreen).ToList());
            previewGraphics.ReleaseHdc(previewHdc.Value);
            previewHdc = HDC.Null;
            stopwatch.Stop();

            // show the placeholder image if it looks like it might take a while to capture
            // the remaining screenshot images
            if (stopwatch.ElapsedMilliseconds > 150)
            {
                var activatedArea = layoutConfig.ScreenBounds[layoutConfig.ActivatedScreen].Area;
                var totalArea = layoutConfig.ScreenBounds.Sum(screen => screen.Area);
                if ((activatedArea / totalArea) < 0.5M)
                {
                    var brush = Brushes.Black;
                    var bounds = layoutCoords.ScreenBounds
                        .Where((_, idx) => idx != layoutConfig.ActivatedScreen)
                        .Select(screen => screen.ToRectangle())
                        .ToArray();
                    if (bounds.Any())
                    {
                        previewGraphics.FillRectangles(brush, bounds);
                    }

                    this.Show();
                    this.Thumbnail.Refresh();
                }
            }

            // draw the remaining screen captures on the preview image
            previewHdc = new HDC(previewGraphics.GetHdc());
            PreviewImageComposer.CopyFromScreen(
                desktopHdc,
                previewHdc,
                layoutConfig.ScreenBounds.Where((_, idx) => idx != layoutConfig.ActivatedScreen).ToList(),
                layoutCoords.ScreenBounds.Where((_, idx) => idx != layoutConfig.ActivatedScreen).ToList());
            previewGraphics.ReleaseHdc(previewHdc.Value);
            previewHdc = HDC.Null;
            this.Thumbnail.Refresh();
        }
        finally
        {
            if (!desktopHwnd.IsNull && !desktopHdc.IsNull)
            {
                _ = User32.ReleaseDC(desktopHwnd, desktopHdc);
            }

            if (!previewHdc.IsNull)
            {
                previewGraphics.ReleaseHdc(previewHdc.Value);
            }
        }

        if (!this.Visible)
        {
            this.Show();
        }

        // we have to activate the form to make sure the deactivate event fires
        this.Activate();
    }

    // Sends an input simulating an absolute mouse move to the new location.
    private void SimulateMouseMovementEvent(Point location)
    {
        var mouseMoveInput = new NativeMethods.NativeMethods.INPUT
        {
            type = NativeMethods.NativeMethods.INPUTTYPE.INPUT_MOUSE,
            data = new NativeMethods.NativeMethods.InputUnion
            {
                mi = new NativeMethods.NativeMethods.MOUSEINPUT
                {
                    dx = NativeMethods.NativeMethods.CalculateAbsoluteCoordinateX(location.X),
                    dy = NativeMethods.NativeMethods.CalculateAbsoluteCoordinateY(location.Y),
                    mouseData = 0,
                    dwFlags = (uint)NativeMethods.NativeMethods.MOUSE_INPUT_FLAGS.MOUSEEVENTF_MOVE
                        | (uint)NativeMethods.NativeMethods.MOUSE_INPUT_FLAGS.MOUSEEVENTF_ABSOLUTE,
                    time = 0,
                    dwExtraInfo = 0,
                },
            },
        };
        var inputs = new NativeMethods.NativeMethods.INPUT[] { mouseMoveInput };
        _ = NativeMethods.NativeMethods.SendInput(1, inputs, NativeMethods.NativeMethods.INPUT.Size);
    }
}
