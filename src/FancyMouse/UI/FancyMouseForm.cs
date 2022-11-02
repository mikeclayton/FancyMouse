using FancyMouse.Extensions;
using FancyMouse.UI;

namespace FancyMouse;

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

    public Bitmap? Screenshot
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
            this.Hide();
        }
    }

    private void FancyMouseForm_Deactivate(object sender, EventArgs e)
    {
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
                var options = new FancyMouseOptions();
                options.Location = this.GetCenteredChildLocation(options);
                options.ShowDialog();
            }
            else
            {
                // plain click - move mouse pointer
                var scale = (double)panel1.Width / this.Screenshot.Width;
                var mouseEvent = (MouseEventArgs)e;
                var cursorPosition = new Point(
                    (int)(mouseEvent.X / scale),
                    (int)(mouseEvent.Y / scale)
                );
                Cursor.Position = cursorPosition;
            }
        }
        this.Hide();
    }

    #endregion

    #region Helpers

    private static int ClipValue(int min, int value, int max)
    {
        return Math.Max(Math.Min(value, max), min);
    }

    private static Point ClipLocation(Rectangle bounds, Point location, Size size)
    {
        return new Point(
            FancyMouseForm.ClipValue(bounds.X, location.X, bounds.Right - size.Width),
            FancyMouseForm.ClipValue(bounds.Y, location.Y, bounds.Bottom - size.Height)
        );
    }

    /// <summary>
    /// Return where to position an object with a given size so it is centered on the given point.
    /// </summary>
    /// <param name="size"></param>
    /// <param name="location"></param>
    /// <returns></returns>
    private static Point Center(Size size, Point center)
    {
        return new Point(
            (int)(center.X - (float)size.Width / 2),
            (int)(center.Y - (float)size.Height / 2)
        );
    }

    /// <summary>
    /// Return where to position a scaled object so the original location overlaps the 
    /// same point on the orignal sized object.
    /// </summary>
    /// <param name="size"></param>
    /// <param name="location"></param>
    /// <returns></returns>
    private static Point AlignScaled(Size originalSize, Point originalLocation, Size scaledSize)
    {
        return new Point(
                (int)(originalLocation.X - (float)scaledSize.Width * originalLocation.X / originalSize.Width),
                (int)(originalLocation.Y - (float)scaledSize.Height * originalLocation.Y / originalSize.Height)
        );
    }

    #endregion

    #region Form Management

    public void InitForm()
    {
        this.FormBorderStyle = FormBorderStyle.None;
        this.TopMost = true;
    }

    public void InitPreview()
    {

        // dispose the previous image if there is one
        if (pbxPreview.Image != null)
        {
            var previous = pbxPreview.Image;
            pbxPreview.Image = null;
            previous.Dispose();
        }

        // update the image
        pbxPreview.Image = this.Screenshot;

        // resize the form
        var padding = new Size(
            panel1.Padding.Left + panel1.Padding.Right,
            panel1.Padding.Top + panel1.Padding.Bottom
        );
        var formSize = this.Screenshot.Size.ScaleToFit(
            this.Options.MaximumSize - padding
        ) + padding;

        this.Size = formSize;

    }

    public void PositionForm()
    {

        var cursorPosition = Cursor.Position;
        var screenBounds = Screen.FromPoint(cursorPosition).Bounds;

        // current mouse position is at the centre of the form
        // (this isn't quite right - it assumes the panel padding is symmetrical, which it doesn't *have* to be)
        var centered = FancyMouseForm.ClipLocation(
            bounds: screenBounds,
            location: FancyMouseForm.Center(this.Size, cursorPosition),
            size: this.Size
        );

        //// the preview image and the desktop are aligned at current mouse position
        //var desktopBounds = ScreenHelper.GetDesktopBounds();
        //var aligned = FancyMouseForm.ClipLocation(
        //    bounds: screenBounds,
        //    location: FancyMouseForm.AlignScaled(desktopBounds.Size, cursorPosition, pbxPreview.Size),
        //    size: this.Size
        //);

        this.Location = centered;

    }

    #endregion

}