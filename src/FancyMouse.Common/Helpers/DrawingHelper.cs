using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

using FancyMouse.Common.Bezels;
using FancyMouse.Models.Drawing;
using FancyMouse.Models.Styles;

namespace FancyMouse.Common.Helpers;

public static class DrawingHelper
{
    /// <summary>
    /// Renders the gradient-filled background for a box of the given <paramref name="size"/>.
    /// Can be used to render the main background for the preview pane, and the screen
    /// background image for delayed screenshot capture scenarios.
    /// </summary>
    public static Bitmap RenderBackground(SizeInfo size, BackgroundStyle backgroundStyle)
    {
        // prepare the bitmap (with a transparent background) to draw the background fill onto
        var boxStyle = new BoxStyle(MarginStyle.Empty, BorderStyle.Empty, PaddingStyle.Empty, backgroundStyle);
        var canvasBounds = BoxBounds.CreateFromOuterBounds(new RectangleInfo(size), boxStyle);
        var bounds = canvasBounds.OuterBounds.ToRectangle();
        var image = new Bitmap(bounds.Width, bounds.Height, PixelFormat.Format32bppPArgb);
        using var graphics = Graphics.FromImage(image);
        graphics.Clear(Color.Transparent);

        // pick a brush matching the configured background - a two-colour gradient, a solid fill
        // (whichever single colour is set), or nothing at all if neither colour is configured
        var backgroundBounds = canvasBounds.PaddingBounds.ToRectangle();
        using var backgroundBrush = backgroundStyle switch
        {
            { Color1: not null, Color2: not null } =>
                /* draw a gradient fill if both colors are specified */
                new LinearGradientBrush(
                    backgroundBounds,
                    backgroundStyle.Color1.Value,
                    backgroundStyle.Color2.Value,
                    LinearGradientMode.ForwardDiagonal),
            { Color1: not null } =>
                /* draw a solid fill if only one color is specified */
                new SolidBrush(backgroundStyle.Color1.Value),
            { Color2: not null } =>
                /* draw a solid fill if only one color is specified */
                new SolidBrush(backgroundStyle.Color2.Value),
            _ => (Brush?)null,
        };

        // fill the background, if a colour was actually configured
        if (backgroundBrush is not null)
        {
            graphics.FillRectangle(backgroundBrush, backgroundBounds);
        }

        return image;
    }

    /// <summary>
    /// Renders a raised border ring for a box of the given <paramref name="size"/>.
    /// Can be used to render the main preview window border, and the individual
    /// screen bezels.
    /// </summary>
    public static Bitmap RenderBorder(SizeInfo size, BorderStyle borderStyle)
    {
        // prepare the bitmap (with a transparent background) to draw the border / bezel onto
        var boxStyle = new BoxStyle(MarginStyle.Empty, borderStyle, PaddingStyle.Empty, BackgroundStyle.Empty);
        var hostBounds = BoxBounds.CreateFromOuterBounds(new RectangleInfo(size), boxStyle);
        var bounds = hostBounds.OuterBounds.ToRectangle();
        var image = new Bitmap(bounds.Width, bounds.Height, PixelFormat.Format32bppPArgb);
        using var graphics = Graphics.FromImage(image);
        graphics.Clear(Color.Transparent);

        if ((borderStyle.Horizontal < 1) || (borderStyle.Vertical < 1))
        {
            return image;
        }

        if (borderStyle.Color is null)
        {
            return image;
        }

        // draw a contoured border (could be an outer border or a bezel) - values are
        // compile-time constants; BezelConfig exists so DrawBezel is parameterised and ready
        // to accept per-bezel variation later
        var borderBounds = hostBounds.BorderBounds.ToRectangle();
        var bezelConfig = new BezelConfig(
            fadeStart: 30.0,            // degrees from edge where corner rolloff begins
            fadeEnd: 60.0,              // degrees where rolloff reaches zero
            highlightMax: 0x44 / 255.0, // peak highlight opacity (~26.7 %)
            shadowMax: 0x44 / 255.0,    // peak shadow   opacity (~26.7 %)
            edgeFadeFraction: 0.75f,    // fraction of edge length with secondary effect
            rampAngleDegrees: 45.0);    // chamfer inclination — cos(45°) ≈ 0.707 uniform intensity
        using var renderer = new BezelRenderer(borderStyle, bezelConfig);
        renderer.DrawBezel(graphics, borderBounds.X, borderBounds.Y, borderBounds.Width, borderBounds.Height);

        return image;
    }
}
