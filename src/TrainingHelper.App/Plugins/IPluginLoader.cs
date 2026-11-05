
using System.Collections.Generic;

namespace TrainingHelper.App.Plugins;

internal interface IPluginLoader
{
    IEnumerable<T> LoadPlugins<T>(string pluginPath) where T : class;
}