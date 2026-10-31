using TrainingHelper.Common.Exercise;
using TrainingHelper.Common.Settings;
using TrainingHelper.FreeExerciseDb.GUI.Models;
using TrainingHelper.FreeExerciseDb.Settings;

namespace TrainingHelper.FreeExerciseDb.CustomConfig;

internal class FreeExerciseDbCustomConfigFactory : IRoundCustomConfigFactory
{
    private readonly ConfigureExerciseModel _model;
    public FreeExerciseDbCustomConfigFactory(ISettingsService<FreeExerciseDbConfig> settingsService)
    {
        _model = new ConfigureExerciseModel(settingsService);
    }

    public IRoundCustomConfig Create() => new FreeExerciseDbCustomConfig(_model);
}