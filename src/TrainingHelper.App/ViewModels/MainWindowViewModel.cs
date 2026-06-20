using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TrainingHelper.App.Model;

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
    private double _totalSeconds = 0;

    [ObservableProperty]
    private bool _isEditEnabled = true;

    [ObservableProperty]
    private int _currentRepet = 1;

    [ObservableProperty]
    private bool _work = true;

    private readonly MainModel _model;

    public MainWindowViewModel(MainModel model)
    {
        _model = model;
        _model.CountdownStateChanged += OnCountdownStateChanged;
    }

    private void OnCountdownStateChanged(object? sender, (int SecondsPassed, int TotalSeconds, int CurrentRepet, bool Work) e)
    {
        Work = e.Work;
        TotalSeconds = e.TotalSeconds;
        SecondsPassed = e.SecondsPassed;
        CurrentRepet = e.CurrentRepet + 1;
    }

    [RelayCommand()]
    private async Task StopCountdown()
    {
        await _model.StopCountdownAsync();
        TotalSeconds = Interval.TotalSeconds;
        UpdateGuiStateAfterStopCountdown();
    }

    [RelayCommand()]
    private async Task StartCountdown()
    {
        TotalSeconds = Interval.TotalSeconds;
        SecondsPassed = 0;
        IsEditEnabled = false;
        await _model.StartCountdownAsync(Interval, Break, RepetCount);
        UpdateGuiStateAfterStopCountdown();
    }

    private void UpdateGuiStateAfterStopCountdown()
    {
        SecondsPassed = 0;
        IsEditEnabled = true;
        CurrentRepet = 1;
    }
}
