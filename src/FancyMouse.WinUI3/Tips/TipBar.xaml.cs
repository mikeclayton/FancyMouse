using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace FancyMouse.WinUI3.Tips;

/// <summary>
/// A visualization of whatever <see cref="Tip"/> is currently set on
/// <see cref="CurrentTip"/> - see <see cref="Tip"/> for the design this probes. Deliberately a
/// pure "selector" component: it renders whatever it's given and has no engine/timing logic of
/// its own.
/// </summary>
public sealed partial class TipBar : UserControl
{
    public static readonly DependencyProperty CurrentTipProperty = DependencyProperty.Register(
        nameof(TipBar.CurrentTip),
        typeof(Tip),
        typeof(TipBar),
        new PropertyMetadata(null, TipBar.OnCurrentTipChanged));

    public TipBar()
    {
        this.InitializeComponent();
    }

    /// <summary>
    /// Raised when the settings button (always visible, independent of <see cref="CurrentTip"/>
    /// - see TipBar.xaml's remarks) is clicked. This control has no idea what "settings" means
    /// or how to show them - it just signals the click and leaves acting on it to the host.
    /// </summary>
    public event EventHandler? SettingsRequested;

    /// <summary>
    /// Raised when the "fancyMouse" badge is clicked - same "pure selector" split as
    /// <see cref="SettingsRequested"/>: this control doesn't know what a repository URL is or how
    /// to open one, it just signals the click and leaves acting on it to the host.
    /// </summary>
    public event EventHandler? RepositoryRequested;

    public Tip? CurrentTip
    {
        get => (Tip?)this.GetValue(TipBar.CurrentTipProperty);
        set => this.SetValue(TipBar.CurrentTipProperty, value);
    }

    private void SettingsButton_Click(object sender, RoutedEventArgs e)
        => this.SettingsRequested?.Invoke(this, EventArgs.Empty);

    private void RepositoryButton_Click(object sender, RoutedEventArgs e)
        => this.RepositoryRequested?.Invoke(this, EventArgs.Empty);

    private static void OnCurrentTipChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        => ((TipBar)sender).ApplyCurrentTip((Tip?)e.NewValue);

    private void ApplyCurrentTip(Tip? tip)
    {
        if (tip is InfoTip infoTip)
        {
            this.InfoIcon.Text = infoTip.Icon ?? string.Empty;
            this.InfoMessage.Text = infoTip.Message;
            this.InfoPanel.Visibility = Visibility.Visible;
        }
        else
        {
            this.InfoPanel.Visibility = Visibility.Collapsed;
        }
    }
}
