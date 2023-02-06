using FancyMouse.Internal;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace FancyMouse.UI;

internal partial class FancyMouseForm : Form
{

    #region Constructors

    public FancyMouseForm(FancyMouseDialogOptions options)
    {
        this.Options = options ?? throw new ArgumentNullException(nameof(options));
        this.InitializeComponent();
    }

    #endregion

    #region Properties

    private FancyMouseDialogOptions Options
    {
        get;
    }

    #endregion

    #region Form Events

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

        this.Options.Logger.Debug("-----------");
        this.Options.Logger.Debug(nameof(FancyMouseForm.Thumbnail_Click));
        this.Options.Logger.Debug("-----------");

        var mouseEventArgs = (MouseEventArgs)e;
        this.Options.Logger.Debug($"mouse event args = ");
        this.Options.Logger.Debug($"    button   = {mouseEventArgs.Button} ");
        this.Options.Logger.Debug($"    location = {mouseEventArgs.Location} ");

        if (mouseEventArgs.Button == MouseButtons.Left)
        {
            if (Control.ModifierKeys == Keys.Control)
            {
                // ctrl click - show settings dialog
                var options = new FancyMouseSettings();
                options.Location = LayoutHelper.CenterObject(
                    obj: options.Size,
                    origin: LayoutHelper.GetMidpoint(this.Bounds)
                );
                options.ShowDialog();
            }
            else
            {

                // plain click - move mouse pointer
                var desktopBounds = LayoutHelper.CombineRegions(
                    Screen.AllScreens.Select(
                        screen => screen.Bounds
                    ).ToList()
                );
                this.Options.Logger.Debug(
                    $"desktop bounds  = {desktopBounds}"
                );

                var mouseEvent = (MouseEventArgs)e;

                var cursorPosition = LayoutHelper.ScaleLocation(
                    originalBounds: this.Thumbnail.Bounds,
                    originalLocation: new Point(mouseEvent.X, mouseEvent.Y),
                    scaledBounds: desktopBounds
                );
                this.Options.Logger.Debug(
                    $"cursor position = {cursorPosition}"
                );

                //MessageBox.Show(
                //    $"screen = {this.Screenshot!.Size}\r\n" +
                //    $"preview = {this.pbxPreview.Size}\r\n" +
                //    $"scale = {scale}\r\n" +
                //    $"click = {mouseEventArgs.Location}\r\n" +
                //    $"position = {cursorPosition}",
                //    "FancyMouse - Debug"
                //);

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
                Cursor.Position = cursorPosition;
                Cursor.Position = cursorPosition;

            }
        }

        this.Hide();

    }

    #endregion

    #region Form Management

    public void ShowPreview()
    {

        this.Options.Logger.Debug("-----------");
        this.Options.Logger.Debug(nameof(FancyMouseForm.ShowPreview));
        this.Options.Logger.Debug("-----------");

        if (this.Thumbnail.Image != null)
        {
            var tmp = this.Thumbnail.Image;
            this.Thumbnail.Image = null;
            tmp.Dispose();
        }

        var screens = Screen.AllScreens;
        foreach (var i in Enumerable.Range(0, screens.Length - 1))
        {
            var screen = screens[i];
            this.Options.Logger.Debug($"screen[{i}] = \"{screen.DeviceName}\"");
            this.Options.Logger.Debug($"    primary      = {screen.Primary}");
            this.Options.Logger.Debug($"    bounds       = {screen.Bounds}");
            this.Options.Logger.Debug($"    working area = {screen.WorkingArea}");
        }

        var desktopBounds = LayoutHelper.CombineRegions(
            screens.Select(screen => screen.Bounds).ToList()
        );
        this.Options.Logger.Debug(
            $"desktop bounds  = {desktopBounds}"
        );

        var cursorPosition = Cursor.Position;
        this.Options.Logger.Debug(
            $"cursor position = {cursorPosition}"
        );

        var previewImagePadding = new Size(
            panel1.Padding.Left + panel1.Padding.Right,
            panel1.Padding.Top + panel1.Padding.Bottom
        );
        this.Options.Logger.Debug(
            $"image padding   = {previewImagePadding}"
        );

        var formBounds = LayoutHelper.GetPreviewFormBounds(
            desktopBounds: desktopBounds,
            cursorPosition: cursorPosition,
            currentMonitorBounds: Screen.FromPoint(cursorPosition).Bounds,
            maximumPreviewImageSize: this.Options.MaximumPreviewImageSize,
            thumbnailImagePadding: previewImagePadding
        );
        this.Options.Logger.Debug(
            $"form bounds     = {formBounds}"
        );

        // take a screenshot of the entire desktop
        // see https://learn.microsoft.com/en-gb/windows/win32/gdi/the-virtual-screen
        using var screenshot = new Bitmap(desktopBounds.Width, desktopBounds.Height, PixelFormat.Format32bppArgb);
        using (var graphics = Graphics.FromImage(screenshot))
        {
            // note - it *might* be faster to capture each monitor individually and assemble them into
            // a single image ourselves as we *may* not have to transfer all of the blank pixels
            // that are outside the desktop bounds - e.g. the *** in the ascii art below
            //
            // +----------------+********
            // |                |********
            // |       1        +-------+
            // |                |       |
            // +----------------+   0   |
            // *****************|       |
            // *****************+-------+
            //
            // for very irregular monitor layouts this *might* be a big percentage of the rectangle
            // containing the desktop bounds.
            //
            // then again, it might not make much difference at all - we'd need to do some perf tests
            graphics.CopyFromScreen(desktopBounds.Left, desktopBounds.Top, 0, 0, desktopBounds.Size);
        }

        // scale the screenshot to fit the preview image. not *strictly* necessary as the
        // preview image box is set to "SizeMode = StretchImage" at design time, *but* we
        // use less memory holding a smaller image in memory. the trade-off is our memory
        // usage spikes a little bit higher while we generate the thumbnail.
        this.Thumbnail.SizeMode = PictureBoxSizeMode.Normal;
        var preview = FancyMouseForm.ResizeImage(
            screenshot,
            formBounds.Size - previewImagePadding
        );

        // resize and position the form
        // note - do this in two steps rather than "this.Bounds = formBounds" as there
        // appears to be an issue in WinForms with dpi scaling even when using PerMonitorV2,
        // where the form scaling uses either the *primary* screen scaling or the *previous*
        // screen's scaling when the form is moved to a different screen. i've got no idea
        // *why*, but the exact sequence of calls below seems to be a workaround...
        // see https://github.com/mikeclayton/FancyMouse/issues/2
        this.Location = formBounds.Location;
        _ = this.PointToScreen(Point.Empty);
        this.Size = formBounds.Size;

        // update the preview image
        this.Thumbnail.Image = preview;

        this.Show();

        // we have to activate the form to make sure the deactivate event fires
        this.Activate();

    }

    private static Bitmap ResizeImage(Image image, Size size)
    {
        return FancyMouseForm.ResizeImage(image, size.Width, size.Height);
    }

    /// <summary>
    /// Resize the image to the specified width and height.
    /// </summary>
    /// <param name="image">The image to resize.</param>
    /// <param name="width">The width to resize to.</param>
    /// <param name="height">The height to resize to.</param>
    /// <returns>The resized image.</returns>
    /// <remarks>
    /// See https://stackoverflow.com/questions/1922040/how-to-resize-an-image-c-sharp/24199315#24199315
    /// </remarks>
    private static Bitmap ResizeImage(Image image, int width, int height)
    {
        var destRect = new Rectangle(0, 0, width, height);
        var destImage = new Bitmap(width, height);
        destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);
        using (var graphics = Graphics.FromImage(destImage))
        {
            //// high quality / slow
            //graphics.CompositingMode = CompositingMode.SourceCopy;
            //graphics.CompositingQuality = CompositingQuality.HighQuality;
            //graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            //graphics.SmoothingMode = SmoothingMode.HighQuality;
            //graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
            // low quality / fast
            //graphics.CompositingMode = CompositingMode.SourceCopy;
            graphics.CompositingQuality = CompositingQuality.HighSpeed;
            graphics.InterpolationMode = InterpolationMode.Low;
            graphics.SmoothingMode = SmoothingMode.HighSpeed;
            graphics.PixelOffsetMode = PixelOffsetMode.HighSpeed;
            using (var wrapMode = new ImageAttributes())
            {
                wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
            }
        }
        return destImage;
    }

    #endregion

}
