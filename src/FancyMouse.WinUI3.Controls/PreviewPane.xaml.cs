using FancyMouse.Drawing.Blurring;
using FancyMouse.Models.Display;
using FancyMouse.Models.Layout;
using FancyMouse.Telemetry;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace FancyMouse.WinUI3.Controls;

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
    /// Backing field for <see cref="Telemetry"/> - null until the host sets it.
    /// </summary>
    private TelemetryContext? telemetry;

    /// <summary>
    /// Backing field for <see cref="BlurPipeline"/> - creation is deferred to allow
    /// <see cref="Telemetry"/> to be initialised first and passed to the blur pipeline.
    /// </summary>
    private BlurPipeline? blurPipeline;

    private List<ScreenSlot> screenSlots = [];

    /// <summary>
    /// The rasterization scale everything was last positioned for - see <see cref="ApplyScale"/>.
    /// </summary>
    private double appliedScale;

    /// <summary>
    /// The <see cref="XamlRoot"/> whose <see cref="XamlRoot.Changed"/> event this pane is
    /// listening to for scale changes, if any - see <see cref="PreviewPane_Loaded"/>.
    /// </summary>
    private XamlRoot? subscribedXamlRoot;

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
        this.Loaded += this.PreviewPane_Loaded;
        this.Unloaded += this.PreviewPane_Unloaded;
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
    /// Gets or sets the context this pane records its telemetry to - set by the hosting window,
    /// the same way it passes its logger to everything else. Required: a host that doesn't want
    /// telemetry passes a context it never starts, which records nothing and costs nothing.
    /// </summary>
    /// <remarks>
    /// A plain property rather than a constructor parameter because XAML creates this control,
    /// and rather than a dependency property because it's a service, not something to bind or
    /// animate. Must be set before the pane is first used - <see cref="BlurPipeline"/> captures
    /// it when it's created, so a later change would otherwise be silently ignored. The host
    /// owns the context, so the pane never disposes it.
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// Getting it before it's been set, or setting it after the pane has already been used.
    /// </exception>
    public TelemetryContext Telemetry
    {
        get => this.telemetry
            ?? throw new InvalidOperationException($"{nameof(this.Telemetry)} must be set before the {nameof(PreviewPane)} is used.");
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            if (this.blurPipeline is not null)
            {
                throw new InvalidOperationException($"{nameof(this.Telemetry)} must be set before the {nameof(PreviewPane)} is first used.");
            }

            this.telemetry = value;
        }
    }

    /// <summary>
    /// Gets the pipeline that generates the blurred screenshots shown while the latest
    /// screenshot is being captured, if a screen capture takes longer than the budget allowed
    /// for showing the form.
    /// </summary>
    private BlurPipeline BlurPipeline
        => this.blurPipeline ??= new BlurPipeline(this.Telemetry);

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
