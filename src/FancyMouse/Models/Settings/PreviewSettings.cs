﻿using System.Text.Json.Serialization;

namespace FancyMouse.Models.Settings;

public class PreviewSettings
{
    public sealed class Builder
    {
        public CanvasSizeSettings? CanvasSize
        {
            get;
            set;
        }

        public CanvasStyleSettings? CanvasStyle
        {
            get;
            set;
        }

        public ScreenshotStyleSettings? ScreenshotStyle
        {
            get;
            set;
        }

        public PreviewSettings Build()
        {
            return new PreviewSettings(
                canvasSize: this.CanvasSize ?? throw new InvalidOperationException($"{nameof(this.CanvasSize)} must be initialized before calling {nameof(this.Build)}."),
                canvasStyle: this.CanvasStyle ?? throw new InvalidOperationException($"{nameof(this.CanvasStyle)} must be initialized before calling {nameof(this.Build)}."),
                screenshotStyle: this.ScreenshotStyle ?? throw new InvalidOperationException($"{nameof(this.ScreenshotStyle)} must be initialized before calling {nameof(this.Build)}."));
        }
    }

    public PreviewSettings(
        CanvasSizeSettings canvasSize,
        CanvasStyleSettings canvasStyle,
        ScreenshotStyleSettings screenshotStyle)
    {
        this.CanvasSize = canvasSize ?? throw new ArgumentNullException(nameof(canvasSize));
        this.CanvasStyle = canvasStyle ?? throw new ArgumentNullException(nameof(canvasStyle));
        this.ScreenshotStyle = screenshotStyle ?? throw new ArgumentNullException(nameof(screenshotStyle));
    }

    [JsonPropertyName("size")]
    public CanvasSizeSettings CanvasSize
    {
        get;
    }

    [JsonPropertyName("canvas")]
    public CanvasStyleSettings CanvasStyle
    {
        get;
    }

    [JsonPropertyName("screenshot")]
    public ScreenshotStyleSettings ScreenshotStyle
    {
        get;
    }
}
