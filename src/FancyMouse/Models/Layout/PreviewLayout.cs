﻿using System.Collections.ObjectModel;
using FancyMouse.Models.Drawing;
using FancyMouse.Models.Screen;

namespace FancyMouse.Models.Layout;

public sealed class PreviewLayout
{
    public sealed class Builder
    {
        public Builder()
        {
            this.Screens = new();
            this.ScreenshotBounds = new();
        }

        public RectangleInfo? VirtualScreen
        {
            get;
            set;
        }

        public List<ScreenInfo> Screens
        {
            get;
            set;
        }

        public ScreenInfo? ActivatedScreen
        {
            get;
            set;
        }

        public RectangleInfo? FormBounds
        {
            get;
            set;
        }

        public BoxStyle? PreviewStyle
        {
            get;
            set;
        }

        public BoxBounds? PreviewBounds
        {
            get;
            set;
        }

        public BoxStyle? ScreenshotStyle
        {
            get;
            set;
        }

        public List<BoxBounds> ScreenshotBounds
        {
            get;
            set;
        }

        public PreviewLayout Build()
        {
            return new PreviewLayout(
                virtualScreen: this.VirtualScreen ?? throw new InvalidOperationException($"{nameof(this.VirtualScreen)} must be initialized before calling {nameof(this.Build)}."),
                screens: this.Screens ?? throw new InvalidOperationException($"{nameof(this.Screens)} must be initialized before calling {nameof(this.Build)}."),
                activatedScreen: this.ActivatedScreen ?? throw new InvalidOperationException($"{nameof(this.ActivatedScreen)} must be initialized before calling {nameof(this.Build)}."),
                formBounds: this.FormBounds ?? throw new InvalidOperationException($"{nameof(this.FormBounds)} must be initialized before calling {nameof(this.Build)}."),
                previewStyle: this.PreviewStyle ?? throw new InvalidOperationException($"{nameof(this.PreviewStyle)} must be initialized before calling {nameof(this.Build)}."),
                previewBounds: this.PreviewBounds ?? throw new InvalidOperationException($"{nameof(this.PreviewBounds)} must be initialized before calling {nameof(this.Build)}."),
                screenshotStyle: this.ScreenshotStyle ?? throw new InvalidOperationException($"{nameof(this.ScreenshotStyle)} must be initialized before calling {nameof(this.Build)}."),
                screenshotBounds: this.ScreenshotBounds ?? throw new InvalidOperationException($"{nameof(this.ScreenshotBounds)} must be initialized before calling {nameof(this.Build)}."));
        }
    }

    public PreviewLayout(
        RectangleInfo virtualScreen,
        IEnumerable<ScreenInfo> screens,
        ScreenInfo activatedScreen,
        RectangleInfo formBounds,
        BoxStyle previewStyle,
        BoxBounds previewBounds,
        BoxStyle screenshotStyle,
        IEnumerable<BoxBounds> screenshotBounds)
    {
        this.VirtualScreen = virtualScreen ?? throw new ArgumentNullException(nameof(virtualScreen));
        this.Screens = new(
            (screens ?? throw new ArgumentNullException(nameof(screens)))
            .ToList());
        this.ActivatedScreen = activatedScreen ?? throw new ArgumentNullException(nameof(activatedScreen));
        this.FormBounds = formBounds ?? throw new ArgumentNullException(nameof(formBounds));
        this.PreviewStyle = previewStyle ?? throw new ArgumentNullException(nameof(previewStyle));
        this.PreviewBounds = previewBounds ?? throw new ArgumentNullException(nameof(previewBounds));
        this.ScreenshotStyle = screenshotStyle ?? throw new ArgumentNullException(nameof(screenshotStyle));
        this.ScreenshotBounds = new(
            (screenshotBounds ?? throw new ArgumentNullException(nameof(screenshotBounds)))
            .ToList());
    }

    public RectangleInfo VirtualScreen
    {
        get;
    }

    public ReadOnlyCollection<ScreenInfo> Screens
    {
        get;
    }

    public ScreenInfo ActivatedScreen
    {
        get;
    }

    public RectangleInfo FormBounds
    {
        get;
    }

    public BoxStyle PreviewStyle
    {
        get;
    }

    public BoxBounds PreviewBounds
    {
        get;
    }

    public BoxStyle ScreenshotStyle
    {
        get;
    }

    public ReadOnlyCollection<BoxBounds> ScreenshotBounds
    {
        get;
    }
}