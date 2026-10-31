using System;
using System.Collections.Generic;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using TrainingHelper.App.Models;
using TrainingHelper.Common.Exercise;

namespace TrainingHelper.App.ViewModels;

internal partial class RoundViewModel : ObservableObject, IRoundDefinition
{
    public IEnumerable<string> RoundImagesPaths => _roundCustomConfig?.ExercsieImagesPaths ?? new List<string>();

    [ObservableProperty]
    private TimeSpan _interval;

    [ObservableProperty]
    private TimeSpan _break;

    [ObservableProperty]
    private bool _work;

    [ObservableProperty]
    private int _repetitions = 1;

    [ObservableProperty]
    private Control? _customConfigControl;

    private readonly IRoundCustomConfig? _roundCustomConfig;

    public RoundViewModel(TimeSpan interval, TimeSpan breakTime, bool work, IRoundCustomConfig roundCustomConfig)
    {
        _interval = interval;
        _break = breakTime;
        _work = work;
        _roundCustomConfig = roundCustomConfig;
        CustomConfigControl = _roundCustomConfig?.ConfigurationControl;
    }
}