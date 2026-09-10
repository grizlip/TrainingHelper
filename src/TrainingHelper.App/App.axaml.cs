using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using TrainingHelper.App.ViewModels;
using TrainingHelper.App.Views;
using Microsoft.Extensions.DependencyInjection;
using TrainingHelper.Sdl;
using Microsoft.Extensions.Configuration;
using TrainingHelper.App.Models;
using NLog;
using TrainingHelper.FreeExerciseDb;
using TrainingHelper.Common.Configuration;
using System;
using System.IO;

namespace TrainingHelper.App;

public partial class App : Application
{
    private static IConfigurationRoot? _configurationRoot;
    private static Lazy<string> ConfigDirectory = new Lazy<string>(static () =>
       {
           if (OperatingSystem.IsWindows())
           {
               return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "TraningHelper.App");
           }

           if (OperatingSystem.IsLinux())
           {
               var xdgConfigHome = Environment.GetEnvironmentVariable("XDG_CONFIG_HOME");

               var basePath = !string.IsNullOrWhiteSpace(xdgConfigHome)
                   ? xdgConfigHome
                   : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".config");

               return Path.Combine(basePath, "TraningHelper.App");
           }

           throw new PlatformNotSupportedException();
       });

    private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        try
        {
            if (Directory.Exists(ConfigDirectory.Value) == false)
            {
                Directory.CreateDirectory(ConfigDirectory.Value);
            }
        }
        catch (PlatformNotSupportedException ex)
        {
            _logger.Error(ex, "Failed to create config directory");
        }
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var collection = new ServiceCollection();
        var configurationBuilder = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
        try
        {
            configurationBuilder.AddJsonFile(Path.Combine(ConfigDirectory.Value, "appsettings.json"), optional: true, reloadOnChange: true);
        }
        catch (PlatformNotSupportedException ex)
        {
            _logger.Error(ex, "Failed to add user config file, Only defaults will be used");
        }
        _configurationRoot = configurationBuilder.Build();
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
