using FancyMouse.Windows.Helpers;

using Microsoft.UI.Xaml;

using Windows.Graphics;

namespace FancyMouse.Editor;

public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        this.InitializeComponent();
        this.AppWindow.Resize(new SizeInt32(1200, 800));

        // a snapshot of the local machine's screens, each at its Display Settings position and
        // native size - or, with --demo, a hard-coded multi-monitor layout (see DemoScreens) for
        // trying the editor out on a machine with fewer real screens. edits are in-memory only,
        // so every launch starts from this layout
        var useDemoScreens = Environment.GetCommandLineArgs()
            .Skip(1)
            .Any(arg => string.Equals(arg, "--demo", StringComparison.OrdinalIgnoreCase));
        var screens = useDemoScreens
            ? DemoScreens.Screens
            : DeviceHelper.GetDisplayInfo().Devices[0].Screens;
        this.LayoutEditor.LoadScreens(screens);
        if (useDemoScreens)
        {
            this.Title += " (demo screens)";
        }

        this.LayoutEditor.SelectionChanged += this.LayoutEditor_SelectionChanged;
        this.UpdateCommandStates();
    }

    private void UpdateCommandStates()
    {
        // every command except "Reset all" acts on the selected screen
        var hasSelection = this.LayoutEditor.SelectedScreen is not null;
        this.BringToFrontButton.IsEnabled = hasSelection;
        this.BringForwardButton.IsEnabled = hasSelection;
        this.SendBackwardButton.IsEnabled = hasSelection;
        this.SendToBackButton.IsEnabled = hasSelection;
        this.ResetScreenButton.IsEnabled = hasSelection;
    }

    private void LayoutEditor_SelectionChanged(object? sender, EventArgs e)
        => this.UpdateCommandStates();

    private void BringToFrontButton_Click(object sender, RoutedEventArgs e)
        => this.LayoutEditor.BringToFront();

    private void BringForwardButton_Click(object sender, RoutedEventArgs e)
        => this.LayoutEditor.BringForward();

    private void SendBackwardButton_Click(object sender, RoutedEventArgs e)
        => this.LayoutEditor.SendBackward();

    private void SendToBackButton_Click(object sender, RoutedEventArgs e)
        => this.LayoutEditor.SendToBack();

    private void ResetScreenButton_Click(object sender, RoutedEventArgs e)
        => this.LayoutEditor.ResetSelectedScreen();

    private void ResetAllButton_Click(object sender, RoutedEventArgs e)
        => this.LayoutEditor.ResetAllScreens();
}
