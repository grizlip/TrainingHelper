using System.Text.Json;
using NLog;

namespace TrainingHelper.Common.Settings.JSON;

public sealed class JsonSettingsService<TSettings> : ISettingsService<TSettings> where TSettings : class, new()
{
    private readonly string _filePath;
    private readonly JsonSerializerOptions _jsonOptions;
    private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

    public JsonSettingsService()
    {
        if (Directory.Exists(ConfigDirectory.Value) == false)
        {
            Directory.CreateDirectory(ConfigDirectory.Value);
        }
        
        _filePath = Path.Combine(ConfigDirectory.Value, $"{typeof(TSettings).Name}.json");

        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true
        };
    }

    public TSettings Load()
    {
        if (!File.Exists(_filePath))
        {
            _logger.Info("Settings file {FilePath} does not exist. Creating new settings.", _filePath);
            return new TSettings();
        }
        try
        {
            var json = File.ReadAllText(_filePath);

            return JsonSerializer.Deserialize<TSettings>(json, _jsonOptions) ?? new TSettings();
        }
        catch (JsonException ex)
        {
            _logger.Error(ex, "Failed to deserialize settings from {FilePath}. Returning default settings.", _filePath);
            return new TSettings();
        }
    }

    public void Save(TSettings settings)
    {
        try
        {
            var directory = Path.GetDirectoryName(_filePath)!;

            Directory.CreateDirectory(directory);

            var json = JsonSerializer.Serialize(settings, _jsonOptions);

            File.WriteAllText(_filePath, json);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to save settings to {FilePath}.", _filePath);
        }
    }

    private static Lazy<string> ConfigDirectory = new Lazy<string>(static () =>
        {
            if (OperatingSystem.IsWindows())
            {
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "TraningHelper.App");
            }

            if (OperatingSystem.IsLinux())
            {
                var xdgConfigHome = Environment.GetEnvironmentVariable("XDG_CONFIG_HOME");

                var basePath = !string.IsNullOrWhiteSpace(xdgConfigHome)
                    ? xdgConfigHome
                    : Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

                return Path.Combine(basePath, "TraningHelper.App");
            }

            return "UserConfig";
        });
}
