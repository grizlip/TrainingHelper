using TrainingHelper.Common.Plugins;

namespace TrainingHelper.Common.Exercise;

public interface IRoundCustomConfigFactory : IPlugin
{
    IRoundCustomConfig Create();
}