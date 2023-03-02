using FancyMouse.Display;
using FancyMouse.Helpers;

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

        logger.Debug("-----------");
        logger.Debug(nameof(FancyMouseForm.Thumbnail_Click));
        logger.Debug("-----------");

        var mouseEventArgs = (MouseEventArgs)e;
        logger.Debug($"mouse event args = ");
        logger.Debug($"    button   = {mouseEventArgs.Button} ");
        logger.Debug($"    location = {mouseEventArgs.Location} ");

        if (mouseEventArgs.Button == MouseButtons.Left)
        {
            if (Control.ModifierKeys == Keys.Control)
            {
                // ctrl click - show settings dialog
                var options = new FancyMouseSettings();
                options.Location = LayoutHelper.CenterObject(
                    obj: options.Size,
                    origin: LayoutHelper.GetMidpoint(this.Bounds));
                options.ShowDialog();
            }
            else
            {
                // plain click - move mouse pointer
                var desktopBounds = LayoutHelper.CombineRegions(
                    Screen.AllScreens.Select(
                        screen => screen.Bounds).ToList());
                logger.Debug(
                    $"desktop bounds  = {desktopBounds}");

                var mouseEvent = (MouseEventArgs)e;

                var scaledLocation = LayoutHelper.ScaleLocation(
                    originalBounds: this.Thumbnail.Bounds,
                    originalLocation: new Point(mouseEvent.X, mouseEvent.Y),
                    scaledBounds: desktopBounds);
                logger.Debug(
                    $"scaled location = {scaledLocation}");

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
        }

        this.Hide();
    }

    public void ShowThumbnail()
    {
        var logger = this.Options.Logger;

        logger.Debug("-----------");
        logger.Debug(nameof(FancyMouseForm.ShowThumbnail));
        logger.Debug("-----------");

        if (this.Thumbnail.Image != null)
        {
            var tmp = this.Thumbnail.Image;
            this.Thumbnail.Image = null;
            tmp.Dispose();
        }

        var screens = Screen.AllScreens;
        foreach (var i in Enumerable.Range(0, screens.Length))
        {
            var screen = screens[i];
            logger.Debug($"screen[{i}] = \"{screen.DeviceName}\"");
            logger.Debug($"    primary      = {screen.Primary}");
            logger.Debug($"    bounds       = {screen.Bounds}");
            logger.Debug($"    working area = {screen.WorkingArea}");
        }

        var desktopBounds = LayoutHelper.CombineRegions(
            screens.Select(screen => screen.Bounds).ToList());
        logger.Debug(
            $"desktop bounds  = {desktopBounds}");

        var activatedPosition = Cursor.Position;
        logger.Debug(
            $"activated position = {activatedPosition}");

        var previewImagePadding = new Size(
            panel1.Padding.Left + panel1.Padding.Right,
            panel1.Padding.Top + panel1.Padding.Bottom);
        logger.Debug(
            $"image padding   = {previewImagePadding}");

        var maxThumbnailSize = this.Options.MaximumThumbnailImageSize;
        var formBounds = LayoutHelper.GetPreviewFormBounds(
            desktopBounds: desktopBounds,
            activatedPosition: activatedPosition,
            activatedMonitorBounds: Screen.FromPoint(activatedPosition).Bounds,
            maximumThumbnailImageSize: maxThumbnailSize,
            thumbnailImagePadding: previewImagePadding);
        logger.Debug(
            $"form bounds     = {formBounds}");

        var screenCopyHelper = new NativeJigsawScreenCopyHelper();

        // capture the screen area under where the form will be displayed
        // because once we show the form it'll be visible on the screenshot
        // var formBackground = screenCopyHelper.CopyFromScreen(
        //    desktopBounds,
        //    screens.Select(s => s.Bounds),
        //    formBounds.Size - previewImagePadding);

        // var screenCopyHelper = new JigsawScreenCopyHelper();
        var preview = screenCopyHelper.CopyFromScreen(
            desktopBounds,
            screens.Select(s => s.Bounds),
            formBounds.Size - previewImagePadding);

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
}
