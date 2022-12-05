using FancyMouse.Extensions;
using FancyMouse.Lib;
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

    public FancyMouseDialogOptions Options
    {
        get;
    }

    private Size DesktopSize
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
            this.DesktopSize = Size.Empty;
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
                options.Location = this.GetCenteredChildLocation(options);
                options.ShowDialog();
            }
            else
            {
                // plain click - move mouse pointer
                var scale = (double)pbxPreview.Width / this.DesktopSize.Width;
                var mouseEvent = (MouseEventArgs)e;
                var cursorPosition = new Point(
                    (int)(mouseEvent.X / scale),
                    (int)(mouseEvent.Y / scale)
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

    public void ShowPreview(
        Bitmap screenshot
    )
    {

        if (screenshot == null)
        {
            throw new ArgumentNullException(nameof(screenshot));
        }

        // dispose the existing image if there is one
        if (pbxPreview.Image != null)
        {
            pbxPreview.Image.Dispose();
            pbxPreview.Image = null;
            this.DesktopSize = Size.Empty;
        }

        // update the image
        pbxPreview.Image = screenshot;
        this.DesktopSize = screenshot.Size;

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