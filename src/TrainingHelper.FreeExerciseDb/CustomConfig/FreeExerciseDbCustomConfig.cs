using Avalonia.Controls;
using TrainingHelper.Common.Exercise;
using TrainingHelper.FreeExerciseDb.GUI.Models;
using TrainingHelper.FreeExerciseDb.GUI.ViewModels;
using TrainingHelper.FreeExerciseDb.GUI.Views;

namespace TrainingHelper.FreeExerciseDb.CustomConfig;

internal class FreeExerciseDbCustomConfig : IRoundCustomConfig
{
    public Control ConfigurationControl { get; }
    public IEnumerable<string> ExercsieImagesPaths => _configureExerciseViewModel.SelectedExercsie?.ImagesPaths ?? new List<string>();
    
    private readonly ConfigureExerciseViewModel _configureExerciseViewModel;
    private readonly ConfigureExerciseModel _model;
    public FreeExerciseDbCustomConfig(ConfigureExerciseModel model)
    {
        _model = model;
        _configureExerciseViewModel = new ConfigureExerciseViewModel(_model);

        ConfigurationControl = new ConfigureExerciseView
        {
            DataContext = _configureExerciseViewModel
        };
    }
}