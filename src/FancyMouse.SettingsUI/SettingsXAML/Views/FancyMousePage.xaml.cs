using FancyMouse.Settings.V2;
using FancyMouse.SettingsUI.Helpers;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace FancyMouse.SettingsUI.Views;

public sealed partial class FancyMousePage : Page
{
    private string appSettingsPath = string.Empty;
    private AppConfig? loadedConfig;

    public FancyMousePage()
    {
        this.InitializeComponent();
    }

    internal void Initialize(string appSettingsPath)
    {
        this.appSettingsPath = appSettingsPath;
        this.LoadConfig();
    }

    private void LoadConfig()
    {
        var (config, settingsConfig) = SettingsFileHelper.Load(this.appSettingsPath);
        this.loadedConfig = config;
        this.fancyMousePane.SetConfig(settingsConfig);
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        // always save the "custom" style settings even if the active style is "compact"
        // or "Bezelled". this is so the custom settings can be restored if the style is
        // changed back to "custom"
        var viewModel = this.fancyMousePane.ViewModel!;
        var previewStyle = viewModel.ToPreviewStyle(extraColors: []); // use ToPreviewStyle to preserve custom style settings
        var hotkey = viewModel.FancyMouseActivationShortcut?.ToString();
        var previewType = viewModel.FancyMousePreviewType;
        SettingsFileHelper.Save(this.appSettingsPath, this.loadedConfig!, previewStyle, hotkey, previewType);

        this.LoadConfig();

        this.StatusText.Text = $"Saved to {this.appSettingsPath}";
    }
}
