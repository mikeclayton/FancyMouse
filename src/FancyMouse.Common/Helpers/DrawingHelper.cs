using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

using FancyMouse.Models.Drawing;
using FancyMouse.Models.Layout;
using FancyMouse.Models.Styles;

namespace FancyMouse.Common.Helpers;

public static class DrawingHelper
{
    /// <summary>
    /// Renders the gradient-filled background for the specified canvas layout, as its own
    /// transparent-elsewhere image sized to <paramref name="canvasLayout"/>'s own outer
    /// bounds - a hosting window layers this beneath the per-screen bezels/screenshots (and,
    /// separately, its own border image) rather than having it baked into a single flattened
    /// bitmap.
    /// </summary>
    public static Bitmap RenderBackground(CanvasLayout canvasLayout)
    {
        var bounds = canvasLayout.CanvasBounds.OuterBounds.ToRectangle();
        var image = new Bitmap(bounds.Width, bounds.Height, PixelFormat.Format32bppPArgb);
        using var graphics = Graphics.FromImage(image);
        graphics.Clear(Color.Transparent);
        DrawingHelper.DrawBackgroundFill(
            graphics,
            canvasLayout.CanvasStyle,
            canvasLayout.CanvasBounds,
            []);
        return image;
    }

    /// <summary>
    /// Renders a raised border ring for the specified box, as its own transparent-elsewhere
    /// image sized to <paramref name="hostBounds"/>'s outer bounds. Used both by a hosting
    /// window to render its own outer border (see
    /// <see cref="LayoutHelper.GetHostBoxStyle"/>/<see cref="LayoutHelper.GetHostBounds"/>)
    /// and, per screen, to render each screen's bezel - the two uses share this method because
    /// a bezel is just a border ring around a smaller box.
    /// </summary>
    /// <remarks>
    /// <paramref name="hostBounds"/> must be zero-based (<c>OuterBounds.Location == (0,0)</c>)
    /// - its rectangles are used directly as pixel coordinates into the bitmap this
    /// allocates, which is sized to exactly fit it. Callers deriving a box by enlarging or
    /// using a non-zero-based content box (e.g. <see cref="LayoutHelper.GetHostBounds"/>, or a
    /// <see cref="ScreenLayout.ScreenBounds"/> which is relative to the canvas origin,
    /// not its own) need to re-anchor it first - e.g. <c>bounds.MoveTo(new PointInfo(0, 0))</c>.
    /// </remarks>
    public static Bitmap RenderBorder(BoxBounds hostBounds, BoxStyle hostStyle)
    {
        var bounds = hostBounds.OuterBounds.ToRectangle();
        var image = new Bitmap(bounds.Width, bounds.Height, PixelFormat.Format32bppPArgb);
        using var graphics = Graphics.FromImage(image);
        graphics.Clear(Color.Transparent);
        DrawingHelper.DrawRaisedBorder(graphics, hostBounds, hostStyle);
        return image;
    }

    // Hardcoded 3-D lighting config shared by all bezels.
    // Values are compile-time constants; BezelConfig exists so the rendering
    // methods are parameterised and ready to accept per-bezel variation later.
    private static readonly Drawing.ContouredBezels.BezelConfig ContouredBezelConfig = new(
        fadeStart: 30.0,            // degrees from edge where corner rolloff begins
        fadeEnd: 60.0,              // degrees where rolloff reaches zero
        highlightMax: 0x44 / 255.0, // peak highlight opacity (~26.7 %)
        shadowMax: 0x44 / 255.0,    // peak shadow   opacity (~26.7 %)
        edgeFadeFraction: 0.75f,    // fraction of edge length with secondary effect
        rampAngleDegrees: 45.0);    // chamfer inclination — cos(45°) ≈ 0.707 uniform intensity

    /// <summary>
    /// Draws a border shape with a raised 3-D highlight and shadow effect.
    /// </summary>
    private static void DrawRaisedBorder(
        Graphics graphics, BoxBounds boxBounds, BoxStyle boxStyle)
    {
        var borderStyle = boxStyle.BorderStyle;
        if ((borderStyle.Horizontal < 1) || (borderStyle.Vertical < 1))
        {
            return;
        }

        if (borderStyle.Color is null)
        {
            return;
        }

        // draw a contoured bezel
        var bounds = boxBounds.BorderBounds.ToRectangle();
        using var renderer = new Drawing.ContouredBezels.BezelRenderer(
            borderStyle,
            DrawingHelper.ContouredBezelConfig);
        renderer.DrawBezel(graphics, bounds.X, bounds.Y, bounds.Width, bounds.Height);
    }

    /// <summary>
    /// Draws a gradient-filled background shape.
    /// </summary>
    private static void DrawBackgroundFill(
        Graphics graphics, BoxStyle boxStyle, BoxBounds boxBounds, IEnumerable<RectangleInfo> excludeBounds)
    {
        var backgroundBounds = boxBounds.PaddingBounds;

        using var backgroundBrush = DrawingHelper.GetBackgroundStyleBrush(boxStyle.BackgroundStyle, backgroundBounds);
        if (backgroundBrush == null)
        {
            return;
        }

        // it's faster to build a region with the screen areas excluded
        // and fill that than it is to fill the entire bounding rectangle
        var backgroundRegion = new Region(backgroundBounds.ToRectangle());
        foreach (var exclude in excludeBounds)
        {
            backgroundRegion.Exclude(exclude.ToRectangle());
        }

        graphics.FillRegion(backgroundBrush, backgroundRegion);
    }

    private static Brush? GetBackgroundStyleBrush(BackgroundStyle backgroundStyle, RectangleInfo backgroundBounds)
    {
        var backgroundBrush = backgroundStyle switch
        {
            { Color1: not null, Color2: not null } =>
                /* draw a gradient fill if both colors are specified */
                new LinearGradientBrush(
                    backgroundBounds.ToRectangle(),
                    backgroundStyle.Color1.Value,
                    backgroundStyle.Color2.Value,
                    LinearGradientMode.ForwardDiagonal),
            { Color1: not null } =>
                /* draw a solid fill if only one color is specified */
                new SolidBrush(
                    backgroundStyle.Color1.Value),
            { Color2: not null } =>
                /* draw a solid fill if only one color is specified */
                new SolidBrush(
                    backgroundStyle.Color2.Value),
            _ => (Brush?)null,
        };
        return backgroundBrush;
    }
}
