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
using NLog;
using System.Collections.Generic;
using System;

namespace TrainingHelper.App;

public partial class App : Application
{
    private readonly JsonSettingsService<AppSettings> _jsonSettingsService;
    private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();
    private readonly List<IPlugin> plugins = [];

    public App()
    {
        _logger.Trace($"===App started at {DateTime.Now}===");
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

        foreach (var factory in loader.LoadPlugins<IRoundCustomConfigFactory>())
        {
            if (factory.Activate())
            {
                collection.AddSingleton(factory);
                plugins.Add(factory);
            }
            else
            {
                _logger.Warn($"Failed to activate plugin: {factory.GetType().Name}");
            }
        }

        collection.AddSingleton<ISettingsService<AppSettings>>(_jsonSettingsService);
        collection.AddSdlSound();
        collection.AddSingleton<MainModel>();
        collection.AddSingleton<MainWindowViewModel>();
        var services = collection.BuildServiceProvider();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = services.GetRequiredService<MainWindowViewModel>()
            };
            desktop.Exit += OnExit;
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void OnExit(object? sender, ControlledApplicationLifetimeExitEventArgs e)
    {
        _logger.Trace($"===App exited at {DateTime.Now}===");
        foreach (var plugin in plugins)
        {
            if (!plugin.Deactivate())
            {
                _logger.Warn($"Failed to deactivate plugin: {plugin.GetType().Name}");
            }
        }
    }
}
