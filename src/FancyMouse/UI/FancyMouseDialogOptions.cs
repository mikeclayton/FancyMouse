namespace FancyMouse.UI;

internal sealed class FancyMouseDialogOptions
{

    #region Constructors

    public FancyMouseDialogOptions(
        Size maximumSize
    )
    {
        this.MaximumSize = maximumSize;
    }

    #endregion

    #region Properties

    public Size MaximumSize
    {
        get;
    }

    #endregion

}
