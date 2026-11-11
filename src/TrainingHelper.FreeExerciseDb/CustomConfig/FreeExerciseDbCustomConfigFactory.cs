using System.Reflection;
using TrainingHelper.Common.Exercise;
using TrainingHelper.Common.Settings;
using TrainingHelper.Common.Settings.JSON;
using TrainingHelper.FreeExerciseDb.GUI.Models;
using TrainingHelper.FreeExerciseDb.Settings;

namespace TrainingHelper.FreeExerciseDb.CustomConfig;

internal class FreeExerciseDbCustomConfigFactory : IRoundCustomConfigFactory
{
    private readonly ConfigureExerciseModel _model;

    public FreeExerciseDbCustomConfigFactory()
    {
        var assemblyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        ISettingsService<FreeExerciseDbConfig> settingsService = new JsonSettingsService<FreeExerciseDbConfig>(assemblyDir);
        _model = new ConfigureExerciseModel(settingsService);   
    }

    public FreeExerciseDbCustomConfigFactory(ISettingsService<FreeExerciseDbConfig> settingsService)
    {
        _model = new ConfigureExerciseModel(settingsService);
    }

    public bool Activate() => true;

    public IRoundCustomConfig Create() => new FreeExerciseDbCustomConfig(_model);

    public bool Deactivate() => true;
}