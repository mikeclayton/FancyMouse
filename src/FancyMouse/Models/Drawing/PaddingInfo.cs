﻿namespace FancyMouse.Models.Drawing;

/// <summary>
/// Immutable version of a System.Windows.Forms.Padding object with some extra utility methods.
/// </summary>
public sealed class PaddingInfo
{
    public static readonly PaddingInfo Empty = new(0);

    public PaddingInfo(decimal all)
        : this(all, all, all, all)
    {
    }

    public PaddingInfo(decimal left, decimal top, decimal right, decimal bottom)
    {
        this.Left = left;
        this.Top = top;
        this.Right = right;
        this.Bottom = bottom;
    }

    public decimal Left
    {
        get;
    }

    public decimal Top
    {
        get;
    }

    public decimal Right
    {
        get;
    }

    public decimal Bottom
    {
        get;
    }

    public decimal Horizontal => this.Left + this.Right;

    public decimal Vertical => this.Top + this.Bottom;

    public MarginInfo ToMarginInfo()
    {
        return new MarginInfo(this.Left, this.Top, this.Right, this.Bottom);
    }

    public override string ToString()
    {
        return "{" +
            $"{nameof(this.Left)}={this.Left}," +
            $"{nameof(this.Top)}={this.Top}," +
            $"{nameof(this.Right)}={this.Right}," +
            $"{nameof(this.Bottom)}={this.Bottom}" +
            "}";
    }
}
