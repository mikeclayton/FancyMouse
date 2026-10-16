using FancyMouse.Common.Capture;
using FancyMouse.Telemetry;
using FancyMouse.WinUI3.Internal.Helpers;

using Microsoft.UI.Xaml;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.
namespace FancyMouse.WinUI3.UI;

/// <summary>
/// An empty window that can be used on its own or navigated to within a Frame.
/// </summary>
internal sealed partial class PreviewWindow : Window
{
    internal PreviewWindow(NLog.ILogger logger, TelemetryContext telemetry, ConfigHelper configHelper)
    {
        this.Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.Telemetry = telemetry ?? throw new ArgumentNullException(nameof(telemetry));
        this.ConfigHelper = configHelper ?? throw new ArgumentNullException(nameof(configHelper));
        this.InitializeComponent();
        this.InitializePreviewPane();
        this.InitializeWindow();
    }

    private void InitializePreviewPane()
    {
        // PreviewPane is created by XAML, so it can't take this in its constructor - it has to
        // be set here, before the pane is first used (see PreviewPane.Telemetry)
        this.PreviewPane.Telemetry = this.Telemetry;
    }

    private NLog.ILogger Logger
    {
        get;
    }

    private TelemetryContext Telemetry
    {
        get;
    }

    private ConfigHelper ConfigHelper
    {
        get;
    }

    /// <summary>
    /// Used for cancelling an activation of the preview window, and all associated background tasks.
    /// </summary>
    private CancellationTokenSource? activationCancellation;

    /// <summary>
    /// Maximum time to wait for screen capture to complete before showing the window
    /// anyway - long enough that a typical (fast, local) activation shows fully populated with
    /// no visible placeholder-then-backfill repainting, short enough that one slow screen (e.g.
    /// a large screen or a future remote capture provider) can't make the window feel
    /// unresponsive.
    /// </summary>
    private static readonly TimeSpan ScreenshotGracePeriod = TimeSpan.FromMilliseconds(250);
}
