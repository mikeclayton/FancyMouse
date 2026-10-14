using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace FancyMouse.SettingsUI.Converters;

/// <summary>
/// Small local stand-in for CommunityToolkit.WinUI.Converters' own
/// <c>DoubleToVisibilityConverter</c> / <c>DoubleToInvertedVisibilityConverter</c>
/// (used by PowerToys' own <c>ShortcutDialogContentControl.xaml</c>
/// for its <c>Keys.Count</c> bindings).
/// </summary>
public sealed partial class IntToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var isZero = value is int count && count == 0;
        var invert = string.Equals(parameter as string, "Invert", StringComparison.OrdinalIgnoreCase);
        var visible = invert ? isZero : !isZero;
        return visible ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}
