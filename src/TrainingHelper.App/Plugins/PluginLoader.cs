using System;
using System.Collections.Generic;
using NLog;

namespace TrainingHelper.App.Plugins;

internal class PluginLoader : IPluginLoader
{
    private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

    public IEnumerable<T> LoadPlugins<T>(string pluginPath) where T : class
    {
        var pluginLoadContext = new PluginLoadContext(pluginPath);
        var assembly = pluginLoadContext.LoadFromAssemblyName(new System.Reflection.AssemblyName(System.IO.Path.GetFileNameWithoutExtension(pluginPath)));
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