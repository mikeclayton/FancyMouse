﻿namespace FancyMouse.Models.Styles;

/// <summary>
/// Represents the styles to apply to a simple box-layout based drawing object.
/// </summary>
public sealed class BoxStyle
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

    public static readonly BoxStyle Empty = new BoxStyle(
        MarginStyle.Empty,
        BorderStyle.Empty,
        PaddingStyle.Empty,
        BackgroundStyle.Empty,
        isEmpty: true);

    public BoxStyle(
        MarginStyle marginStyle,
        BorderStyle borderStyle,
        PaddingStyle paddingStyle,
        BackgroundStyle backgroundStyle)
        : this(marginStyle, borderStyle, paddingStyle, backgroundStyle, false)
    {
    }

    private BoxStyle(
        MarginStyle marginStyle,
        BorderStyle borderStyle,
        PaddingStyle paddingStyle,
        BackgroundStyle backgroundStyle,
        bool isEmpty)
    {
        this.MarginStyle = marginStyle ?? throw new ArgumentNullException(nameof(marginStyle));
        this.BorderStyle = borderStyle ?? throw new ArgumentNullException(nameof(borderStyle));
        this.PaddingStyle = paddingStyle ?? throw new ArgumentNullException(nameof(paddingStyle));
        this.BackgroundStyle = backgroundStyle ?? throw new ArgumentNullException(nameof(backgroundStyle));
        this.IsEmpty = isEmpty;
    }

    /// <summary>
    /// Gets the margin style for this layout box.
    /// </summary>
    public MarginStyle MarginStyle
    {
        get;
    }

    /// <summary>
    /// Gets the border style for this layout box.
    /// </summary>
    public BorderStyle BorderStyle
    {
        get;
    }

    /// <summary>
    /// Gets the padding style for this layout box.
    /// </summary>
    public PaddingStyle PaddingStyle
    {
        get;
    }

    /// <summary>
    /// Gets the background fill style for the content area of this layout box.
    /// </summary>
    public BackgroundStyle BackgroundStyle
    {
        get;
    }

    public bool IsEmpty
    {
        get;
    }
}
