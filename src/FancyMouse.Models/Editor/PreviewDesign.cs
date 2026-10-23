using System.Collections.ObjectModel;

using FancyMouse.Models.Display;
using FancyMouse.Models.Drawing;
using FancyMouse.Models.Styles;

namespace FancyMouse.Models.Editor;

/// <summary>
/// The screens being placed on a preview - their positions, sizes, stacking order and selection,
/// and the rules that keep them inside the preview's content area.
/// </summary>
/// <remarks>
/// Positions are in preview pixels (see <see cref="EditableScreen"/>), so the preview's own frame -
/// <see cref="PreviewBounds"/>, from the style's canvas size, margin, border and padding - is
/// exactly what the preview draws. A host that restores saved placements will need to fit them
/// into <see cref="ContentBounds"/> first, since the style may have changed since they were saved.
/// </remarks>
public sealed class PreviewDesign
{
    /// <summary>
    /// The smallest a screen's content area (inside its margin, border and padding) can be
    /// resized to, in preview pixels.
    /// </summary>
    public const double MinimumContentSize = 8;

    /// <summary>
    /// The screens, back to front - a screen's index here is its z-order.
    /// </summary>
    private readonly List<EditableScreen> zOrder;

    public PreviewDesign(IEnumerable<ScreenInfo> screens, PreviewStyle previewStyle)
    {
        ArgumentNullException.ThrowIfNull(screens);
        this.PreviewStyle = previewStyle ?? throw new ArgumentNullException(nameof(previewStyle));
        this.PreviewBounds = BoxBounds.CreateFromOuterBounds(
            outerBounds: new RectangleInfo(previewStyle.CanvasSize),
            boxStyle: previewStyle.CanvasStyle);

        var number = 1;
        this.zOrder = PreviewDesign.GetSystemPlacements(screens.ToList(), this.ContentBounds)
            .Select(placement => new EditableScreen(placement.ScreenInfo, number++, placement.X, placement.Y, placement.Scale))
            .ToList();
        this.Screens = this.zOrder.AsReadOnly();
    }

    public PreviewStyle PreviewStyle
    {
        get;
    }

    /// <summary>
    /// Gets the configured canvas size as a box in preview pixels - its outer bounds are the
    /// largest the preview window can be, and its content bounds are the area screens are confined
    /// to, so there's always room around them for the window's margin, border and padding (see
    /// <see cref="FrameBounds"/>).
    /// </summary>
    public BoxBounds PreviewBounds
    {
        get;
    }

    public RectangleInfo ContentBounds => this.PreviewBounds.ContentBounds;

    /// <summary>
    /// Gets the preview window's actual frame in preview pixels - the canvas style's margin,
    /// border and padding wrapped around the screens' combined bounds, the same way LayoutHelper
    /// shrinks the canvas to fit the screens. Its size is the size the preview window will be,
    /// which is never bigger than the configured canvas size (<see cref="PreviewBounds"/>), and is
    /// smaller if the screens have been arranged to take up less room.
    /// </summary>
    public BoxBounds FrameBounds
    {
        get
        {
            if (this.zOrder.Count == 0)
            {
                return this.PreviewBounds;
            }

            var left = this.zOrder.Min(screen => screen.X);
            var top = this.zOrder.Min(screen => screen.Y);
            var right = this.zOrder.Max(screen => screen.X + screen.Width);
            var bottom = this.zOrder.Max(screen => screen.Y + screen.Height);
            return BoxBounds.CreateFromContentBounds(
                contentBounds: new RectangleInfo((decimal)left, (decimal)top, (decimal)(right - left), (decimal)(bottom - top)),
                boxStyle: this.PreviewStyle.CanvasStyle);
        }
    }

    public BoxStyle ScreenStyle => this.PreviewStyle.ScreenStyle;

    /// <summary>
    /// Gets the screens, back to front.
    /// </summary>
    public ReadOnlyCollection<EditableScreen> Screens
    {
        get;
    }

    /// <summary>
    /// Gets the currently selected screen, if any - the one the z-order and reset commands act on.
    /// </summary>
    public EditableScreen? SelectedScreen
    {
        get;
        private set;
    }

    /// <summary>
    /// Selects <paramref name="screen"/> (or nothing), and returns whether the selection changed.
    /// </summary>
    public bool Select(EditableScreen? screen)
    {
        if ((screen is not null) && !this.zOrder.Contains(screen))
        {
            throw new ArgumentException("Screen is not part of this design.", nameof(screen));
        }

        if (screen == this.SelectedScreen)
        {
            return false;
        }

        this.SelectedScreen = screen;
        return true;
    }

    public void BringToFront()
        => this.MoveSelectedInZOrder(index => this.zOrder.Count - 1);

    public void BringForward()
        => this.MoveSelectedInZOrder(index => index + 1);

    public void SendBackward()
        => this.MoveSelectedInZOrder(index => index - 1);

    public void SendToBack()
        => this.MoveSelectedInZOrder(index => 0);

    /// <summary>
    /// Puts the selected screen back where the automatic layout has it. Its z-order is left
    /// alone - resetting where a screen is shouldn't silently restack it.
    /// </summary>
    public void ResetSelectedScreen()
        => this.SelectedScreen?.ResetToSystemPosition();

    /// <summary>
    /// Puts every screen back where the automatic layout has it, in its original stacking order -
    /// i.e. the layout the design started with.
    /// </summary>
    public void ResetAllScreens()
    {
        foreach (var screen in this.zOrder)
        {
            screen.ResetToSystemPosition();
        }

        this.zOrder.Sort((left, right) => left.Number.CompareTo(right.Number));
    }

    /// <summary>
    /// Moves <paramref name="screen"/> to (<paramref name="x"/>, <paramref name="y"/>), stopping at
    /// the edges of <see cref="ContentBounds"/> - a screen can't be dragged out of the region the
    /// preview actually draws screens in. A screen bigger than the content area (which resizing
    /// doesn't allow) is pinned to its top-left corner.
    /// </summary>
    public void MoveScreen(EditableScreen screen, double x, double y)
    {
        ArgumentNullException.ThrowIfNull(screen);
        var bounds = this.ContentBounds;
        screen.X = Math.Max((double)bounds.Left, Math.Min(x, (double)bounds.Right - screen.Width));
        screen.Y = Math.Max((double)bounds.Top, Math.Min(y, (double)bounds.Bottom - screen.Height));
    }

    /// <summary>
    /// Gets the corner of <paramref name="screen"/> diagonally opposite <paramref name="corner"/> -
    /// the one that stays put while <paramref name="corner"/> is dragged.
    /// </summary>
    public static (double X, double Y) GetResizeAnchor(EditableScreen screen, ResizeCorner corner)
    {
        ArgumentNullException.ThrowIfNull(screen);
        return (
            corner is ResizeCorner.TopLeft or ResizeCorner.BottomLeft ? screen.X + screen.Width : screen.X,
            corner is ResizeCorner.TopLeft or ResizeCorner.TopRight ? screen.Y + screen.Height : screen.Y);
    }

    /// <summary>
    /// Resizes <paramref name="screen"/> uniformly so the dragged <paramref name="corner"/> follows
    /// the pointer while the opposite corner stays at the anchor - using whichever axis the pointer
    /// has moved furthest along, so the corner tracks the pointer as closely as a locked aspect
    /// ratio allows. The size stops growing when either edge reaches <see cref="ContentBounds"/>,
    /// and stops shrinking when there's no room left inside the screen's own margin, border and
    /// padding (plus <see cref="MinimumContentSize"/>).
    /// </summary>
    public void ResizeScreen(EditableScreen screen, ResizeCorner corner, double anchorX, double anchorY, double pointerX, double pointerY)
    {
        ArgumentNullException.ThrowIfNull(screen);
        var isLeft = corner is ResizeCorner.TopLeft or ResizeCorner.BottomLeft;
        var isTop = corner is ResizeCorner.TopLeft or ResizeCorner.TopRight;

        var bounds = this.ContentBounds;
        var availableWidth = isLeft ? anchorX - (double)bounds.Left : (double)bounds.Right - anchorX;
        var availableHeight = isTop ? anchorY - (double)bounds.Top : (double)bounds.Bottom - anchorY;
        var maximumScale = Math.Max(0, Math.Min(availableWidth / screen.NativeWidth, availableHeight / screen.NativeHeight));

        var style = this.ScreenStyle;
        var chromeWidth = (double)(style.MarginStyle.Horizontal + style.BorderStyle.Horizontal + style.PaddingStyle.Horizontal);
        var chromeHeight = (double)(style.MarginStyle.Vertical + style.BorderStyle.Vertical + style.PaddingStyle.Vertical);
        var minimumScale = Math.Min(
            maximumScale,
            Math.Max(
                (chromeWidth + PreviewDesign.MinimumContentSize) / screen.NativeWidth,
                (chromeHeight + PreviewDesign.MinimumContentSize) / screen.NativeHeight));

        var scale = Math.Max(
            Math.Abs(pointerX - anchorX) / screen.NativeWidth,
            Math.Abs(pointerY - anchorY) / screen.NativeHeight);
        screen.Scale = Math.Clamp(scale, minimumScale, maximumScale);

        screen.X = isLeft ? anchorX - screen.Width : anchorX;
        screen.Y = isTop ? anchorY - screen.Height : anchorY;
    }

    /// <summary>
    /// Works out where the automatic layout puts each screen - the desktop's combined bounds
    /// scaled to fit <paramref name="contentBounds"/> and centred in it, with every screen's
    /// outer box scaled by the same factor, as LayoutHelper does for a single device.
    /// </summary>
    private static IEnumerable<(ScreenInfo ScreenInfo, double X, double Y, double Scale)> GetSystemPlacements(
        List<ScreenInfo> screenInfos, RectangleInfo contentBounds)
    {
        if (screenInfos.Count == 0)
        {
            yield break;
        }

        var desktop = RectangleInfo.Union(screenInfos.Select(screenInfo => screenInfo.DisplayArea));
        var scale = Math.Min(
            (double)contentBounds.Width / (double)desktop.Width,
            (double)contentBounds.Height / (double)desktop.Height);
        var originX = (double)contentBounds.X + (((double)contentBounds.Width - ((double)desktop.Width * scale)) / 2);
        var originY = (double)contentBounds.Y + (((double)contentBounds.Height - ((double)desktop.Height * scale)) / 2);

        foreach (var screenInfo in screenInfos)
        {
            yield return (
                screenInfo,
                originX + ((double)(screenInfo.DisplayArea.X - desktop.X) * scale),
                originY + ((double)(screenInfo.DisplayArea.Y - desktop.Y) * scale),
                scale);
        }
    }

    private void MoveSelectedInZOrder(Func<int, int> getNewIndex)
    {
        if (this.SelectedScreen is null)
        {
            return;
        }

        var oldIndex = this.zOrder.IndexOf(this.SelectedScreen);
        var newIndex = Math.Clamp(getNewIndex(oldIndex), 0, this.zOrder.Count - 1);
        if (newIndex == oldIndex)
        {
            return;
        }

        this.zOrder.RemoveAt(oldIndex);
        this.zOrder.Insert(newIndex, this.SelectedScreen);
    }
}
