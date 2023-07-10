namespace FancyMouse.UI;

internal class FancyMouseDialog
{
    public FancyMouseDialog(FancyMouseDialogOptions options)
    {
        this.Form = new FancyMouseForm(
            options ?? throw new ArgumentNullException(nameof(options)));
    }

    private FancyMouseForm Form
    {
        get;
    }

    public void Show()
    {
        var form = this.Form;
        form.ShowPreview();

        // GC.Collect();
    }
}
