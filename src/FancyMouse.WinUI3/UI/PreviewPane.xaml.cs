using FancyMouse.Common.Blurring;
using FancyMouse.Models.Display;
using FancyMouse.Models.Layout;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace FancyMouse.WinUI3.UI;

/// <summary>
/// Encapsulates the preview pane's own content - the background rectangle and the
/// bezels/screenshots on top of it. Deliberately excludes the outer border, which is the
/// hosting window's responsibility (see <see cref="Common.Helpers.LayoutHelper.GetHostBoxStyle"/>).
/// The hosting window supplies the pre-computed <see cref="Layout"/> (this control doesn't
/// calculate its own size); the background image and each screen's bezel are rendered
/// internally from <see cref="Layout"/> as soon as it's set, and each screen starts out
/// showing a placeholder fill until the hosting window backfills its real screenshot via
/// <see cref="SetScreenshot"/> (screenshot capture needs the host's own capture pipeline,
/// which this control has no access to). This control also owns turning raw mouse/keyboard
/// input into navigation intent - see <see cref="NavigateTo"/>/<see cref="Cancel"/> - so the
/// host doesn't need its own copy of "which screen is that" or "which screen is next" logic.
/// </summary>
public sealed partial class PreviewPane : UserControl
{
    public static readonly DependencyProperty LayoutProperty = DependencyProperty.Register(
        nameof(PreviewPane.Layout),
        typeof(PreviewLayout),
        typeof(PreviewPane),
        new PropertyMetadata(null, PreviewPane.OnLayoutChanged));

    public static readonly DependencyProperty ActiveScreenProperty = DependencyProperty.Register(
        nameof(PreviewPane.ActiveScreen),
        typeof(ScreenInfo),
        typeof(PreviewPane),
        new PropertyMetadata(null));

    /// <summary>
    /// Generates and retains each screen's blurred stand-in placeholder, keyed by physical
    /// screen rather than layout position - see <see cref="ScreenshotBlurPipeline"/>. One long-lived
    /// instance for this control's whole lifetime; <see cref="ApplyLayout"/> tells it which
    /// screens currently exist (<see cref="ScreenshotBlurPipeline.SetActiveScreens"/>) once per activation.
    /// </summary>
    private readonly ScreenshotBlurPipeline blurPipeline = new();

    private List<ScreenSlot> screenSlots = new();

    /// <summary>
    /// How long <see cref="CrossfadeContent"/> takes to fade a screen's content in. Short enough
    /// not to read as a deliberate animation, but spreading the change across enough frames
    /// (roughly 6, at 60Hz) that no single frame carries more than a fraction of the total
    /// change - the goal is specifically to avoid a hard single-frame swap's sudden luminance
    /// change catching peripheral vision, not to make the fade itself noticeable.
    /// </summary>
    private static readonly TimeSpan ContentTransitionDuration = TimeSpan.FromMilliseconds(100);

    public PreviewPane()
    {
        this.InitializeComponent();
        this.PointerPressed += this.PreviewPane_PointerPressed;
        this.PreviewKeyDown += this.PreviewPane_PreviewKeyDown;
    }

    /// <summary>
    /// Gets or sets the mathematical model describing this pane's own size and the position/style of
    /// every device/screen bezel within it. Set by the hosting window - this control doesn't
    /// calculate its own size from anything else.
    /// </summary>
    public PreviewLayout? Layout
    {
        get => (PreviewLayout?)this.GetValue(PreviewPane.LayoutProperty);
        set => this.SetValue(PreviewPane.LayoutProperty, value);
    }

    /// <summary>
    /// Gets or sets the screen that keyboard navigation (Left/Right in particular) is relative
    /// to - set by the hosting window alongside <see cref="Layout"/>, normally to whichever
    /// screen was activated. This control never changes it itself: every navigation key acts
    /// immediately and the host is expected to close the preview afterwards (see
    /// <see cref="NavigateTo"/>), so there's no in-preview "browse mode" that would need this to
    /// track a moving selection.
    /// </summary>
    public ScreenInfo? ActiveScreen
    {
        get => (ScreenInfo?)this.GetValue(PreviewPane.ActiveScreenProperty);
        set => this.SetValue(PreviewPane.ActiveScreenProperty, value);
    }

    /// <summary>
    /// Raised when the pointer clicks a screen, or a keyboard shortcut that means the same
    /// thing (1-9, arrow keys relative to <see cref="ActiveScreen"/>, P for primary, Home/End)
    /// resolves to one. <see cref="NavigateToEventArgs.Location"/> is already resolved to the
    /// corresponding physical location on that screen's own display area.
    /// </summary>
    public event EventHandler<NavigateToEventArgs>? NavigateTo;

    /// <summary>
    /// Raised on a right-click or Escape - the host is expected to just close the preview
    /// without moving the pointer anywhere.
    /// </summary>
    public event EventHandler? Cancel;
}
