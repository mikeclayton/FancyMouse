using FancyMouse.Models.Settings;
using NLog;

namespace FancyMouse.UI;

internal sealed class FancyMouseDialogOptions
{
    public FancyMouseDialogOptions(
        ILogger logger,
        PreviewSettings previewSettings)
    {
        this.Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.PreviewSettings = previewSettings ?? throw new ArgumentNullException(nameof(previewSettings));
    }

    public ILogger Logger
    {
        get;
    }

    public PreviewSettings PreviewSettings
    {
        get;
    }
}
