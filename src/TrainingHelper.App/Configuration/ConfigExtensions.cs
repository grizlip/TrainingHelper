using System.IO;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using TrainingHelper.Common.Configuration;

namespace TrainingHelper.App.Configuration;

public static class ConfigExtensions
{
    public static void UpdateConfiguration(this IConfiguration configuration, string defaultAudioDevice, string settingsFilePath = "appsettings.json")
    {
        var config = new Config();
        configuration.Bind(config);

        config.DefaultAudioDeviceSelection = defaultAudioDevice;
        var options = new JsonSerializerOptions { WriteIndented = true };
        string updatedJson = JsonSerializer.Serialize(config, options);
        File.WriteAllText(settingsFilePath, updatedJson);
    }
}