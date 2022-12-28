using FancyMouse.Helpers;
using FancyMouse.Lib;

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

    private Rectangle ScreenshotBounds
    {
        get;
        set;
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

        var mouseEventArgs = (MouseEventArgs)e;

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
                var mouseEvent = (MouseEventArgs)e;
                var cursorPosition = LayoutHelper.ScaleLocation(
                    originalBounds: pbxPreview.Bounds,
                    originalLocation: new Point(mouseEvent.X, mouseEvent.Y),
                    scaledBounds: this.ScreenshotBounds
                );
                //MessageBox.Show(
                //    $"screen = {this.Screenshot!.Size}\r\n" +
                //    $"preview = {this.pbxPreview.Size}\r\n" +
                //    $"scale = {scale}\r\n" +
                //    $"click = {mouseEventArgs.Location}\r\n" +
                //    $"position = {cursorPosition}",
                //    "FancyMouse - Debug"
                //);
                Cursor.Position = cursorPosition;
            }
        }

        this.Hide();

    }

    #endregion

    #region Form Management

    public void ShowPreview()
    {

        // dispose the existing image if there is one
        if (pbxPreview.Image != null)
        {
            pbxPreview.Image.Dispose();
            pbxPreview.Image = null;
            this.ScreenshotBounds = Rectangle.Empty;
        }

        this.ScreenshotBounds = LayoutHelper.CombineBounds(
            Screen.AllScreens.Select(screen => screen.Bounds)
        );

        // update the image
        var screenshot = ScreenHelper.GetDesktopImage(this.ScreenshotBounds);
        pbxPreview.Image = screenshot;

        // resize the form
        var padding = new Size(
            panel1.Padding.Left + panel1.Padding.Right,
            panel1.Padding.Top + panel1.Padding.Bottom
        );
        var formSize = LayoutHelper.ScaleToFit(
            obj: screenshot.Size,
            bounds: this.Options.MaximumSize - padding
        ) + padding;

        // position the form
        var cursorPosition = Cursor.Position;
        var formBounds = LayoutHelper.GetPreviewBounds(
            Screen.FromPoint(cursorPosition).Bounds,
            cursorPosition,
            formSize
        );

        this.Bounds = formBounds;

        this.Show();

    }

    #endregion

}