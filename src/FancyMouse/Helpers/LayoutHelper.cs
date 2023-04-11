using FancyMouse.Models.Drawing;
using FancyMouse.Models.Layout;

namespace FancyMouse.Helpers;

internal class LayoutHelper
{
    public static LayoutInfo CalculateLayoutInfo(
        LayoutConfig layoutConfig)
    {
        if (layoutConfig is null)
        {
            throw new ArgumentNullException(nameof(layoutConfig));
        }

        var builder = new LayoutInfo.Builder
        {
            LayoutConfig = layoutConfig,
        };

        builder.ActivatedScreen = layoutConfig.ScreenBounds[layoutConfig.ActivatedScreenIndex];

        // work out the maximum *constrained* form size
        // * can't be bigger than the activated screen
        // * can't be bigger than the max form size
        var maxFormSize = builder.ActivatedScreen.Size
            .Intersect(layoutConfig.MaximumFormSize);

        // the drawing area for screen images is inside the
        // form border and inside the preview border
        var maxDrawingSize = maxFormSize
            .Shrink(layoutConfig.FormPadding)
            .Shrink(layoutConfig.PreviewPadding);

        // scale the virtual screen to fit inside the drawing bounds
        var scalingRatio = layoutConfig.VirtualScreen.Size
            .ScaleToFitRatio(maxDrawingSize);

        // position the drawing bounds inside the preview border
        var drawingBounds = layoutConfig.VirtualScreen.Size
            .ScaleToFit(maxDrawingSize)
            .PlaceAt(layoutConfig.PreviewPadding.Left, layoutConfig.PreviewPadding.Top);

        // now we know the size of the drawing area we can work out the preview size
        builder.PreviewBounds = drawingBounds.Enlarge(layoutConfig.PreviewPadding);

        // ... and the form size
        // * center the form to the activated position, but nudge it back
        //   inside the visible area of the activated screen if it falls outside
        builder.FormBounds = builder.PreviewBounds.Size
            .PlaceAt(0, 0)
            .Enlarge(layoutConfig.FormPadding)
            .Center(layoutConfig.ActivatedLocation)
            .Clamp(builder.ActivatedScreen);

        // now calculate the positions of each of the screen images on the preview
        builder.ScreenBounds = layoutConfig.ScreenBounds
            .Select(
                screen => screen
                    .Offset(layoutConfig.VirtualScreen.Location.Size.Negate())
                    .Scale(scalingRatio)
                    .Offset(layoutConfig.PreviewPadding.Left, layoutConfig.PreviewPadding.Top))
            .ToList();

        return builder.Build();
    }

    /// <summary>
    /// Resize and position the specified form.
    /// </summary>
    public static void PositionForm(
        Form form, RectangleInfo formBounds)
    {
        // note - do this in two steps rather than "this.Bounds = formBounds" as there
        // appears to be an issue in WinForms with dpi scaling even when using PerMonitorV2,
        // where the form scaling uses either the *primary* screen scaling or the *previous*
        // screen's scaling when the form is moved to a different screen. i've got no idea
        // *why*, but the exact sequence of calls below seems to be a workaround...
        // see https://github.com/mikeclayton/FancyMouse/issues/2
        var bounds = formBounds.ToRectangle();
        form.Location = bounds.Location;
        _ = form.PointToScreen(Point.Empty);
        form.Size = bounds.Size;
    }
}
