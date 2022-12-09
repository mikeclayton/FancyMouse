using FancyMouse.Helpers;

namespace FancyMouse.UI;

internal class FancyMouseDialog
{

    #region Constructors

    public FancyMouseDialog(FancyMouseDialogOptions options)
    {
        this.Form = new FancyMouseForm(
            options ?? throw new ArgumentNullException(nameof(options))
        );
    }

    #endregion

    #region Properties

    private FancyMouseForm Form
    {
        get;
    }

    #endregion

    public void Show()
    {

        var form = this.Form;

        form.Visible = false;

        form.ShowPreview();

    }

}
