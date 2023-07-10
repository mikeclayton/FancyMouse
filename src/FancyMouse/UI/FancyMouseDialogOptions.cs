using FancyMouse.Models.Styles;
using NLog;

namespace FancyMouse.UI;

internal sealed class FancyMouseDialogOptions
{
    public FancyMouseDialogOptions(
        ILogger logger,
        PreviewStyle previewStyle)
    {
        this.Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.PreviewStyle = previewStyle ?? throw new ArgumentNullException(nameof(previewStyle));
    }

    public ILogger Logger
    {
        get;
    }

    public PreviewStyle PreviewStyle
    {
        get;
    }
}
