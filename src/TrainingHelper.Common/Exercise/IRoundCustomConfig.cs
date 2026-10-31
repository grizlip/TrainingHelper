using Avalonia.Controls;

namespace TrainingHelper.Common.Exercise;

public interface IRoundCustomConfig
{
    public Control ConfigurationControl { get; }
    public IEnumerable<string> ExercsieImagesPaths { get; }
}