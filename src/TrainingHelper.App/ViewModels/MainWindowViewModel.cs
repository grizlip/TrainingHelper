using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Options;
using TrainingHelper.App.Configuration;
using TrainingHelper.App.Models;

namespace TrainingHelper.App.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    [ObservableProperty]
    private TimeSpan _interval = TimeSpan.FromSeconds(5);

    [ObservableProperty]
    private TimeSpan _break = TimeSpan.FromSeconds(3);

    [ObservableProperty]
    private int _repetCount = 1;

    [ObservableProperty]
    private int _secondsPassed = 0;

    [ObservableProperty]
    private int _totalSeconds = 0;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(PauseResumeCountdownCommand))]
    [NotifyCanExecuteChangedFor(nameof(StopCountdownCommand))]
    private bool _isCountdownInProgress = false;

    [ObservableProperty]
    private int _currentRepet = 1;

    [ObservableProperty]
    private bool _work = true;

    [ObservableProperty]
    private Bitmap? _photo;

    [ObservableProperty]
    private bool _showImages;
    private readonly MainModel _model;
    private readonly IOptions<Config> _configuration;
    public MainWindowViewModel(MainModel model, IOptions<Config> configuration)
    {
        _configuration = configuration;
        ShowImages = _configuration.Value.Pictures.Any();

        _model = model;
        _model.CountdownStateChanged += OnCountdownStateChanged;
        Interval = _configuration.Value.WorkTime;
        Break = _configuration.Value.BreakTime;
    }

    partial void OnIntervalChanged(TimeSpan value)
    {
        TotalSeconds = (int)value.TotalSeconds;
    }

    private void OnCountdownStateChanged(object? sender, (int SecondsPassed, int TotalSeconds, int CurrentRepet, bool Work, string ImagPath) e)
    {
        Work = e.Work;
        if (!string.IsNullOrEmpty(e.ImagPath))
        {
            Photo?.Dispose();
            Photo = null;
            Photo = new Bitmap(e.ImagPath);
        }
        TotalSeconds = e.TotalSeconds;
        SecondsPassed = e.SecondsPassed;
        CurrentRepet = e.CurrentRepet + 1;
    }

    [RelayCommand(CanExecute = nameof(IsCountdownInProgress))]
    private async Task StopCountdown()
    {
        await _model.StopCountdownAsync();
        TotalSeconds = (int)Interval.TotalSeconds;
        UpdateGuiStateAfterStopCountdown();
    }

    [RelayCommand()]
    private async Task StartCountdown()
    {
        TotalSeconds = (int)Interval.TotalSeconds;
        SecondsPassed = 0;
        IsCountdownInProgress = true;
        await _model.StartCountdownAsync(Interval, Break, RepetCount, _configuration.Value.Pictures);
        UpdateGuiStateAfterStopCountdown();
    }

    [RelayCommand(CanExecute = nameof(IsCountdownInProgress))]
    private void PauseResumeCountdown()
    {
        _model.PauseResumeCountdown();
    }
    private void UpdateGuiStateAfterStopCountdown()
    {
        SecondsPassed = 0;
        IsCountdownInProgress = false;
        CurrentRepet = 1;
        Work = true;
        Photo?.Dispose();
        Photo = null;
    }
}
