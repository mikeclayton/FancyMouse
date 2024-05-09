using FancyMouse.Common.Models.Drawing;
using FancyMouse.Common.Models.Layout;
using FancyMouse.Common.Models.Styles;

namespace FancyMouse.Common.Helpers;

internal static class LayoutHelper
{
    public static PreviewLayout GetPreviewLayout(
        PreviewStyle previewStyle, List<RectangleInfo> screens, PointInfo activatedLocation)
    {
        ArgumentNullException.ThrowIfNull(previewStyle);
        ArgumentNullException.ThrowIfNull(screens);

        if (screens.Count == 0)
        {
            throw new ArgumentException("Value must contain at least one item.", nameof(screens));
        }

        var builder = new PreviewLayout.Builder();
        builder.Screens = screens.ToList();

        // calculate the bounding rectangle for the virtual screen
        builder.VirtualScreen = LayoutHelper.GetCombinedScreenBounds(builder.Screens);

        // find the screen that contains the activated location - this is the
        // one we'll show the preview form on
        var activatedScreen = builder.Screens.Single(
            screen => screen.Contains(activatedLocation));
        builder.ActivatedScreenIndex = builder.Screens.IndexOf(activatedScreen);

        // work out the maximum *constrained* form size
        // * can't be bigger than the activated screen
        // * can't be bigger than the max form size
        var maxPreviewSize = activatedScreen.Size
            .Intersect(previewStyle.CanvasSize);

        // the drawing area for screenshots is inside the
        // preview border and inside the preview padding (if any)
        var maxContentSize = maxPreviewSize
            .Shrink(previewStyle.CanvasStyle.MarginStyle)
            .Shrink(previewStyle.CanvasStyle.BorderStyle)
            .Shrink(previewStyle.CanvasStyle.PaddingStyle);

        // scale the virtual screen to fit inside the content area
        var screenScalingRatio = builder.VirtualScreen.Size
            .ScaleToFitRatio(maxContentSize);

        // position the drawing area on the preview image, offset to
        // allow for any borders and padding
        var contentBounds = builder.VirtualScreen.Size
            .Scale(screenScalingRatio)
            .Floor()
            .PlaceAt(0, 0)
            .Offset(previewStyle.CanvasStyle.MarginStyle.Left, previewStyle.CanvasStyle.MarginStyle.Top)
            .Offset(previewStyle.CanvasStyle.BorderStyle.Left, previewStyle.CanvasStyle.BorderStyle.Top)
            .Offset(previewStyle.CanvasStyle.PaddingStyle.Left, previewStyle.CanvasStyle.PaddingStyle.Top);

        // now we know the size of the content area we can work out the background bounds
        builder.PreviewStyle = previewStyle;
        builder.PreviewBounds = LayoutHelper.GetBoxBoundsFromContentBounds(
            contentBounds,
            previewStyle.CanvasStyle);

        // ... and the form bounds
        // * center the form to the activated position, but nudge it back
        //   inside the visible area of the activated screen if it falls outside
        var formBounds = builder.PreviewBounds.OuterBounds
            .Center(activatedLocation)
            .Clamp(activatedScreen);
        builder.FormBounds = formBounds;

        // now calculate the positions of each of the screenshot images on the preview
        builder.ScreenshotBounds = builder.Screens
            .Select(
                screen => LayoutHelper.GetBoxBoundsFromOuterBounds(
                    screen
                        .Offset(builder.VirtualScreen.Location.ToSize().Negate())
                        .Scale(screenScalingRatio)
                        .Offset(builder.PreviewBounds.ContentBounds.Location.ToSize())
                        .Truncate(),
                    previewStyle.ScreenStyle))
            .ToList();

        return builder.Build();
    }

    public static RectangleInfo GetCombinedScreenBounds(List<RectangleInfo> screens)
    {
        return screens.Skip(1).Aggregate(
            seed: screens.First(),
            (bounds, screen) => bounds.Union(screen));
    }

    public static BoxBounds GetBoxBoundsFromContentBounds(
        RectangleInfo contentBounds,
        BoxStyle boxStyle)
    {
        var paddingBounds = contentBounds.Enlarge(
            boxStyle.PaddingStyle ?? throw new ArgumentException(nameof(boxStyle.PaddingStyle)));
        var borderBounds = paddingBounds.Enlarge(
            boxStyle.BorderStyle ?? throw new ArgumentException(nameof(boxStyle.BorderStyle)));
        var marginBounds = borderBounds.Enlarge(
            boxStyle.MarginStyle ?? throw new ArgumentException(nameof(boxStyle.MarginStyle)));
        var outerBounds = marginBounds;
        return new(
            outerBounds, marginBounds, borderBounds, paddingBounds, contentBounds);
    }

    public static BoxBounds GetBoxBoundsFromOuterBounds(
        RectangleInfo outerBounds,
        BoxStyle boxStyle)
    {
        var marginBounds = outerBounds ?? throw new ArgumentNullException(nameof(outerBounds));
        var borderBounds = marginBounds.Shrink(
            boxStyle.MarginStyle ?? throw new ArgumentException(nameof(boxStyle.MarginStyle)));
        var paddingBounds = borderBounds.Shrink(
            boxStyle.BorderStyle ?? throw new ArgumentException(nameof(boxStyle.BorderStyle)));
        var contentBounds = paddingBounds.Shrink(
            boxStyle.PaddingStyle ?? throw new ArgumentException(nameof(boxStyle.PaddingStyle)));
        return new(
            outerBounds, marginBounds, borderBounds, paddingBounds, contentBounds);
    }
}
