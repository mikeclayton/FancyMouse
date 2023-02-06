using NLog;

namespace FancyMouse.UI;

internal sealed class FancyMouseDialogOptions
{

    #region Constructors

    public FancyMouseDialogOptions(
        ILogger logger,
        Size maximumSize
    )
    {
        this.Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.MaximumPreviewImageSize = maximumSize;
    }

    #endregion

    #region Properties

    public ILogger Logger
    {
        get;
    }

    public Size MaximumPreviewImageSize
    {
        get;
    }

    #endregion

}
