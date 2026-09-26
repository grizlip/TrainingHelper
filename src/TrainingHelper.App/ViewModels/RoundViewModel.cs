using System;
using CommunityToolkit.Mvvm.ComponentModel;
using TrainingHelper.Common.Exercise;

namespace TrainingHelper.App.ViewModels;

internal partial class RoundViewModel : ObservableObject, IRoundDefinition
{
    [ObservableProperty]
    private int _roundNumber;

    [ObservableProperty]
    private TimeSpan _interval;

    [ObservableProperty]
    private TimeSpan _break;

    [ObservableProperty]
    private bool _work;

    [ObservableProperty]
    private string _roundImagePath = string.Empty;

    public RoundViewModel(int roundNumber, TimeSpan interval, TimeSpan breakTime, bool work, string roundImagePath)
    {
        _roundNumber = roundNumber;
        _interval = interval;
        _break = breakTime;
        _work = work;
        _roundImagePath = roundImagePath;
    }
}