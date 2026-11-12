using NLog;
using TrainingHelper.Common.Settings.JSON;

namespace TrainingHelper.Common.Plugins;

public class PluginLoader : IPluginLoader
{
    private readonly Dictionary<string, JsonSettingsService<PluginManifest>> _availableManifests = new Dictionary<string, JsonSettingsService<PluginManifest>>();
    private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();
    
    public PluginLoader(string pluginsDirectory)
    {
        if (string.IsNullOrEmpty(pluginsDirectory))
        {
            throw new ArgumentException("Plugins directory cannot be null or empty.", nameof(pluginsDirectory));
        }

        if (!Directory.Exists(pluginsDirectory))
        {
            throw new DirectoryNotFoundException($"The specified plugins directory does not exist: {pluginsDirectory}");
        }
    
        foreach (var pluginDir in Directory.EnumerateDirectories(pluginsDirectory))
        {
            if (string.IsNullOrEmpty(pluginDir))
            {
                _logger.Warn($"Plugin directory is empty or null: {pluginDir}");
                continue;
            }

            if (File.Exists(Path.Combine(pluginDir, $"{nameof(PluginManifest)}.json")))
            {
                var manifestSettings = new JsonSettingsService<PluginManifest>(Path.Combine(pluginDir));

                var pluginPath = Path.Combine(pluginDir, manifestSettings.Load().EntryAssembly);
                if (File.Exists(pluginPath))
                {
                    _availableManifests[pluginPath] = manifestSettings;
                }
                else
                {
                    _logger.Warn($"Plugin entry assembly does not exist: {pluginPath}");
                }
            }
            else
            {
                _logger.Warn($"Plugin directory does not contain a {nameof(PluginManifest)} file: {pluginDir}");
            }
        }
    }

    public IEnumerable<T> LoadPlugins<T>() where T : IPlugin
    {
        var ids = new List<string>();
        foreach (var pluginPath in _availableManifests.Keys)
        {
            foreach (var plugin in LoadPlugins<T>(pluginPath))
            {
                if(ids.Contains(plugin.Id))
                {
                    _logger.Warn($"Duplicate plugin ID detected: {plugin.Id}. Skipping this plugin.");
                    continue;
                }
                ids.Add(plugin.Id);
                yield return plugin;
            }
        }
    }

    private IEnumerable<T> LoadPlugins<T>(string pluginPath) where T : IPlugin
    {
        var pluginLoadContext = new PluginLoadContext(pluginPath);
        var assembly = pluginLoadContext.LoadFromAssemblyName(new System.Reflection.AssemblyName(Path.GetFileNameWithoutExtension(pluginPath)));
        foreach (var type in assembly.GetTypes())
        {
            if (typeof(T).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract)
            {
                if (Activator.CreateInstance(type) is T instance)
                {
                    yield return instance;
                }
                else
                {
                    _logger.Warn($"Failed to create an instance of type {type.FullName}.");
                }
            }
        }
    }
}