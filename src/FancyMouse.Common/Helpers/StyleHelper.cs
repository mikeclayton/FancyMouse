using FancyMouse.Models.Styles;

namespace FancyMouse.Common.Helpers;

public static class StyleHelper
{
    /// <summary>
    /// The equivalent of PowerToys' own <c>SettingsHelper.GetActivePreviewStyle</c> - derives the
    /// *visual* representation to render from a *settings* representation
    /// (<paramref name="previewStyle"/>/<paramref name="previewType"/>, e.g. FancyMouse's own
    /// <c>AppSettings.PreviewStyle</c>/<c>AppSettings.PreviewType</c>): copies
    /// <paramref name="previewStyle"/>'s canvas size onto whichever of
    /// <see cref="PreviewStyles.Compact"/>/<see cref="PreviewStyles.Bezelled"/> is selected, or returns
    /// <paramref name="previewStyle"/> itself unchanged for Custom. Never mutates
    /// <paramref name="previewStyle"/> - call this at the point of rendering, not when
    /// parsing/loading settings, so the settings representation it derives from is never itself
    /// overwritten by a preset's values.
    /// </summary>
    /// <remarks>
    /// The presets themselves are shared data, so they live in Models (<see cref="PreviewStyles"/>);
    /// choosing between them is behaviour, so it stays here. It only takes the shared
    /// <see cref="PreviewStyle"/>/<see cref="PreviewType"/> types, so each app's settings code
    /// resolves to those and calls this, without the settings code itself needing to depend on
    /// this assembly.
    /// </remarks>
    public static PreviewStyle GetActivePreviewStyle(PreviewStyle previewStyle, PreviewType previewType)
    {
        return previewType switch
        {
            PreviewType.Compact => PreviewStyles.Compact.WithCanvasSize(previewStyle.CanvasSize),
            PreviewType.Bezelled => PreviewStyles.Bezelled.WithCanvasSize(previewStyle.CanvasSize),
            PreviewType.Custom => previewStyle,
            _ => throw new InvalidOperationException($"Unhandled {nameof(PreviewType)} '{previewType}'"),
        };
    }
}
