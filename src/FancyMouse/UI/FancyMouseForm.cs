using FancyMouse.Internal;
using Microsoft.Extensions.Logging;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;

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
        if (pbxPreview.Image != null)
        {
            pbxPreview.Image.Dispose();
            pbxPreview.Image = null;
            this.DesktopBounds = Rectangle.Empty;
        }

        this.Hide();

    }

    private void pbxPreview_Click(object sender, EventArgs e)
    {

        this.Options.Logger.LogDebug("-----------");
        this.Options.Logger.LogDebug(nameof(FancyMouseForm.pbxPreview_Click));
        this.Options.Logger.LogDebug("-----------");

        var mouseEventArgs = (MouseEventArgs)e;
        this.Options.Logger.LogDebug($"mouse event args = ");
        this.Options.Logger.LogDebug($"    button   = {mouseEventArgs.Button} ");
        this.Options.Logger.LogDebug($"    location = {mouseEventArgs.Location} ");

        if (mouseEventArgs.Button == MouseButtons.Left)
        {
            if (Control.ModifierKeys == Keys.Control)
            {
                // ctrl click - show settings dialog
                var options = new FancyMouseSettings();
                options.Location = LayoutHelper.Center(
                    obj: options.Size,
                    midpoint: LayoutHelper.Midpoint(this.Bounds)
                );
                options.ShowDialog();
            }
            else
            {

                // plain click - move mouse pointer
                var desktopBounds = LayoutHelper.Combine(
                    Screen.AllScreens.Select(
                        screen => screen.Bounds
                    )
                );
                this.Options.Logger.LogDebug(
                    $"desktop bounds  = {desktopBounds}"
                );

                var mouseEvent = (MouseEventArgs)e;

                var cursorPosition = LayoutHelper.MapLocation(
                    originalBounds: pbxPreview.Bounds,
                    originalLocation: new Point(mouseEvent.X, mouseEvent.Y),
                    scaledBounds: desktopBounds
                );
                this.Options.Logger.LogDebug(
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

                //This needs to be set twice sometimes because of a bug in Windows, at least on my environment. Otherwise the cursor will intermittently move to the edge of the main screen instead of other screens.
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

        this.Options.Logger.LogDebug("-----------");
        this.Options.Logger.LogDebug(nameof(FancyMouseForm.ShowPreview));
        this.Options.Logger.LogDebug("-----------");

        if (pbxPreview.Image != null)
        {
            var tmp = pbxPreview.Image;
            pbxPreview.Image = null;
            tmp.Dispose();
        }

        var screens = Screen.AllScreens;
        foreach (var i in Enumerable.Range(0, screens.Length - 1))
        {
            var screen = screens[i];
            this.Options.Logger.LogDebug($"screen[{i}] = \"{screen.DeviceName}\"");
            this.Options.Logger.LogDebug($"    primary      = {screen.Primary}");
            this.Options.Logger.LogDebug($"    bounds       = {screen.Bounds}");
            this.Options.Logger.LogDebug($"    working area = {screen.WorkingArea}");
        }

        var desktopBounds = LayoutHelper.Combine(
            screens.Select(screen => screen.Bounds)
        );
        this.Options.Logger.LogDebug(
            $"desktop bounds  = {desktopBounds}"
        );

        var cursorPosition = Cursor.Position;
        this.Options.Logger.LogDebug(
            $"cursor position = {cursorPosition}"
        );

        var previewImagePadding = new Size(
            panel1.Padding.Left + panel1.Padding.Right,
            panel1.Padding.Top + panel1.Padding.Bottom
        );
        this.Options.Logger.LogDebug(
            $"image padding   = {previewImagePadding}"
        );

        var formBounds = LayoutHelper.GetPreviewFormBounds(
            desktopBounds: desktopBounds,
            cursorPosition: cursorPosition,
            currentMonitorBounds: Screen.FromPoint(cursorPosition).Bounds,
            maximumPreviewImageSize: this.Options.MaximumPreviewImageSize,
            previewImagePadding: previewImagePadding
        );
        this.Options.Logger.LogDebug(
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
        pbxPreview.SizeMode = PictureBoxSizeMode.Normal;
        var preview = FancyMouseForm.ResizeImage(
            screenshot,
            formBounds.Size - previewImagePadding
        );

        // resize and position the form, and update the preview image
        this.Bounds = formBounds;
        pbxPreview.Image = preview;

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
