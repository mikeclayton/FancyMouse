using System.Text.Json.Nodes;

namespace FancyMouse.Settings;

public static class AppSettingsReader
{
    public static AppSettings ReadFile(string filename)
    {
        // determine the version of the config file so we know which converter to use
        var configJson = File.ReadAllText(filename);
        return AppSettingsReader.ParseJson(configJson);
    }

    public static AppSettings ParseJson(string? configJson)
    {
        if (configJson is null)
        {
            return AppSettings.DefaultSettings;
        }

        // determine the version of the config file so we know which converter to use
        int configVersion;
        try
        {
            var configNode = JsonNode.Parse(configJson);

            // if the version isn't specified we'll default to v1
            configVersion = configNode?["version"]?.GetValue<int>() ?? 1;
        }
        catch
        {
            return AppSettings.DefaultSettings;
        }

        var appSettings = configVersion switch
        {
            1 => V1.SettingsConverter.ParseAppSettings(configJson),
            2 => V2.SettingsConverter.ParseAppSettings(configJson),
            _ => AppSettings.DefaultSettings,
        };
        return appSettings;
    }
}
