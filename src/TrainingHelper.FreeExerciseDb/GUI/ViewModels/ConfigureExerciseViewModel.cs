using System.Collections.ObjectModel;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NLog;
using TrainingHelper.FreeExerciseDb.GUI.Models;

namespace TrainingHelper.FreeExerciseDb.GUI.ViewModels;

internal partial class ConfigureExerciseViewModel : ObservableObject
{
    private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

    [ObservableProperty]
    private ObservableCollection<ExerciseDetails> _exerciseDetails = new ObservableCollection<ExerciseDetails>();

    [ObservableProperty]
    private ExerciseDetails? _selectedExercsie = null;

    [ObservableProperty]
    private ObservableCollection<Bitmap> _images = new ObservableCollection<Bitmap>();

    private readonly ConfigureExerciseModel _model;

    public ConfigureExerciseViewModel(ConfigureExerciseModel model)
    {
        _model = model;
    }

    partial void OnSelectedExercsieChanged(ExerciseDetails? value)
    {
        foreach (var b in Images)
        {
            b.Dispose();
        }
        Images.Clear();
        if (value?.ImagesPaths != null)
        {
            foreach (var path in value.ImagesPaths)
            {
                if (File.Exists(path))
                {
                    _logger.Trace($"Adding {path} image");
                    Images.Add(new Bitmap(path));
                }
                else
                {
                    _logger.Warn($"File {path} does not exists");
                }
            }
        }
        else
        {
            _logger.Warn($"No images found for exercsie {value}");
        }
    }

    [RelayCommand]
    private async Task Loaded()
    {
        var exercsieDetails = await _model.GetAllExerciseDetailsAsync(CancellationToken.None);

        foreach (var a in exercsieDetails)
        {
            ExerciseDetails.Add(a);
        }
    }
}