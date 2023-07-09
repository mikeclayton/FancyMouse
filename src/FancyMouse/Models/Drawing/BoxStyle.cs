namespace FancyMouse.Models.Drawing;

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

    public static readonly BoxStyle Empty = new(MarginInfo.Empty, BorderInfo.Empty, PaddingInfo.Empty, BackgroundInfo.Empty);

    internal BoxStyle(
        MarginInfo marginInfo,
        BorderInfo borderInfo,
        PaddingInfo paddingInfo,
        BackgroundInfo backgroundInfo)
    {
        this.MarginInfo = marginInfo ?? throw new ArgumentNullException(nameof(marginInfo));
        this.BorderInfo = borderInfo ?? throw new ArgumentNullException(nameof(borderInfo));
        this.PaddingInfo = paddingInfo ?? throw new ArgumentNullException(nameof(paddingInfo));
        this.BackgroundInfo = backgroundInfo ?? throw new ArgumentNullException(nameof(backgroundInfo));
    }

    /// <summary>
    /// Gets the margin settings for this layout box.
    /// </summary>
    public MarginInfo MarginInfo
    {
        get;
    }

    /// <summary>
    /// Gets the border settings for this layout box.
    /// </summary>
    public BorderInfo BorderInfo
    {
        get;
    }

    /// <summary>
    /// Gets the padding settings for this layout box.
    /// </summary>
    public PaddingInfo PaddingInfo
    {
        get;
    }

    public BackgroundInfo BackgroundInfo
    {
        get;
    }
}
