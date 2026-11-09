namespace TrainingHelper.Common.Plugins;

public interface IPlugin
{
    bool Activate();
    bool Deactivate();
}