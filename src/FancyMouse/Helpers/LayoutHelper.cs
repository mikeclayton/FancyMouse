using FancyMouse.Models.Drawing;
using FancyMouse.Models.Layout;
using FancyMouse.Models.Screen;
using FancyMouse.Models.Styles;

namespace FancyMouse.Helpers;

internal static class LayoutHelper
{
    public static PreviewLayout GetPreviewLayout(
        PreviewStyle previewStyle, IEnumerable<ScreenInfo> screens, PointInfo activatedLocation)
    {
        if (previewStyle is null)
        {
            throw new ArgumentNullException(nameof(previewStyle));
        }

        if (screens is null)
        {
            throw new ArgumentNullException(nameof(screens));
        }

        var allScreens = screens.ToList();
        if (allScreens.Count == 0)
        {
            throw new ArgumentException("Value must contain at least one item.", nameof(screens));
        }

        var builder = new PreviewLayout.Builder();

        // calculate the bounding rectangle for the virtual screen
        var virtualScreen = allScreens.Skip(1).Aggregate(
            seed: allScreens.First().DisplayArea,
            (bounds, screen) => bounds.Union(screen.DisplayArea));
        builder.VirtualScreen = virtualScreen;

        builder.Screens = allScreens;

        // find the screen that contains the activated location - this is the
        // one we'll show the preview form on
        var activatedScreen = allScreens.Single(
            screen => screen.DisplayArea.Contains(activatedLocation));
        builder.ActivatedScreen = activatedScreen;

        // work out the maximum *constrained* form size
        // * can't be bigger than the activated screen
        // * can't be bigger than the max form size
        var maxPreviewSize = activatedScreen.DisplayArea.Size
            .Intersect(previewStyle.CanvasSize);

        // the drawing area for screenshots is inside the
        // preview border and inside the preview padding (if any)
        var maxContentSize = maxPreviewSize
            .Shrink(previewStyle.CanvasStyle.MarginStyle)
            .Shrink(previewStyle.CanvasStyle.BorderStyle)
            .Shrink(previewStyle.CanvasStyle.PaddingStyle);

        // position the drawing area on the preview image, offset to
        // allow for any borders and padding
        var contentBounds = virtualScreen.Size
            .ScaleToFit(maxContentSize)
            .Floor()
            .PlaceAt(0, 0)
            .Offset(previewStyle.CanvasStyle.MarginStyle.Left, previewStyle.CanvasStyle.MarginStyle.Top)
            .Offset(previewStyle.CanvasStyle.BorderStyle.Left, previewStyle.CanvasStyle.BorderStyle.Top)
            .Offset(previewStyle.CanvasStyle.PaddingStyle.Left, previewStyle.CanvasStyle.PaddingStyle.Top);

        // now we know the size of the content area we can work out the background bounds
        builder.PreviewStyle = previewStyle.CanvasStyle;
        builder.PreviewBounds = LayoutHelper.GetBoxBoundsFromContentBounds(
            contentBounds,
            previewStyle.CanvasStyle);

        // ... and the form bounds
        // * center the form to the activated position, but nudge it back
        //   inside the visible area of the activated screen if it falls outside
        var formBounds = builder.PreviewBounds.OuterBounds
            .Center(activatedLocation)
            .Clamp(activatedScreen.DisplayArea);
        builder.FormBounds = formBounds;

        // scale the virtual screen to fit inside the preview content bounds
        var scalingRatio = builder.VirtualScreen.Size
            .ScaleToFitRatio(contentBounds.Size);

        // now calculate the positions of each of the screenshot images on the preview
        builder.ScreenshotStyle = previewStyle.ScreenshotStyle;
        builder.ScreenshotBounds = allScreens
            .Select(
                screen => LayoutHelper.GetBoxBoundsFromOuterBounds(
                    screen.DisplayArea
                        .Offset(virtualScreen.Location.ToSize().Negate())
                        .Scale(scalingRatio)
                        .Offset(builder.PreviewBounds.ContentBounds.Location.ToSize())
                        .Truncate(),
                    previewStyle.ScreenshotStyle))
            .ToList();

        return builder.Build();
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
