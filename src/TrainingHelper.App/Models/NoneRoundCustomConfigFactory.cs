using System.Collections.Generic;
using Avalonia.Controls;
using TrainingHelper.Common.Exercise;

internal class NoneRoundCustomConfigFactory : IRoundCustomConfigFactory
{
    public string Id => string.Empty;
    public string Description => "None";

    public bool Activate() => true;

    public IRoundCustomConfig Create() => new NoneRoundCustomConfig();

    public bool Deactivate() => true;
    private class NoneRoundCustomConfig : IRoundCustomConfig
    {
        public Control ConfigurationControl => new Control();

        public IEnumerable<string> ExercsieImagesPaths => [];
    }
}