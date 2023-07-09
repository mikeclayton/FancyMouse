using FancyMouse.Models.Drawing;
using FancyMouse.Models.Layout;
using FancyMouse.Models.Screen;
using FancyMouse.Models.Settings;

namespace FancyMouse.Helpers;

internal static class LayoutHelper
{
    public static PreviewLayout GetPreviewLayout(
        PreviewSettings previewSettings, IEnumerable<ScreenInfo> screens, PointInfo activatedLocation)
    {
        if (previewSettings is null)
        {
            throw new ArgumentNullException(nameof(previewSettings));
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
            seed: allScreens.First().Bounds,
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
            .Intersect(previewSettings.Size);

        // the drawing area for screenshots is inside the
        // preview border and inside the preview padding (if any)
        var maxContentSize = maxPreviewSize
            .Shrink(previewSettings.PreviewStyle.MarginInfo)
            .Shrink(previewSettings.PreviewStyle.BorderInfo)
            .Shrink(previewSettings.PreviewStyle.PaddingInfo);

        // position the drawing area on the preview image, offset to
        // allow for any borders and padding
        var contentBounds = virtualScreen.Size
            .ScaleToFit(maxContentSize)
            .Floor()
            .PlaceAt(0, 0)
            .Offset(previewSettings.PreviewStyle.MarginInfo.Left, previewSettings.PreviewStyle.MarginInfo.Top)
            .Offset(previewSettings.PreviewStyle.BorderInfo.Left, previewSettings.PreviewStyle.BorderInfo.Top)
            .Offset(previewSettings.PreviewStyle.PaddingInfo.Left, previewSettings.PreviewStyle.PaddingInfo.Top);

        // now we know the size of the content area we can work out the background bounds
        builder.PreviewStyle = previewSettings.PreviewStyle;
        builder.PreviewBounds = LayoutHelper.GetBoxBoundsFromContentBounds(
            contentBounds,
            previewSettings.PreviewStyle);

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
        builder.ScreenshotStyle = previewSettings.ScreenshotStyle;
        builder.ScreenshotBounds = allScreens
            .Select(
                screen => LayoutHelper.GetBoxBoundsFromOuterBounds(
                        screen.Bounds
                            .Offset(virtualScreen.Location.ToSize().Negate())
                            .Scale(scalingRatio)
                            .Offset(builder.PreviewBounds.ContentBounds.Location.ToSize()),
                        previewSettings.ScreenshotStyle))
            .ToList();

        return builder.Build();
    }

    public static BoxBounds GetBoxBoundsFromContentBounds(
        RectangleInfo contentBounds,
        BoxStyle boxStyle)
    {
        var paddingBounds = contentBounds.Enlarge(
            boxStyle.PaddingInfo ?? throw new ArgumentException(nameof(boxStyle.PaddingInfo)));
        var borderBounds = paddingBounds.Enlarge(
            boxStyle.BorderInfo ?? throw new ArgumentException(nameof(boxStyle.BorderInfo)));
        var marginBounds = borderBounds.Enlarge(
            boxStyle.MarginInfo ?? throw new ArgumentException(nameof(boxStyle.MarginInfo)));
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
            boxStyle.MarginInfo ?? throw new ArgumentException(nameof(boxStyle.MarginInfo)));
        var paddingBounds = borderBounds.Shrink(
            boxStyle.BorderInfo ?? throw new ArgumentException(nameof(boxStyle.BorderInfo)));
        var contentBounds = paddingBounds.Shrink(
            boxStyle.PaddingInfo ?? throw new ArgumentException(nameof(boxStyle.PaddingInfo)));
        return new(
            outerBounds, marginBounds, borderBounds, paddingBounds, contentBounds);
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
