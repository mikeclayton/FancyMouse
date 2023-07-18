using NLog;

namespace FancyMouse.UI;

internal sealed class FancyMouseDialogOptions
{
    public FancyMouseDialogOptions(
        ILogger logger)
    {
        this.Logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public ILogger Logger
    {
        get;
    }
}
