using FancyMouse.Common.Capture;

using Microsoft.UI.Xaml;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.
namespace FancyMouse.WinUI3.UI;

/// <summary>
/// An empty window that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class PreviewWindow : Window
{
    public PreviewWindow(NLog.ILogger logger)
    {
        this.Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.InitializeComponent();
        this.InitializeWindow();
    }

    private NLog.ILogger Logger
    {
        get;
    }

    /// <summary>
    /// Governs every screenshot capture started by the current activation's
    /// <see cref="ScreenshotCapturePipeline"/> - linked to the <see cref="CancellationToken"/>
    /// <see cref="ShowPreviewAsync"/> was called with (see its remarks for what that guards
    /// against), and *also* cancelled whenever the preview is cleared for any other reason (see
    /// <see cref="ClearPreview"/> - e.g. the user dismissed the window while backfill captures
    /// were still running), so outstanding captures for a no-longer-relevant activation don't
    /// keep doing GDI work in the background either way.
    /// </summary>
    private CancellationTokenSource? activationCancellation;

    /// <summary>
    /// Maximum time to wait for every screen's capture to finish before showing the window
    /// anyway - long enough that a typical (fast, local) activation shows fully populated with
    /// no visible placeholder-then-backfill repainting, short enough that one slow screen (e.g.
    /// a future remote capture provider) can't make the window feel unresponsive. Matches the
    /// grace period the legacy WinForms version used.
    /// </summary>
    private static readonly TimeSpan ScreenshotGracePeriod = TimeSpan.FromMilliseconds(250);
}
