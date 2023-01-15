namespace FancyMouse.UI;

internal sealed class FancyMouseDialogOptions
{

    #region Constructors

    public FancyMouseDialogOptions(
        Size maximumSize
    )
    {
        this.MaximumPreviewImageSize = maximumSize;
    }

    #endregion

    #region Properties

    public Size MaximumPreviewImageSize
    {
        get;
    }

    #endregion

}
