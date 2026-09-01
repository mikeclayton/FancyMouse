using WinUIEx;

namespace FancyMouse.SettingsUI;

public sealed partial class MainWindow : WindowEx
{
    public MainWindow(string appSettingsPath)
    {
        this.InitializeComponent();
        this.fancyMousePage.Initialize(appSettingsPath);
    }
}
