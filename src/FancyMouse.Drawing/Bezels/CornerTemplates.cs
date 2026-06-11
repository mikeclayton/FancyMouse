using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

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
    internal static Bitmap GetCornerTemplates(
        int bezelThickness,
        int bezel3DDepth,
        Color bezelColor,
        double fadeStartDegrees,
        double fadeEndDegrees,
        double highlightMax,
        double shadowMax)
    {
        var n = bezelThickness;
        var d = bezel3DDepth;

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

        // outer effect overlay — a white ring D pixels wide at the outer arc zone
        // of each corner, drawn with GDI+ antialiasing so we can blend the effect
        // into the color (or transparency) behind it
        //
        // e.g. top left corner - outer effect "ring"
        // +----------+
        // |   ░▒▓▓▓▓▓|
        // | ░▓▓▓▓▒   |
        // |▒▓▓▒▒     |
        // |▓▓▓▒      |
        // +----------+
        // |<->| D
        //     |<---->| N - D
        // |<-- N --->|
        using var outerEffectOverlay = CornerTemplates.DrawCornerRegions(
            cornerSize: n,
            outerRadius: n,
            innerRadius: n - d,
            color: Color.White);

        // inner effect overlay — a white ring D pixels wide at the inner arc
        // zone of each corner
        //
        // e.g. top left corner - inner effect "ring"
        // +----------+
        // |          |
        // |          |
        // |       ░▒▓|
        // |      ░▓▓▓|
        // +----------+
        //        |<->| D
        // |<-- N --->|
        using var innerEffectOverlay = CornerTemplates.DrawCornerRegions(
            cornerSize: n,
            outerRadius: d,
            innerRadius: 0,
            color: Color.White);

        // ── Step 2: copy the effect overlay pixels onto the corner image ───────────────────
        //
        // we've already drawn the *flat* bezel corners onto the corner image.
        //
        // now we want to add our highlight and shadow effects, which we'll do
        // by drawing partially transparent white (highlight) or black (shadow)
        // pixels on top of the flat bezel pixels. this allows the light effect
        // to be drawn as an accent based on the flat bezel color.
        //
        // the transparency for each pixel is taken from the outer and inner
        // overlay images and transferred onto the corner image as a partially
        // transparent highlight (white) or shadow (black) effect. if an overlay
        // pixel is fully transparent we don't draw anything on the corner image.
        double CornerEffectWeight(double theta) => BezelPrimitives.CornerEffectWeight(theta, fadeStartDegrees, fadeEndDegrees);

        var cornerData = default(BitmapData);
        var outerOverlayData = default(BitmapData);
        var innerOverlayData = default(BitmapData);

        try
        {
            // lock the bitmaps so we can access direct memory for pixel io operations
            cornerData = cornerTemplates.LockBits(
                new Rectangle(0, 0, cornerTemplates.Width, cornerTemplates.Height),
                ImageLockMode.ReadWrite,
                PixelFormat.Format32bppArgb);
            outerOverlayData = outerEffectOverlay.LockBits(
                new Rectangle(0, 0, outerEffectOverlay.Width, outerEffectOverlay.Height),
                ImageLockMode.ReadOnly,
                PixelFormat.Format32bppArgb);
            innerOverlayData = innerEffectOverlay.LockBits(
                new Rectangle(0, 0, innerEffectOverlay.Width, innerEffectOverlay.Height),
                ImageLockMode.ReadOnly,
                PixelFormat.Format32bppArgb);

            unsafe
            {
                byte* cornerScan0 = (byte*)cornerData.Scan0;
                var cornerStride = cornerData.Stride;
                byte* outerOverlayScan0 = (byte*)outerOverlayData.Scan0;
                var outerOverlayStride = outerOverlayData.Stride;
                byte* innerOverlayScan0 = (byte*)innerOverlayData.Scan0;
                var innerOverlayStride = innerOverlayData.Stride;
                const int bytesPerPixel = 4; // PixelFormat.Format32bppArgb

                // iterate over the entire corner and overlay images.
                // depending on which quadrant a pixel is in, we'll use
                // different combinations of colors for lighting effects
                // with the outer and inner overlay pixels
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

                        byte* outerOverlayArgb = outerOverlayScan0 + (srcY * outerOverlayStride) + (srcX * bytesPerPixel);
                        var outerFade = outerOverlayArgb[3] / 255.0;
                        var inOuterArc = outerFade > 0.0;

                        byte* innerOverlayArgb = innerOverlayScan0 + (srcY * innerOverlayStride) + (srcX * bytesPerPixel);
                        var innerFade = innerOverlayArgb[3] / 255.0;
                        var inInnerArc = innerFade > 0.0;

                        if (!inOuterArc && !inInnerArc)
                        {
                            // flat zone between lighting effects — leave as plain bezelColor
                            continue;
                        }

                        var arcFade = inOuterArc ? outerFade : innerFade;

                        double theta;
                        var hl = 0.0;
                        var sh = 0.0;

                        var originOffset = new Point(
                            y: srcY - n,
                            x: srcX - n);

                        if (originOffset.Y < 0)
                        {
                            if (originOffset.X < 0)
                            {
                                // TL — outer: highlight from the left edge meets highlight from the top edge,
                                //      inner: shadow from the left edge meets shadow from the top edge,
                                //      → outer double-highlight, inner double-shadow
                                theta = 270.0 - GdiAngle(originOffset.X, originOffset.Y);
                                var weight = CornerEffectWeight(theta) + CornerEffectWeight(90 - theta) + 0.5 + (0.75 * MidpointPeak(theta));
                                if (inOuterArc)
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
                                if (inOuterArc)
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
                                if (inOuterArc)
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
                                if (inOuterArc)
                                {
                                    sh = CornerEffectWeight(theta) + CornerEffectWeight(90 - theta) + 0.5 + (0.275 * MidpointPeak(theta));
                                }
                                else
                                {
                                    hl = (0.5 * CornerEffectWeight(theta)) + (0.5 * CornerEffectWeight(90 - theta)) + 0.25 + (0.375 * MidpointPeak(theta));
                                }
                            }
                        }

                        var newColor = ApplyEffect(hl * arcFade, sh * arcFade, bezelColor, highlightMax, shadowMax);
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

            if (outerOverlayData is not null)
            {
                outerEffectOverlay.UnlockBits(outerOverlayData);
            }

            if (innerOverlayData is not null)
            {
                innerEffectOverlay.UnlockBits(innerOverlayData);
            }
        }

        cornerTemplates.Save(
            $@"C:\temp\corner_atlas_{n}_{bezel3DDepth}.png",
            ImageFormat.Png);

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
