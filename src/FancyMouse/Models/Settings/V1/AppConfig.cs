﻿namespace FancyMouse.Models.Settings.V1;

/// <summary>
/// Represents the configuration file format to allow for easier
/// serialization / deserialization. This needs to be converted
/// into an AppSettings object for the main application to use.
/// </summary>
internal sealed class AppConfig
{
    public AppConfig(
        FancyMouseSettings? fancymouse)
    {
        this.FancyMouse = fancymouse;
    }

    public FancyMouseSettings? FancyMouse
    {
        get;
    }
}
