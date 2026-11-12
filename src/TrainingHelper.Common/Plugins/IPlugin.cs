namespace TrainingHelper.Common.Plugins;

public interface IPlugin
{
    string Id { get; }
    string Description { get; }
    bool Activate();
    bool Deactivate();
}