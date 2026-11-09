namespace TrainingHelper.Common.Plugins;

public interface IPluginLoader
{
    IEnumerable<T> LoadPlugins<T>() where T : IPlugin;
}