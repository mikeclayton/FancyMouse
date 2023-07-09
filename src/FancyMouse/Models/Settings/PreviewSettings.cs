using FancyMouse.Models.Drawing;
using FancyMouse.Models.Layout;

namespace FancyMouse.Models.Settings;

public class PreviewSettings
{
    public sealed class Builder
    {
        public SizeInfo? Size
        {
            get;
            set;
        }

        public BoxStyle? PreviewStyle
        {
            get;
            set;
        }

        public BoxStyle? ScreenshotStyle
        {
            get;
            set;
        }

        public PreviewSettings Build()
        {
            return new PreviewSettings(
                size: this.Size ?? throw new InvalidOperationException($"{nameof(this.Size)} must be initialized before calling {nameof(this.Build)}."),
                previewStyle: this.PreviewStyle ?? throw new InvalidOperationException($"{nameof(this.PreviewStyle)} must be initialized before calling {nameof(this.Build)}."),
                screenshotStyle: this.ScreenshotStyle ?? throw new InvalidOperationException($"{nameof(this.ScreenshotStyle)} must be initialized before calling {nameof(this.Build)}."));
        }
    }

    public PreviewSettings(
        SizeInfo size,
        BoxStyle previewStyle,
        BoxStyle screenshotStyle)
    {
        this.Size = size ?? throw new ArgumentNullException(nameof(size));
        this.PreviewStyle = previewStyle ?? throw new ArgumentNullException(nameof(previewStyle));
        this.ScreenshotStyle = screenshotStyle ?? throw new ArgumentNullException(nameof(screenshotStyle));
    }

    /// <summary>
    /// Gets the maximum allowed size of the preview image.
    /// </summary>
    public SizeInfo Size
    {
        get;
    }

    public BoxStyle PreviewStyle
    {
        get;
    }

    public BoxStyle ScreenshotStyle
    {
        get;
    }
}
