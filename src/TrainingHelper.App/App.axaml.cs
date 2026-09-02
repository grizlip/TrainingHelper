using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using TrainingHelper.App.ViewModels;
using TrainingHelper.App.Views;
using Microsoft.Extensions.DependencyInjection;
using TrainingHelper.Sdl;
using Microsoft.Extensions.Configuration;
using TrainingHelper.App.Configuration;
using TrainingHelper.App.Models;
using NLog;
using TrainingHelper.FreeExerciseDb;
using TrainingHelper.Common.Configuration;

namespace TrainingHelper.App;

public partial class App : Application
{
    private static IConfigurationRoot? _configurationRoot;
    private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }


    public static void UpdateDefaultAudioDeviceSelection(string defaultAudioDevice)
    {
        if (_configurationRoot != null)
        {
            _configurationRoot.UpdateConfiguration(defaultAudioDevice);
        }
        else
        {
            _logger.Warn("ConfigurationRoot is null. Configuration will not be updated.");
        }
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var collection = new ServiceCollection();
        _configurationRoot = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: false, reloadOnChange: true).Build();
        collection.AddSingleton<IConfiguration>(_configurationRoot);
        collection.Configure<Config>(_configurationRoot);

        collection.AddSdlSound(_configurationRoot.Get<Config>()!);
        collection.AddFreeExercsieDbExercsieProvider(_configurationRoot.Get<Config>()!);

        collection.AddSingleton<MainModel>();
        collection.AddSingleton<MainWindowViewModel>();
        var services = collection.BuildServiceProvider();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = services.GetRequiredService<MainWindowViewModel>()
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
