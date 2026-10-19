using System.Drawing;

using FancyMouse.Drawing.Helpers;

using SkiaSharp;

using static FancyMouse.Drawing.Bezels.BezelPrimitives;

namespace FancyMouse.Drawing.Bezels;

public sealed partial class BezelRenderer
{
    /// <summary>
    /// Returns an image containing a template for the 4 corners of the bezel.
    /// </summary>
    /// <remarks>
    /// Antialiased arcs aren't guaranteed to be drawn perfectly symmetrically or
    /// consistently (GDI+, which this was originally written for, even rounded
    /// differently depending on the *coordinates* being drawn to rather than the
    /// shape of the path), which means we'd need to apply highlight and shadow
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
    /// Individual cells in the grid are the size of the "bezelWidth" parameter,
    /// so the full image is "2 × bezelWidth" in width and height. The corners
    /// are arranged in the "obvious" order - the image for the top left corner is
    /// in the top left cell of the grid, etc.
    ///
    ///         TL   TR
    ///       0    n   2n
    ///     0 +----+----+
    ///       | // | \\ |
    ///     n +----+----+
    ///       | \\ | // |
    ///    2n +----+----+
    ///         BL   BR
    ///
    /// The template image needs to be recreated if the color, thickness or 3d effect
    /// depth settings change.
    /// </returns>
    private static SKBitmap GetCornerTemplates(int bezelWidth, Color bezelColor, IBezelProfile bezelProfile)
    {
        // Render at 2× and scale down so GDI+ antialiases the highlight/shadow
        // zone-boundary edges in the corner tiles before they are baked into the atlas.
        var scaledWidth = bezelWidth * 2;

        // ── Step 1: render temporary bezel "ring" images ───────────────────

        // render a set of temporary bezels that we'll use to draw the
        // corner images and apply highlight and shadow effects
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
        // |<--- n -->|
        using var cornerTemplates = BezelRenderer.DrawCornerRegions(
            cornerRadius: scaledWidth,
            bezelColor: bezelColor);

        // ── Step 2: apply highlight and shadow effects ─────────────────────────
        double CornerEffectWeight(double theta) => BezelPrimitives.CornerEffectWeight(theta, BezelConstants.CornerGradientStart, BezelConstants.CornerGradientEnd);

        try
        {
            unsafe
            {
                byte* cornerScan0 = (byte*)cornerTemplates.GetPixels();
                var cornerStride = cornerTemplates.RowBytes;
                const int bytesPerPixel = 4; // SKColorType.Bgra8888

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
                        // corner template - the sign on the x and y coordinate tell us
                        // which quadrant it's it in (i.e. TL, TR, BL, BR)
                        var originOffset = new Point(
                            y: srcY - scaledWidth,
                            x: srcX - scaledWidth);

                        // BezelProfile.GetCornerNormal calculates the normal of the bezel's
                        // profile at this location in a bezel corner, and GetLightingEffectIntensity
                        // converts that into the intensity of the highlight or shadow.
                        //
                        // it returns a signed intensity in the range [-1, +1]:
                        //
                        //   effectIntensity > 0  — outer arc, surface faces the light → apply as highlight
                        //   effectIntensity < 0  — inner arc, surface faces away      → apply as shadow
                        //   effectIntensity ≈ 0  — flat zone                          → no effect
                        var cornerNormal = bezelProfile.GetCornerNormal(scaledWidth, originOffset);
                        var effectIntensity = FancyMouse.Drawing.Bezels.BezelProfile.GetLightingEffectIntensity(cornerNormal);

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
                                //
                                // top and left are both "forward" edges (BezelConstants.EdgeForward*), so
                                // one strength value covers both boundary terms. CornerEffectWeight(theta) +
                                // CornerEffectWeight(90 - theta) is identically 1 for every theta in [0, 90]
                                // (the cosine ease is point-symmetric about its 45° midpoint), so the
                                // boundary value (theta = 0 or 90, where only one edge is actually adjacent)
                                // reduces to exactly edgeStrength - matching that edge's own corner-adjacent
                                // pixel - with MidpointPeak adding the extra brightness where the two edges'
                                // effects genuinely overlap, right at the 45° diagonal.
                                theta = 270.0 - BezelPrimitives.GetGdiAngle(originOffset.X, originOffset.Y);
                                var edgeStrength = isHighlightEffect ? BezelConstants.EdgeForwardHighlightStrong : BezelConstants.EdgeForwardShadowStrong;
                                var weight = edgeStrength + (0.75 * MidpointPeak(theta));
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
                                //
                                // unlike TL/BR, TR sits at the *fade* end of both its adjoining edges - the
                                // top edge peaks near TL and fades rightward, the right edge peaks near BR
                                // and fades upward (see DrawBezelEdges' own "peak at TL"/"peak at BR"
                                // comments) - so each term uses that edge's Fade constant, not Strong: at
                                // theta = 0 (top edge boundary) MidpointFade(0) = 1, so the top-associated
                                // term reduces to exactly EdgeForward*Fade, matching the top edge's own
                                // pixel at this corner; symmetrically at theta = 90 the right-associated term
                                // reduces to EdgeReverse*Fade.
                                theta = (BezelPrimitives.GetGdiAngle(originOffset.X, originOffset.Y) - 270.0 + 360.0) % 360.0;
                                if (isHighlightEffect)
                                {
                                    hl = CornerEffectWeight(theta) * MidpointFade(theta) * BezelConstants.EdgeForwardHighlightFade;
                                    sh = CornerEffectWeight(90 - theta) * MidpointFade(90 - theta) * BezelConstants.EdgeReverseShadowFade;
                                }
                                else
                                {
                                    hl = CornerEffectWeight(90 - theta) * MidpointFade(90 - theta) * BezelConstants.EdgeReverseHighlightFade;
                                    sh = CornerEffectWeight(theta) * MidpointFade(theta) * BezelConstants.EdgeForwardShadowFade;
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
                                //
                                // like TR, BL sits at the *fade* end of both its adjoining edges - the left
                                // edge peaks near TL and fades downward, the bottom edge peaks near BR and
                                // fades leftward - see the TR case above for why each boundary term reduces
                                // to exactly that edge's own Fade value at theta = 0 / 90.
                                theta = BezelPrimitives.GetGdiAngle(originOffset.X, originOffset.Y) - 90.0;
                                if (isHighlightEffect)
                                {
                                    hl = CornerEffectWeight(90 - theta) * MidpointFade(90 - theta) * BezelConstants.EdgeForwardHighlightFade;
                                    sh = CornerEffectWeight(theta) * MidpointFade(theta) * BezelConstants.EdgeReverseShadowFade;
                                }
                                else
                                {
                                    hl = CornerEffectWeight(theta) * MidpointFade(theta) * BezelConstants.EdgeReverseHighlightFade;
                                    sh = CornerEffectWeight(90 - theta) * MidpointFade(90 - theta) * BezelConstants.EdgeForwardShadowFade;
                                }
                            }
                            else
                            {
                                // BR — outer: shadow from the right edge meets shadow from the bottom edge,
                                //      inner: highlight from the right edge meets highlight from the bottom edge,
                                //      → outer double-shadow, inner single-highlight
                                //      (inner HL halved to avoid over-brightness against outer-BR shadow)
                                //
                                // right and bottom are both "reverse" edges, so one strength value covers
                                // both boundary terms (same CornerEffectWeight(theta) + CornerEffectWeight(90
                                // - theta) ≡ 1 identity as the TL case above). EdgeReverseHighlightStrong is
                                // itself already the "halved" value (0.75, half of EdgeReverseShadowStrong's
                                // 1.5) that avoids over-brightness against the outer-BR shadow, so it's used
                                // directly here rather than halved again.
                                theta = 90.0 - BezelPrimitives.GetGdiAngle(originOffset.X, originOffset.Y);
                                if (isHighlightEffect)
                                {
                                    sh = BezelConstants.EdgeReverseShadowStrong + (0.275 * MidpointPeak(theta));
                                }
                                else
                                {
                                    hl = BezelConstants.EdgeReverseHighlightStrong + (0.375 * MidpointPeak(theta));
                                }
                            }
                        }

                        // effectMagnitude carries the effect intensity - the sign indicates
                        // whether to apply a highlight or shadow. The flat bezel's pixel
                        // alpha (from arc antialiasing) is left unchanged and handles
                        // outer-edge transparency automatically so the resulting pixel still
                        // blends into the background image behind the bezel.
                        var highlighted = ApplyHighlight(bezelColor, hl * effectMagnitude, BezelConstants.HighlightMax);
                        var newColor = ApplyShadow(highlighted, sh * effectMagnitude, BezelConstants.ShadowMax);

                        // note - this is *not* dead code! srcPixelArgb is a raw (unsafe) pointer
                        // into the cornerTemplates bitmap's pixels not a normal managed array, so
                        // the assignments below write directly into the image in-place.
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
            // the pixels were written directly through a pointer, so tell skia they've
            // changed in case it's cached anything derived from them
            cornerTemplates.NotifyPixelsChanged();
        }

        return BezelRenderer.ScaleHalf(cornerTemplates);
    }

    /// <summary>
    /// Scales a bitmap to half its width and height using high-quality bicubic
    /// interpolation, antialiasing any sharp colour transitions in the source.
    /// The caller owns the returned bitmap; the source is not disposed.
    /// </summary>
    private static SKBitmap ScaleHalf(SKBitmap source)
    {
        // filter in premultiplied space, the same as GDI+ did - filtering straight (unpremultiplied)
        // colours blends in the colour of the fully transparent pixels around the arc (black),
        // darkening its antialiased edge pixels more the more transparent they are. a 1:1
        // unfiltered copy onto a premultiplied bitmap does the conversion
        using var premultiplied = SkiaHelper.CreateBitmap(source.Width, source.Height, SKAlphaType.Premul);
        using (var premultipliedCanvas = new SKCanvas(premultiplied))
        {
            premultipliedCanvas.DrawBitmap(source, 0, 0);
        }

        var result = SkiaHelper.CreateBitmap(source.Width / 2, source.Height / 2, SKAlphaType.Unpremul);
        using var canvas = new SKCanvas(result);
        using var sourceImage = SkiaHelper.AsImage(premultiplied);

        // Mitchell is skia's standard "high quality" cubic resampler (what its old
        // SKFilterQuality.High mapped to) - the nearest equivalent of GDI+'s
        // InterpolationMode.HighQualityBicubic
        canvas.DrawImage(
            sourceImage,
            SKRect.Create(0, 0, source.Width, source.Height),
            SKRect.Create(0, 0, result.Width, result.Height),
            new SKSamplingOptions(SKCubicResampler.Mitchell));
        return result;
    }

    /// <summary>
    /// Draws four n×n corner regions packed into a 2n×2n image, where n is
    /// <paramref name="cornerRadius"/>.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="cornerRadius"/> is less than 1.
    /// </exception>
    private static SKBitmap DrawCornerRegions(int cornerRadius, Color bezelColor)
    {
        if (cornerRadius <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(cornerRadius), cornerRadius, $"{nameof(cornerRadius)} must be at least 1.");
        }

        var edgeLength = cornerRadius;
        var imageWidth = edgeLength + (2 * cornerRadius);
        var imageHeight = edgeLength + (2 * cornerRadius);

        // draw the flat bezel *with* straight edges first so that the
        // antialiasing smooths the corners into straight edges rather
        // than into an immediately adjacent corner. it means we have to copy
        // the corners out into a smaller image to remove the straight edges
        // later, but we get a better quality result
        using var sourceImage = SkiaHelper.CreateBitmap(imageWidth, imageHeight, SKAlphaType.Unpremul);
        using (var canvas = new SKCanvas(sourceImage))
        {
            BezelRenderer.DrawFlatBezelRing(
                canvas,
                x: 0,
                y: 0,
                width: imageWidth,
                height: imageHeight,
                cornerRadius: cornerRadius,
                bezelColor: bezelColor);
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
        var w = imageWidth;
        var h = imageHeight;
        var z = edgeLength;
        var c = cornerRadius;
        var copyRegions = new[]
        {
            (source: new Point(0,     0),     target: new Point(0, 0)), // TL
            (source: new Point(w - z, 0),     target: new Point(c, 0)), // TR
            (source: new Point(0,     h - z), target: new Point(0, c)), // BL
            (source: new Point(w - z, h - z), target: new Point(c, c)), // BR
        };

        var cornerImages = SkiaHelper.CreateBitmap(2 * cornerRadius, 2 * cornerRadius, SKAlphaType.Unpremul);
        using var cornerCanvas = new SKCanvas(cornerImages);
        using var sourceSkImage = SkiaHelper.AsImage(sourceImage);

        // Use nearest-neighbour sampling for an exact 1:1 pixel copy - each destination
        // pixel's centre maps onto exactly one source pixel's centre, so it snaps to the
        // correct source pixel with no bilinear blurring. Bilinear would blend adjacent
        // pixels at the arc boundary, softening the visual arc edge and creating a visible
        // gap between the corner arc and the flat-fill straight edges. SKBlendMode.Src
        // copies each pixel as-is rather than blending it over the (empty) destination.
        var sampling = new SKSamplingOptions(SKFilterMode.Nearest);
        using var copyPaint = new SKPaint { BlendMode = SKBlendMode.Src };

        foreach (var (source, target) in copyRegions)
        {
            cornerCanvas.DrawImage(
                sourceSkImage,
                SKRect.Create(source.X, source.Y, cornerRadius, cornerRadius),
                SKRect.Create(target.X, target.Y, cornerRadius, cornerRadius),
                sampling,
                copyPaint);
        }

        return cornerImages;
    }

    /// <summary>
    /// Builds the corner atlas from <see cref="BorderStyle"/>, for use at construction.
    /// </summary>
    private SKImage DrawCornerTemplates()
    {
        var bezelWidth = (int)this.BezelStyle.Left;

        // this.BezelProfile is scale-independent, so the same instance used for the
        // 1× edge rendering is valid here too, even though GetCornerTemplates
        // renders its atlas at 2× supersampling internally.
        using var cornerTemplates = BezelRenderer.GetCornerTemplates(
            bezelWidth,
            this.BezelStyle.Color ?? Color.Transparent,
            this.BezelProfile);

        // an immutable copy, since the atlas is only ever drawn from after this point
        return SKImage.FromBitmap(cornerTemplates);
    }
}
