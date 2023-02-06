using NLog;

namespace FancyMouse.UI;

internal sealed class FancyMouseDialogOptions
{

    #region Constructors

    public FancyMouseDialogOptions(
        ILogger logger,
        Size maximumThumbnailSize
    )
    {
        this.Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.MaximumThumbnailImageSize = maximumThumbnailSize;
    }

    #endregion

    #region Properties

    public ILogger Logger
    {
        get;
    }

    public Size MaximumThumbnailImageSize
    {
        get;
    }

    #endregion

}
