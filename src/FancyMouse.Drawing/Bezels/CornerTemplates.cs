using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

using FancyMouse.Models.Styles;

using static FancyMouse.Drawing.Bezels.BezelPrimitives;

namespace FancyMouse.Drawing.Bezels;

internal static class CornerTemplates
{
    /// <summary>
    /// Returns an image containing a template for the 4 corners of the bezel.
    /// </summary>
    /// <remarks>
    /// GDI+ doesn't draw perfectly symmetrical or consistent arcs, and rounding
    /// is sometimes even based on the *coordinates* being drawn to rather than
    /// the shape of the path, which means we'd need to apply highlight and shadow
    /// effects to the corners dynamically when every bezel is rendered in order
    /// to ensure a pixel-perfect effect is applied to each corner.
    ///
    /// Our highlight and shadow effects are not massively expensive, so this
    /// wouldn't be a *major* performance problem, *but* we can reduce the CPU
    /// load by pre-rendering the corners and reusing the same template for every
    /// bezel we draw. We can then simply use DrawImage to copy regions from the
    /// template into the correct position for each bezel, and be assured that the
    /// same pixel-perfect effect is applied to every corner.
    /// </remarks>
    /// <returns>
    /// Returns a bitmap containing a 2x2 sprite grid of the bezel corners.
    /// Individual cells in the grid are the size of the "BezelThickness" parameter,
    /// so the full image is "2 × BezelThickness" in width and height. The corners
    /// are arranged in the "obvious" order - the image for the top left corner is
    /// in the top left cell of the grid, etc.
    ///
    /// The template image needs to be recreated if the color, thickness or 3d effect
    /// depth settings change.
    /// </returns>
    internal static Bitmap GetCornerTemplates(BorderStyle borderStyle, BezelConfig config)
    {
        var n = (int)borderStyle.Left;
        var depth = (int)borderStyle.Depth;
        var bezelColor = borderStyle.Color ?? Color.Transparent;

        // ── Step 1: render temporary bezel "ring" images ───────────────────

        // render a set of temporary bezels that we'll use to draw the
        // corner images and apply highlight and shadow effects to
        //
        // the images we'll generate are:
        //
        // * a flat bezel, which is the base image we'll draw the lighting effects onto
        // * a thin "outer" bezel with a curved inner radius at outer bounds of the flat bezel
        // * a thin "inner" bezel with quadrant corners at the inner bounds of the flat bezel
        //
        // each call renders a bezel onto a 3N×3N bitmap and extracts the four N×N
        // corner regions into a compact 2N×2N image

        // draw a bezel ring with flat corners, using the base colour pixels that
        // lighting effects are drawn on top of
        //
        // e.g. top left corner - filled arc
        // +----------+
        // |   ░▒▓▓▓▓▓|
        // | ░▓▓▓▓▓▓▓▓|
        // |▒▓▓▓▓▓▓▓▓▓|
        // |▓▓▓▓▓▓▓▓▓▓|
        // +----------+
        // |<-- N --->|
        var cornerTemplates = CornerTemplates.DrawCornerRegions(
            cornerSize: n,
            outerRadius: n,
            innerRadius: 0,
            color: bezelColor);

        if (depth == 0)
        {
            return cornerTemplates;
        }

        // ── Step 2: apply highlight and shadow effects ─────────────────────────
        double CornerEffectWeight(double theta) => BezelPrimitives.CornerEffectWeight(theta, config.FadeStart, config.FadeEnd);

        var profile = new BezelProfile(n, depth);

        var cornerData = default(BitmapData);

        try
        {
            cornerData = cornerTemplates.LockBits(
                new Rectangle(0, 0, cornerTemplates.Width, cornerTemplates.Height),
                ImageLockMode.ReadWrite,
                PixelFormat.Format32bppArgb);

            unsafe
            {
                byte* cornerScan0 = (byte*)cornerData.Scan0;
                var cornerStride = cornerData.Stride;
                const int bytesPerPixel = 4; // PixelFormat.Format32bppArgb

                for (var srcY = 0; srcY < cornerTemplates.Height; srcY++)
                {
                    for (var srcX = 0; srcX < cornerTemplates.Width; srcX++)
                    {
                        // read the current pixel's alpha channel from the flat bezel image
                        byte* srcPixelArgb = cornerScan0 + (srcY * cornerStride) + (srcX * bytesPerPixel);
                        if (srcPixelArgb[3] == 0)
                        {
                            // the flat bezel's pixel is 100% transparent so we're
                            // completely outside the drawing area and don't need
                            // to apply any lighting effect to this pixel
                            continue;
                        }

                        // calculate the offset of the pixel relative to the centre of the
                        // corner  template - the sign on the x and y coordinate tell us
                        // which quadrant it's it in (i.e. TL, TR, BL, BR)
                        var originOffset = new Point(
                            y: srcY - n,
                            x: srcX - n);

                        // BezelProfile.GetCornerIntensity calculates the intensity of the
                        // lighting effect at the specified location in a bezel corner. it
                        // calculates the normal of the bezel's profile at the point and
                        // converts that into the intensity of the highlight or shadow.
                        //
                        // it returns a signed intensity in the range [-1, +1]:
                        //
                        //   effectIntensity > 0  — outer arc, surface faces the light → apply as highlight
                        //   effectIntensity < 0  — inner arc, surface faces away      → apply as shadow
                        //   effectIntensity ≈ 0  — flat zone                          → no effect
                        var effectIntensity = profile.GetCornerIntensity(originOffset);

                        // Math.Abs(effectIntensity) carries the unsigned scaling factor
                        // for the lighting effect on this pixel - multiply this by the
                        // alpha channel of the flat bezel to determine the final
                        // transparency of the effect
                        var effectMagnitude = Math.Abs(effectIntensity);
                        if (effectMagnitude < 1e-10)
                        {
                            // flat zone — leave as plain bezelColor
                            continue;
                        }

                        // +ve effect intensity is highlight,
                        // -ve effect intensity is shadow
                        var isHighlightEffect = effectIntensity > 0.0;

                        double theta;
                        var hl = 0.0;
                        var sh = 0.0;

                        // use the +/- sign of the offsets to determine the quadrant the pixel is in
                        if (originOffset.Y < 0)
                        {
                            if (originOffset.X < 0)
                            {
                                // TL — outer: highlight from the left edge meets highlight from the top edge,
                                //      inner: shadow from the left edge meets shadow from the top edge,
                                //      → outer double-highlight, inner double-shadow
                                theta = 270.0 - GdiAngle(originOffset.X, originOffset.Y);
                                var weight = CornerEffectWeight(theta) + CornerEffectWeight(90 - theta) + 0.5 + (0.75 * MidpointPeak(theta));
                                if (isHighlightEffect)
                                {
                                    hl = weight;
                                }
                                else
                                {
                                    sh = weight;
                                }
                            }
                            else
                            {
                                // TR — outer: highlight from the top edge meets shadow from the right edge,
                                //      inner: shadow from the top edge meets highlight from the right edge,
                                //      → both fade to flat at 45°
                                theta = (GdiAngle(originOffset.X, originOffset.Y) - 270.0 + 360.0) % 360.0;
                                if (isHighlightEffect)
                                {
                                    hl = CornerEffectWeight(theta) * MidpointFade(theta);
                                    sh = CornerEffectWeight(90 - theta) * MidpointFade(90 - theta);
                                }
                                else
                                {
                                    hl = CornerEffectWeight(90 - theta) * MidpointFade(90 - theta);
                                    sh = CornerEffectWeight(theta) * MidpointFade(theta);
                                }
                            }
                        }
                        else
                        {
                            if (originOffset.X < 0)
                            {
                                // BL — outer: highlight from the left edge meets shadow from the bottom edge,
                                //      inner: shadow from the left edge meets highlight from the bottom edge,
                                //      → both fade to flat at 45°
                                theta = GdiAngle(originOffset.X, originOffset.Y) - 90.0;
                                if (isHighlightEffect)
                                {
                                    hl = CornerEffectWeight(90 - theta) * MidpointFade(90 - theta);
                                    sh = CornerEffectWeight(theta) * MidpointFade(theta);
                                }
                                else
                                {
                                    hl = CornerEffectWeight(theta) * MidpointFade(theta);
                                    sh = CornerEffectWeight(90 - theta) * MidpointFade(90 - theta);
                                }
                            }
                            else
                            {
                                // BR — outer: shadow from the right edge meets shadow from the bottom edge,
                                //      inner: highlight from the right edge meets highlight from the bottom edge,
                                //      → outer double-shadow, inner single-highlight
                                //      (inner HL halved to avoid over-brightness against outer-BR shadow)
                                theta = 90.0 - GdiAngle(originOffset.X, originOffset.Y);
                                if (isHighlightEffect)
                                {
                                    sh = CornerEffectWeight(theta) + CornerEffectWeight(90 - theta) + 0.5 + (0.275 * MidpointPeak(theta));
                                }
                                else
                                {
                                    hl = (0.5 * CornerEffectWeight(theta)) + (0.5 * CornerEffectWeight(90 - theta)) + 0.25 + (0.375 * MidpointPeak(theta));
                                }
                            }
                        }

                        // effectMagnitude carries the effect intensity due to the ; the sign replaces the
                        // previous inOuterArc/inInnerArc flags that were sourced from overlay
                        // bitmaps. The flat bezel's pixel alpha (from GDI+ arc antialiasing) is
                        // left unchanged and handles outer-edge transparency automatically.
                        var newColor = ApplyEffect(hl * effectMagnitude, sh * effectMagnitude, bezelColor, config.HighlightMax, config.ShadowMax);
                        srcPixelArgb[0] = newColor.B;
                        srcPixelArgb[1] = newColor.G;
                        srcPixelArgb[2] = newColor.R;

                        // srcArgb[3] (alpha) intentionally unchanged — preserves antialiased
                        // outer-edge coverage for correct DrawImage compositing
                    }
                }
            }
        }
        finally
        {
            if (cornerData is not null)
            {
                cornerTemplates.UnlockBits(cornerData);
            }
        }

        return cornerTemplates;
    }

    /// <summary>
    /// Draws four N×N corner regions packed into a 2N×2N image.
    /// </summary>
    private static Bitmap DrawCornerRegions(int cornerSize, int outerRadius, int innerRadius, Color color)
    {
        var n = cornerSize;

        // draw the flat bezel *with* straight edges first so that the
        // GDI antialiasing smooths the corners into straight edges rather
        // than into an immediately adjacent corner. it means we have to copy
        // the corners out into a smaller image to remove the straight edges
        // later, but we get a better quality result
        using var sourceImage = new Bitmap(3 * n, 3 * n, PixelFormat.Format32bppArgb);
        using (var g = Graphics.FromImage(sourceImage))
        {
            BezelGraphics.DrawFlatBezelRing(
                g,
                x: n - outerRadius,
                y: n - outerRadius,
                width: n + (2 * outerRadius),
                height: n + (2 * outerRadius),
                outerRadius: outerRadius,
                innerRadius: innerRadius,
                color: color);
        }

        // set up the copy regions to extract the corner images from the bezel ring
        //
        //   +----+----+----+
        //   | // | == | \\ |      +----+----+
        //   +----+----+----+      | // | \\ |
        //   | || |    | || |  =>  +----+----+
        //   +----+----+----+      | \\ | // |
        //   | \\ | == | // |      +----+----+
        //   +----+----+----+
        var w = sourceImage.Width;
        var h = sourceImage.Height;
        var copyRegions = new[]
        {
            (source: new Point(0,     0),     target: new Point(0, 0)), // TL
            (source: new Point(w - n, 0),     target: new Point(n, 0)), // TR
            (source: new Point(0,     h - n), target: new Point(0, n)), // BL
            (source: new Point(w - n, h - n), target: new Point(n, n)), // BR
        };

        var cornerImages = new Bitmap(2 * n, 2 * n, PixelFormat.Format32bppArgb);
        using var cornerGraphics = Graphics.FromImage(cornerImages);

        // Use NearestNeighbor + PixelOffsetMode.Half for an exact 1:1 pixel copy.
        // With Half mode the sample point for dest pixel i is exactly i (not i+0.5),
        // so NearestNeighbor snaps to the correct source pixel with no bilinear blurring.
        // Default Bilinear would sample at i+0.5 — blending adjacent pixels at the arc
        // boundary — which shifts the visual arc edge ~0.5 px inward and creates a
        // visible gap between the corner arc and the flat-fill straight edges.
        cornerGraphics.InterpolationMode = InterpolationMode.NearestNeighbor;
        cornerGraphics.PixelOffsetMode = PixelOffsetMode.Half;

        foreach (var (source, target) in copyRegions)
        {
            cornerGraphics.DrawImage(
                sourceImage,
                destRect: new Rectangle(target.X, target.Y, n, n),
                srcRect: new Rectangle(source.X, source.Y, n, n),
                srcUnit: GraphicsUnit.Pixel);
        }

        return cornerImages;
    }
}
