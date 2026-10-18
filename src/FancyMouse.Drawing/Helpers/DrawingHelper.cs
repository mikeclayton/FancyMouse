using System.Drawing;

using FancyMouse.Drawing.Bezels;
using FancyMouse.Models.Drawing;
using FancyMouse.Models.Styles;

using SkiaSharp;

namespace FancyMouse.Drawing.Helpers;

public static class DrawingHelper
{
    /// <summary>
    /// Renders the gradient-filled background for a box of the given <paramref name="size"/>.
    /// Can be used to render the main background for the preview pane, and the screen
    /// background image for delayed screenshot capture scenarios.
    /// </summary>
    public static SKBitmap RenderBackground(SizeInfo size, BackgroundStyle backgroundStyle)
    {
        // prepare the bitmap (with a transparent background) to draw the background fill onto
        var backgroundBounds = new RectangleInfo(size).ToRectangle();
        var image = SkiaHelper.CreateBitmap(backgroundBounds.Width, backgroundBounds.Height, SKAlphaType.Premul);
        using var canvas = new SKCanvas(image);
        canvas.Clear(Color.Transparent.ToSKColor());

        // pick a shader matching the configured background - a two-colour gradient, a solid fill
        // (whichever single colour is set), or nothing at all if neither colour is configured
        using var backgroundShader = backgroundStyle switch
        {
            { Color1: not null, Color2: not null } =>
                /* draw a gradient fill if both colors are specified */
                DrawingHelper.CreateForwardDiagonalGradient(
                    backgroundBounds,
                    backgroundStyle.Color1.Value,
                    backgroundStyle.Color2.Value),
            { Color1: not null } =>
                /* draw a solid fill if only one color is specified */
                SKShader.CreateColor(backgroundStyle.Color1.Value.ToSKColor()),
            { Color2: not null } =>
                /* draw a solid fill if only one color is specified */
                SKShader.CreateColor(backgroundStyle.Color2.Value.ToSKColor()),
            _ => (SKShader?)null,
        };

        // fill the background, if a colour was actually configured
        if (backgroundShader is not null)
        {
            using var backgroundPaint = new SKPaint { Shader = backgroundShader };
            canvas.DrawRect(backgroundBounds.ToSKRect(), backgroundPaint);
        }

        return image;
    }

    /// <summary>
    /// The equivalent of GDI+'s <c>LinearGradientMode.ForwardDiagonal</c> - <paramref name="color1"/>
    /// in the top-left corner of <paramref name="bounds"/>, <paramref name="color2"/> in the
    /// bottom-right corner, with lines of equal colour running parallel to the *other*
    /// (bottom-left to top-right) diagonal whatever the aspect ratio.
    /// </summary>
    /// <remarks>
    /// The gradient is built in a unit square, where a (0,0) to (1,1) gradient's lines of equal
    /// colour are exactly parallel to the bottom-left to top-right diagonal, and then scaled to
    /// <paramref name="bounds"/> so they stay parallel to that diagonal once it's stretched. A
    /// plain corner-to-corner gradient in <paramref name="bounds"/> itself would only match
    /// GDI+ for a square, since its lines of equal colour are perpendicular to the gradient.
    /// </remarks>
    private static SKShader CreateForwardDiagonalGradient(Rectangle bounds, Color color1, Color color2)
    {
        return SKShader.CreateLinearGradient(
            new SKPoint(0, 0),
            new SKPoint(1, 1),
            [color1.ToSKColor(), color2.ToSKColor()],
            null,
            SKShaderTileMode.Clamp,
            SKMatrix.CreateScaleTranslation(bounds.Width, bounds.Height, bounds.X, bounds.Y));
    }

    /// <summary>
    /// Renders a raised border ring for a box of the given <paramref name="size"/>.
    /// Can be used to render the main preview window border, and the individual
    /// screen bezels.
    /// </summary>
    public static SKBitmap RenderBorder(SizeInfo size, BorderStyle borderStyle)
    {
        // prepare the bitmap (with a transparent background) to draw the border / bezel onto
        var borderBounds = new RectangleInfo(size).ToRectangle();
        var image = SkiaHelper.CreateBitmap(borderBounds.Width, borderBounds.Height, SKAlphaType.Premul);
        using var canvas = new SKCanvas(image);
        canvas.Clear(Color.Transparent.ToSKColor());

        if ((borderStyle.Horizontal < 1) || (borderStyle.Vertical < 1))
        {
            return image;
        }

        if (borderStyle.Color is null)
        {
            return image;
        }

        // draw a contoured border (could be an outer border or a bezel)
        using var renderer = new BezelRenderer(borderStyle);
        renderer.DrawBezel(canvas, borderBounds.X, borderBounds.Y, borderBounds.Width, borderBounds.Height);

        return image;
    }
}
