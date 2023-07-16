﻿using System.Text.Json.Serialization;

namespace FancyMouse.Models.Styles;

/// <summary>
/// Represents the margin style for a drawing object.
/// </summary>
public sealed class MarginStyle
{
    public static readonly MarginStyle Empty = new(0);

    public MarginStyle(decimal all)
        : this(all, all, all, all)
    {
    }

    public MarginStyle(decimal left, decimal top, decimal right, decimal bottom)
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

    [JsonIgnore]
    public decimal Horizontal => this.Left + this.Right;

    [JsonIgnore]
    public decimal Vertical => this.Top + this.Bottom;

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
