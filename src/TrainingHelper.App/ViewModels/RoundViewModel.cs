using System;
using CommunityToolkit.Mvvm.ComponentModel;
using TrainingHelper.Common.Exercise;

namespace TrainingHelper.App.ViewModels;

internal partial class RoundViewModel : ObservableObject, IRoundDefinition
{
    [ObservableProperty]
    private TimeSpan _interval;

    [ObservableProperty]
    private TimeSpan _break;

    [ObservableProperty]
    private bool _work;

    [ObservableProperty]
    private int _repetitions = 1;

    [ObservableProperty]
    private string _roundImagePath = string.Empty;

    public RoundViewModel(TimeSpan interval, TimeSpan breakTime, bool work, string roundImagePath)
    {
        _interval = interval;
        _break = breakTime;
        _work = work;
        _roundImagePath = roundImagePath;
    }
}