// Derived from Microsoft PowerToys.
// https://github.com/microsoft/PowerToys
//
// Original notice:
// =====
// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
// =====
//
// Substantially modified for FancyMouse.
// See THIRD-PARTY-NOTICES.md for attribution details.

using System.Diagnostics;

namespace FancyMouse.SettingsUI.Helpers;

/// <summary>
/// Local equivalent of PowerToys' <c>SettingsUtils</c> class - only the
/// <see cref="SaveSettings"/> part is ported, since that's all
/// <see cref="ViewModels.FancyMouseViewModel"/> needs from it.
/// </summary>
/// <remarks>
/// PowerToys' version resolves a per-module settings.json path under its own settings folder,
/// whereas this app has exactly one settings file, so the path is fixed at construction instead.
/// </remarks>
public sealed class SettingsUtils
{
    public SettingsUtils(string settingsPath)
    {
        ArgumentException.ThrowIfNullOrEmpty(settingsPath);
        this.SettingsPath = settingsPath;
    }

    private string SettingsPath
    {
        get;
    }

    /// <summary>
    /// Save settings to a json file.
    /// </summary>
    /// <remarks>
    /// Mirrors PowerToys' <c>SettingsUtils.SaveSettings</c>, which logs and swallows any
    /// exception so a failed save never crashes the settings UI (rethrowing only argument
    /// errors in debug builds, since those indicate a programming error rather than an
    /// environmental one). This app has no logger of its own, so failures go to the debug
    /// output instead.
    /// </remarks>
    public void SaveSettings(string jsonSettings)
    {
        try
        {
            if (jsonSettings != null)
            {
                File.WriteAllText(this.SettingsPath, jsonSettings);
            }
        }
        catch (Exception e)
        {
            Debug.WriteLine($"Exception encountered while saving settings to '{this.SettingsPath}': {e}");
#if DEBUG
            if (e is ArgumentException || e is ArgumentNullException || e is PathTooLongException)
            {
                throw;
            }
#endif
        }
    }
}
