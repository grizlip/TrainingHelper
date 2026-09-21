using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NLog;
using TrainingHelper.App.Settings;
using TrainingHelper.App.Models;
using TrainingHelper.Common.Settings;

namespace TrainingHelper.App.ViewModels;

internal partial class MainWindowViewModel : ObservableObject
{
    [ObservableProperty]
    private TimeSpan _interval = TimeSpan.FromSeconds(5);

    [ObservableProperty]
    private TimeSpan _break = TimeSpan.FromSeconds(3);

    [ObservableProperty]
    private int _roundsCount = 1;

    [ObservableProperty]
    private int _secondsPassed = 0;

    [ObservableProperty]
    private int _totalSeconds = 0;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(PauseResumeCountdownCommand))]
    [NotifyCanExecuteChangedFor(nameof(StopCountdownCommand))]
    private bool _isCountdownInProgress = false;

    [ObservableProperty]
    private int _currentRound = 1;

    [ObservableProperty]
    private bool _work = true;

    [ObservableProperty]
    private Bitmap? _photo;

    [ObservableProperty]
    private bool _showImages;

    [ObservableProperty]
    private bool _hasAudioDevices;

    [ObservableProperty]
    private ObservableCollection<string> _audioDevices = new();

    [ObservableProperty]
    private string? _selectedAudioDevice;
    private readonly MainModel _model;
    private readonly AppSettings _settings;
    private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

    public MainWindowViewModel(MainModel model, ISettingsService<AppSettings> settingsService)
    {
        _logger.Trace($"===App started at {DateTime.Now}===");
        _settings = settingsService.Load();
        ShowImages = _settings.Pictures.Any();
        _model = model;
        _model.CountdownStateChanged += OnCountdownStateChanged;
        Interval = _settings.WorkTime;
        Break = _settings.BreakTime;

        AudioDevices = new ObservableCollection<string>(_model.AudioDevices);
        HasAudioDevices = AudioDevices.Count > 0;
        if (HasAudioDevices)
        {
            SelectedAudioDevice = model.SelectedAudioDevice;
        }
    }

    partial void OnSelectedAudioDeviceChanged(string? value)
    {
        if (!string.IsNullOrEmpty(value) && AudioDevices.Contains(value))
        {
            _model.ChangeSelectedAudioDevice(value);
        }
        else
        {
            _logger.Warn($"Wrong value of requested audio device: {value}");
        }
    }

    partial void OnIntervalChanged(TimeSpan value)
    {
        TotalSeconds = (int)value.TotalSeconds;
    }

    private void OnCountdownStateChanged(object? sender, (int SecondsPassed, int TotalSeconds, int CurrentRound, bool Work, string ImagPath) e)
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
        CurrentRound = e.CurrentRound + 1;
    }

    [RelayCommand(CanExecute = nameof(IsCountdownInProgress))]
    private async Task StopCountdown()
    {
        await _model.StopCountdownAsync();
        UpdateGuiStateAfterStopCountdown();
    }

    [RelayCommand()]
    private async Task StartCountdown()
    {
        await StopCountdown();
        IsCountdownInProgress = true;
        await _model.StartCountdownAsync(Interval, Break, RoundsCount, _settings.Pictures);
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
        TotalSeconds = (int)Interval.TotalSeconds;
        IsCountdownInProgress = false;
        CurrentRound = 1;
        Work = true;
        Photo?.Dispose();
        Photo = null;
    }
}
