using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TrainingHelper.App.Model;

namespace TrainingHelper.App.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    [ObservableProperty]
    private TimeSpan _interval = TimeSpan.FromSeconds(30);

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
    private int _currentRepet = 0;

    [ObservableProperty]
    private bool _work = true;

    private readonly MainModel _model;

    public MainWindowViewModel(MainModel model)
    {
        _model = model;
    }

    [RelayCommand()]
    private async Task StartCountdown()
    {
        IsEditEnabled = false;
        for (int i = 0; i < RepetCount; i++)
        {
            Work = true;
            CurrentRepet = i + 1;
            await Countdown(Interval.TotalSeconds);
            Work = false;
            await Countdown(Break.TotalSeconds);
        }

        IsEditEnabled = true;
    }

    private async Task Countdown(double totalSeconds)
    {
        TotalSeconds = totalSeconds;
        SecondsPassed = 0;
        while (SecondsPassed < TotalSeconds)
        {
            await Task.Delay(1000);
            SecondsPassed = SecondsPassed + 1;
        }
    }
}
