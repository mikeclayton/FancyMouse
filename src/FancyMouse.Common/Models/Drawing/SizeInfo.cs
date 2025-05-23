﻿using System.Drawing;
using FancyMouse.Common.Models.Styles;
using BorderStyle = FancyMouse.Common.Models.Styles.BorderStyle;

namespace FancyMouse.Common.Models.Drawing;

/// <summary>
/// Immutable version of a System.Drawing.Size object with some extra utility methods.
/// </summary>
public sealed record SizeInfo
{
    public SizeInfo(decimal width, decimal height)
    {
        this.Width = width;
        this.Height = height;
    }

    public SizeInfo(Size size)
        : this(size.Width, size.Height)
    {
    }

    public decimal Width
    {
        get;
        init;
    }

    public decimal Height
    {
        get;
        init;
    }

    public SizeInfo Clamp(SizeInfo max)
    {
        return new(
            width: Math.Clamp(this.Width, 0, max.Width),
            height: Math.Clamp(this.Height, 0, max.Height));
    }

    public SizeInfo Enlarge(BorderStyle border) =>
        new(
            this.Width + border.Horizontal,
            this.Height + border.Vertical);

    public SizeInfo Enlarge(PaddingStyle padding) =>
        new(
            this.Width + padding.Horizontal,
            this.Height + padding.Vertical);

    /// <summary>
    /// Rounds down the width and height of this size to the nearest whole number.
    /// </summary>
    /// <returns>A new <see cref="SizeInfo"/> instance with floored dimensions.</returns>
    public SizeInfo Floor()
    {
        return new SizeInfo(
            Math.Floor(this.Width),
            Math.Floor(this.Height));
    }

    /// <summary>
    /// Calculates the intersection of this size with another size, resulting in a size that represents
    /// the overlapping dimensions. Both sizes must be non-negative.
    /// </summary>
    /// <param name="size">The size to intersect with this instance.</param>
    /// <returns>A new <see cref="SizeInfo"/> instance representing the intersection of the two sizes.</returns>
    /// <exception cref="ArgumentException">Thrown when either this size or the specified size has negative dimensions.</exception>
    public SizeInfo Intersect(SizeInfo size)
    {
        if ((this.Width < 0) || (this.Height < 0) || (size.Width < 0) || (size.Height < 0))
        {
            throw new ArgumentException("Sizes must be non-negative");
        }

        return new(
            Math.Min(this.Width, size.Width),
            Math.Min(this.Height, size.Height));
    }

    /// <summary>
    /// Creates a new <see cref="SizeInfo"/> instance with the width and height negated, effectively inverting its dimensions.
    /// </summary>
    /// <returns>A new <see cref="SizeInfo"/> instance with inverted dimensions.</returns>
    public SizeInfo Invert() =>
        new(-this.Width, -this.Height);

    /// <summary>
    /// Creates a new <see cref="RectangleInfo"/> instance representing a rectangle with this size,
    /// positioned at the specified coordinates.
    /// </summary>
    /// <param name="x">The x-coordinate of the upper-left corner of the rectangle.</param>
    /// <param name="y">The y-coordinate of the upper-left corner of the rectangle.</param>
    /// <returns>A new <see cref="RectangleInfo"/> instance representing the positioned rectangle.</returns>
    public RectangleInfo MoveTo(decimal x, decimal y) =>
        new(x, y, this.Width, this.Height);

    public SizeInfo Round() =>
        this.Round(0);

    public SizeInfo Round(int decimals) => new(
        Math.Round(this.Width, decimals),
        Math.Round(this.Height, decimals));

    public SizeInfo Scale(decimal scalingFactor) => new(
        this.Width * scalingFactor,
        this.Height * scalingFactor);

    /// <summary>
    /// Scales this size to fit within the bounds of another size, while maintaining the aspect ratio.
    /// </summary>
    /// <param name="bounds">The size to fit this size into.</param>
    /// <returns>A new <see cref="SizeInfo"/> instance representing the scaled size.</returns>
    public SizeInfo ScaleToFit(SizeInfo bounds)
    {
        return this.ScaleToFit(bounds, out var _);
    }

    /// <summary>
    /// Scales this size to fit within the bounds of another size, while maintaining the aspect ratio.
    /// </summary>
    /// <param name="bounds">The size to fit this size into.</param>
    /// <returns>A new <see cref="SizeInfo"/> instance representing the scaled size.</returns>
    public SizeInfo ScaleToFit(SizeInfo bounds, out decimal scalingFactor)
    {
        var widthRatio = bounds.Width / this.Width;
        var heightRatio = bounds.Height / this.Height;
        switch (widthRatio.CompareTo(heightRatio))
        {
            case < 0:
                scalingFactor = widthRatio;
                return new(bounds.Width, this.Height * widthRatio);
            case 0:
                // widthRatio and heightRatio are the same, so just pick one
                scalingFactor = widthRatio;
                return bounds;
            case > 0:
                scalingFactor = heightRatio;
                return new(this.Width * heightRatio, bounds.Height);
        }
    }

    /// <summary>
    /// Calculates the scaling ratio needed to fit this size within the bounds of another size without distorting the aspect ratio.
    /// </summary>
    /// <param name="bounds">The size to fit this size into.</param>
    /// <returns>The scaling ratio as a decimal.</returns>
    /// <exception cref="ArgumentException">Thrown if the width or height of the bounds is zero.</exception>
    public decimal ScaleToFitRatio(SizeInfo bounds)
    {
        if (bounds.Width == 0 || bounds.Height == 0)
        {
            throw new ArgumentException($"{nameof(bounds.Width)} or {nameof(bounds.Height)} cannot be zero", nameof(bounds));
        }

        var widthRatio = bounds.Width / this.Width;
        var heightRatio = bounds.Height / this.Height;
        var scalingRatio = Math.Min(widthRatio, heightRatio);

        return scalingRatio;
    }

    public SizeInfo Shrink(BorderStyle border) =>
        new(this.Width - border.Horizontal, this.Height - border.Vertical);

    public SizeInfo Shrink(MarginStyle margin) =>
        new(this.Width - margin.Horizontal, this.Height - margin.Vertical);

    public SizeInfo Shrink(PaddingStyle padding) =>
        new(this.Width - padding.Horizontal, this.Height - padding.Vertical);

    public Size ToSize() => new((int)this.Width, (int)this.Height);

    public Point ToPoint() => new((int)this.Width, (int)this.Height);

    public override string ToString()
    {
        return "{" +
            $"{nameof(this.Width)}={this.Width}," +
            $"{nameof(this.Height)}={this.Height}" +
            "}";
    }
}
