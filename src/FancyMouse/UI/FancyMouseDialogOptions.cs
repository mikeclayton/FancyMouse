using FancyMouse.Models.Drawing;
using NLog;

namespace FancyMouse.UI;

internal sealed class FancyMouseDialogOptions
{
    public FancyMouseDialogOptions(
        ILogger logger,
        SizeInfo maximumThumbnailSize)
    {
        this.Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.MaximumThumbnailImageSize = maximumThumbnailSize;
    }

    public ILogger Logger
    {
        get;
    }

    public SizeInfo MaximumThumbnailImageSize
    {
        get;
    }
}
