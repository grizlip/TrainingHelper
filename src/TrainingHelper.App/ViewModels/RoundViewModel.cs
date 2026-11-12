using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using TrainingHelper.App.Models;
using TrainingHelper.Common.Exercise;

namespace TrainingHelper.App.ViewModels;

internal partial class RoundViewModel : ObservableObject, IRoundDefinition
{
    public IEnumerable<string> RoundImagesPaths => _roundCustomConfig?.ExercsieImagesPaths ?? Array.Empty<string>();

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

    [ObservableProperty]
    private ObservableCollection<IRoundCustomConfigFactory> _factories = [];

    [ObservableProperty]
    private IRoundCustomConfigFactory _roundCustomConfigFactory;

    private IRoundCustomConfig? _roundCustomConfig;

    public RoundViewModel(TimeSpan interval, TimeSpan breakTime, bool work, IEnumerable<IRoundCustomConfigFactory> factories)
    {
        _interval = interval;
        _break = breakTime;
        _work = work;
        _factories = new ObservableCollection<IRoundCustomConfigFactory>(factories)
        {
            new NoneRoundCustomConfigFactory()
        };
        RoundCustomConfigFactory = _factories.First(item => item.Id == string.Empty);
    }

    partial void OnRoundCustomConfigFactoryChanging(IRoundCustomConfigFactory value)
    {
        _roundCustomConfig = value.Create();
        CustomConfigControl = _roundCustomConfig?.ConfigurationControl;
    }
}
