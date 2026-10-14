namespace FancyMouse.Settings.V2;

/// <summary>
/// Mirrors PowerToys' own <c>MouseJump.Models.Settings.PreviewType</c> - which of the predefined
/// <see cref="FancyMouse.Common.Helpers.StyleHelper"/> styles (or the user's own custom one) is
/// active. Values and enum name match MouseJump's exactly, since the persisted "type" string is
/// just this enum's <see cref="object.ToString"/> - see <see cref="SettingsConverterV2.GetActivePreviewStyle"/>.
/// </summary>
public enum PreviewType
{
    Custom = 0,
    Compact = 1,
    Bezelled = 2,
}
