using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace FancyMouse.SettingsUI.ViewModels;

/// <summary>
/// Local equivalent of PowerToys' <c>PageViewModelBase</c> class.
/// Not strictly needed for this project, but kept in order to
/// maintain architectural parity with PowerToys.
/// </summary>
public abstract class PageViewModelBase : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
