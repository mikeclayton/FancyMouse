using FancyMouse.Settings.Styles;
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

        // a snapshot of the local machine's screens - or, with --demo, a hard-coded multi-monitor
        // layout (see DemoScreens) for trying the editor out on a machine with fewer real screens
        var args = Environment.GetCommandLineArgs().Skip(1).ToList();
        var useDemoScreens = args.Any(arg => string.Equals(arg, "--demo", StringComparison.OrdinalIgnoreCase));
        var screens = useDemoScreens
            ? DemoScreens.Screens
            : DeviceHelper.GetDisplayInfo().Devices[0].Screens;

        // for the proof of concept the preview is always drawn in the built-in "Bezelled" style
        // (rather than the user's own settings) - edits are in-memory only, so every launch starts
        // from the automatic layout
        this.LayoutEditor.LoadScreens(screens, PreviewStyles.Bezelled);
        if (useDemoScreens)
        {
            this.Title += " (demo screens)";
        }
    }
}
