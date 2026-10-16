using FancyMouse.SettingsUI.Helpers;

using Microsoft.UI.Xaml.Controls;

namespace FancyMouse.SettingsUI.Views;

public sealed partial class FancyMousePage : Page
{
    public FancyMousePage()
    {
        this.InitializeComponent();
    }

    internal void Initialize(string appSettingsPath)
    {
        // there's no Save button - like PowerToys' MouseUtilsViewModel, FancyMouseViewModel
        // saves the settings file itself whenever a property changes (see its
        // NotifyFancyMousePropertyChanged), using the SettingsUtils it's given here.
        var settingsUtils = new SettingsUtils(appSettingsPath);
        var settingsConfig = SettingsFileHelper.Load(appSettingsPath);
        this.fancyMousePane.SetConfig(settingsUtils, settingsConfig);
    }
}
