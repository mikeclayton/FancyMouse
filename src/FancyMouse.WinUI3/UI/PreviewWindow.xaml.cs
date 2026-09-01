using System.Diagnostics;

using FancyMouse.Common.Capture;
using FancyMouse.WinUI3.Internal.Helpers;
using FancyMouse.WinUI3.Tips;

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
        this.TipBarControl.SettingsRequested += this.TipBarControl_SettingsRequested;
        this.TipBarControl.RepositoryRequested += this.TipBarControl_RepositoryRequested;
    }

    private const string RepositoryUrl = "https://github.com/mikeclayton/fancymouse";

    private void TipBarControl_SettingsRequested(object? sender, EventArgs e)
    {
        try
        {
            SettingsLauncher.Launch();
        }
        catch (Exception ex)
        {
            this.Logger.Error(ex, "failed to launch the settings app");
        }
    }

    private void TipBarControl_RepositoryRequested(object? sender, EventArgs e)
    {
        try
        {
            Process.Start(new ProcessStartInfo(PreviewWindow.RepositoryUrl) { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            this.Logger.Error(ex, "failed to open the GitHub repository");
        }
    }

    private NLog.ILogger Logger
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

    /// <summary>
    /// Height of <see cref="TipBarControl"/>'s reserved strip (in pixels, *not* DIPs)
    /// beneath the preview pane. Set to 0 to hide the status bar. In the same
    /// physical-pixel units as the rest of the layout math
    /// (<see cref="Models.Styles.PreviewStyle.CanvasSize"/> etc.), *not* DIPs.
    /// </summary>
    private const decimal TipBarHeight = 40m;

    /// <summary>
    /// A list of tips that are shown on rotation in the TipBar.
    /// </summary>
    private static readonly IReadOnlyList<Tip> SampleTips =
    [
        new InfoTip("You can bind the activation keys to a spare mouse button for single-click activation.", "\U0001F4A1"),
    ];

    private int sampleTipIndex;
}
