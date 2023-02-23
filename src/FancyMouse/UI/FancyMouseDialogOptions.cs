using NLog;

namespace FancyMouse.UI;

internal sealed class FancyMouseDialogOptions
{
    public FancyMouseDialogOptions(
        ILogger logger,
        Size maximumThumbnailSize)
    {
        this.Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.MaximumThumbnailImageSize = maximumThumbnailSize;
    }

    public ILogger Logger
    {
        get;
    }

    public Size MaximumThumbnailImageSize
    {
        get;
    }
}
