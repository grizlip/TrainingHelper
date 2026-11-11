using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using TrainingHelper.App.ViewModels;
using TrainingHelper.App.Views;
using Microsoft.Extensions.DependencyInjection;
using TrainingHelper.Sdl;
using TrainingHelper.App.Models;
using TrainingHelper.App.Settings;
using TrainingHelper.Common.Settings.JSON;
using TrainingHelper.Common.Settings;
using TrainingHelper.Common.Plugins;
using TrainingHelper.Common.Exercise;
using System.Linq;

namespace TrainingHelper.App;

public partial class App : Application
{
    private readonly JsonSettingsService<AppSettings> _jsonSettingsService;

    public App()
    {
        _jsonSettingsService = new JsonSettingsService<AppSettings>();
    }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var collection = new ServiceCollection();
        PluginLoader loader = new PluginLoader(_jsonSettingsService.Load().PluginsPath);
        var customConfigFactories = loader.LoadPlugins<IRoundCustomConfigFactory>().ToList();

        collection.AddSingleton<ISettingsService<AppSettings>>(_jsonSettingsService);

        collection.AddSdlSound();
        collection.AddSingleton(customConfigFactories[0]);
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
