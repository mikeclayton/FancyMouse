namespace FancyMouse.UI;

internal class FancyMouseDialog
{

    #region Constructors

    public FancyMouseDialog(FancyMouseDialogOptions options)
    {
        this.Options = options ?? throw new ArgumentNullException(nameof(options));
        this.Form = new FancyMouseForm(this.Options);
    }

    #endregion

    #region Properties

    private FancyMouseDialogOptions Options
    {
        get;
    }

    private FancyMouseForm Form
    {
        get;
    }

    #endregion

    public void Show()
    {

        var form = this.Form;

        form.Visible = false;
        form.Screenshot = Helpers.ScreenHelper.GetDesktopImage();

        form.InitForm();
        form.InitPreview();
        form.PositionForm();

        form.Show();

    }

}
