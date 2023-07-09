﻿namespace FancyMouse.Models.Drawing;

public sealed class BoxBounds
{
    /*

    see https://www.w3schools.com/css/css_boxmodel.asp

    +--------------[bounds]---------------+
    |▒▒▒▒▒▒▒▒▒▒▒▒▒▒[margin]▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒|
    |▒▒▓▓▓▓▓▓▓▓▓▓▓▓[border]▓▓▓▓▓▓▓▓▓▓▓▓▓▒▒|
    |▒▒▓▓░░░░░░░░░░[padding]░░░░░░░░░░▓▓▒▒|
    |▒▒▓▓░░                         ░░▓▓▒▒|
    |▒▒▓▓░░                         ░░▓▓▒▒|
    |▒▒▓▓░░        [content]        ░░▓▓▒▒|
    |▒▒▓▓░░                         ░░▓▓▒▒|
    |▒▒▓▓░░                         ░░▓▓▒▒|
    |▒▒▓▓░░░░░░░░░░░░░░░░░░░░░░░░░░░░░▓▓▒▒|
    |▒▒▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▒▒|
    |▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒|
    +-------------------------------------+

    */

    internal BoxBounds(
        RectangleInfo outerBounds,
        RectangleInfo marginBounds,
        RectangleInfo borderBounds,
        RectangleInfo paddingBounds,
        RectangleInfo contentBounds)
    {
        this.OuterBounds = outerBounds ?? throw new ArgumentNullException(nameof(outerBounds));
        this.MarginBounds = marginBounds ?? throw new ArgumentNullException(nameof(marginBounds));
        this.BorderBounds = borderBounds ?? throw new ArgumentNullException(nameof(borderBounds));
        this.PaddingBounds = paddingBounds ?? throw new ArgumentNullException(nameof(paddingBounds));
        this.ContentBounds = contentBounds ?? throw new ArgumentNullException(nameof(contentBounds));
    }

    /// <summary>
    /// Gets the outer bounds of this layout box.
    /// </summary>
    public RectangleInfo OuterBounds
    {
        get;
    }

    public RectangleInfo MarginBounds
    {
        get;
    }

    public RectangleInfo BorderBounds
    {
        get;
    }

    public RectangleInfo PaddingBounds
    {
        get;
    }

    /// <summary>
    /// Gets the content settings for this layout box.
    /// </summary>
    public RectangleInfo ContentBounds
    {
        get;
    }
}
